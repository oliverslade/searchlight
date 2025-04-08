# Searchlight

Searchlight is an API that solves maze puzzles by connecting to a maze-solving service via WebSockets.

## What is Searchlight?

Searchlight is designed to programmatically solve maze puzzles from the Software Delivery Community at Opencast Software. It connects to a maze service, navigates through the maze, and finds the optimal path to the solution.

## How It Works

When provided with a Maze ID, Searchlight:

1. Connects to the maze WebSocket service at `wss://maze.robanderson.dev/ws/{MAZE_ID}`
2. Receives information about the current location in the maze
3. Determines the next direction to move based on solving algorithms
4. Sends commands to navigate through the maze
5. Continues until it reaches the end of the maze

## Maze Specification

### Creating a New Maze

A new maze can be created at https://maze.robanderson.dev/ by clicking the NEW GAME link. Mazes come in three difficulties:

- Small
- Medium
- Large

The maze ID can be found in the URL: `https://maze.robanderson.dev/maze/{MAZE_ID}`

## Using Searchlight

### API Endpoints

- `POST /api/maze/solve/{mazeId}` - Solves the specified maze

## Testing

Run the tests using:

```bash
dotnet test
```
