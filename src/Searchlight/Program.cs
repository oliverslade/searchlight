var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Searchlight API",
        Version = "v1"
    });
});
builder.Services.AddControllers();

builder.Services.AddTransient<Searchlight.Clients.Interfaces.IWebSocketWrapper, Searchlight.Clients.ClientWebSocketWrapper>();
builder.Services.AddTransient<Searchlight.Services.Interfaces.IMazeSolverService, Searchlight.Services.MazeSolverService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var swaggerJsonPath = Path.Combine(AppContext.BaseDirectory, "swagger.json");

    if (File.Exists(swaggerJsonPath))
    {
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger.json", "Searchlight API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "Searchlight API Documentation";
        });

        app.MapGet("/swagger.json", () => Results.File(swaggerJsonPath, "application/json"));
    }
    else
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Searchlight API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "Searchlight API Documentation";
        });
    }
}

// Add a route for the root URL that redirects to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();
