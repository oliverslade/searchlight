var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // Add controllers

// Register services directly
builder.Services.AddTransient<Searchlight.Clients.Interfaces.IWebSocketWrapper, Searchlight.Clients.ClientWebSocketWrapper>();
builder.Services.AddTransient<Searchlight.Services.Interfaces.IMazeSolverService, Searchlight.Services.MazeSolverService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add a route for the root URL that redirects to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers(); // Map controller routes

app.Run();
