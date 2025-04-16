using Searchlight.Clients.Interfaces;
using Searchlight.Models;
using Searchlight.Services.Interfaces;

namespace Searchlight.Services
{
    public class MazeSolver : IMazeSolver
    {
        private readonly IMazeClient _mazeClient;
        private readonly Dictionary<string, MazeNode> _graph = new();
        private string _currentId = string.Empty;
        private string _startId = string.Empty;
        private string? _endId = null;
        private int _moves = 0;
        private int _resets = 0;

        public MazeSolver(IMazeClient mazeClient)
        {
            _mazeClient = mazeClient;
        }

        public async Task<SolveResult> SolveAsync()
        {
            await _mazeClient.ConnectAsync();
            var initialLocation = await _mazeClient.ReceiveLocationAsync();

            _startId = initialLocation.Id;
            _currentId = initialLocation.Id;

            _graph[_startId] = new MazeNode
            {
                AvailableDirections = initialLocation.AvailableDirections,
                Data = initialLocation,
                Neighbours = new Dictionary<string, string>()
            };

            var visited = new HashSet<string> { _startId };

            // Stack for iterative DFS: stores { id, parentId, entryDir }
            var stack = new Stack<NodeInfo>();
            stack.Push(new NodeInfo { Id = _startId, ParentId = null, EntryDirection = null });

            while (stack.Count > 0)
            {
                var currentNodeInfo = stack.Peek();
                var currentId = currentNodeInfo.Id;

                // Make sure we're at the correct location in the maze
                if (currentId != _currentId)
                {
                    var recoveryPath = FindShortestPath(_currentId, currentId);
                    if (recoveryPath == null)
                    {
                        throw new InvalidOperationException("DFS state mismatch and cannot find path to recover.");
                    }

                    var recoveryDirections = GetDirectionsForPath(recoveryPath);
                    foreach (var dir in recoveryDirections)
                    {
                        await _mazeClient.MoveAsync(dir);
                        _moves++;
                    }
                }

                var node = _graph[currentId];
                var locationData = node.Data;

                if (locationData.AvailableDirections.Count == 0)
                {
                    _endId = currentId;
                    break;
                }

                // Find the next unexplored direction
                string? unexploredDir = null;
                foreach (var direction in locationData.AvailableDirections)
                {
                    if (!node.Neighbours.ContainsKey(direction))
                    {
                        unexploredDir = direction;
                        break;
                    }
                }

                if (unexploredDir != null)
                {
                    var nextLoc = await _mazeClient.MoveAsync(unexploredDir);
                    _moves++;
                    var nextId = nextLoc.Id;
                    var backDir = GetReverseDirection(unexploredDir);

                    node.Neighbours[unexploredDir] = nextId;
                    _currentId = nextId;

                    if (!_graph.ContainsKey(nextId))
                    {
                        _graph[nextId] = new MazeNode
                        {
                            AvailableDirections = nextLoc.AvailableDirections,
                            Data = nextLoc,
                            Neighbours = new Dictionary<string, string> { { backDir, currentId } }
                        };
                        visited.Add(nextId);
                        stack.Push(new NodeInfo
                        {
                            Id = nextId,
                            ParentId = currentId,
                            EntryDirection = unexploredDir
                        });
                    }
                    else
                    {
                        _graph[nextId].Neighbours[backDir] = currentId;
                        var backtrackLoc = await _mazeClient.MoveAsync(backDir);
                        _moves++;
                        _currentId = backtrackLoc.Id;
                    }
                }
                else
                {
                    var backtrackNodeInfo = stack.Pop();

                    if (stack.Count > 0)
                    {
                        var parentId = backtrackNodeInfo.ParentId;
                        if (parentId != null)
                        {
                            var dirToParent = GetReverseDirection(backtrackNodeInfo.EntryDirection!);
                            if (dirToParent == null)
                            {
                                throw new InvalidOperationException(
                                    $"Cannot determine backtrack direction from {backtrackNodeInfo.Id} to {parentId}");
                            }
                            var backtrackLoc = await _mazeClient.MoveAsync(dirToParent);
                            _moves++;
                            _currentId = backtrackLoc.Id;
                        }
                    }
                }
            }

            if (_endId != null)
            {
                var result = new SolveResult
                {
                    Success = true,
                    StartLocation = _graph[_startId].Data,
                    EndLocation = _graph[_endId].Data,
                    TotalMoves = _moves,
                    TotalResets = _resets,
                    TotalLocationsDiscovered = _graph.Count,
                    Path = BuildSolutionPath()
                };
                return result;
            }
            else
            {
                return new SolveResult
                {
                    Success = false,
                    StartLocation = _graph[_startId].Data,
                    TotalMoves = _moves,
                    TotalResets = _resets,
                    TotalLocationsDiscovered = _graph.Count
                };
            }
        }

        private List<string>? FindShortestPath(string fromId, string toId)
        {
            var queue = new Queue<List<string>>();
            queue.Enqueue(new List<string> { fromId });
            var visited = new HashSet<string> { fromId };

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var lastId = path[^1];

                if (lastId == toId)
                    return path;

                if (!_graph.TryGetValue(lastId, out var node))
                    continue;

                foreach (var kvp in node.Neighbours)
                {
                    var direction = kvp.Key;
                    var neighbourId = kvp.Value;

                    if (!visited.Contains(neighbourId))
                    {
                        visited.Add(neighbourId);
                        var newPath = new List<string>(path) { neighbourId };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return null;
        }

        private List<string> GetDirectionsForPath(List<string> path)
        {
            var directions = new List<string>();

            for (int i = 0; i < path.Count - 1; i++)
            {
                var currentId = path[i];
                var nextId = path[i + 1];
                var node = _graph[currentId];

                foreach (var kvp in node.Neighbours)
                {
                    if (kvp.Value == nextId)
                    {
                        directions.Add(kvp.Key);
                        break;
                    }
                }
            }

            return directions;
        }

        private string GetReverseDirection(string direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentException($"Unknown direction: {direction}")
            };
        }

        private List<MazeLocation> BuildSolutionPath()
        {
            if (_endId == null)
                return new List<MazeLocation>();

            var path = new List<MazeLocation>();
            var idPath = FindShortestPath(_startId, _endId);

            if (idPath == null)
                return new List<MazeLocation>();

            foreach (var id in idPath)
            {
                path.Add(_graph[id].Data);
            }

            return path;
        }
    }
}