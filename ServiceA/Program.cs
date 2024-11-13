using dotenv.net;
using ServiceA.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotEnv.Load();

// Register GenerateJWT as a scoped service to access configuration
builder.Services.AddScoped<GenerateJWT>();

// Register WebSocketSenderHandler as a singleton, with token dependency injection
builder.Services.AddScoped<WebSocketSenderHandler>();

// Add controllers and Swagger for API documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service A API v1"));

// Map controllers
app.MapControllers();

app.Run();
