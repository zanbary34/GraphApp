using Microsoft.EntityFrameworkCore;
using ServiceB.Data;
using ServiceB.Services.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Configure database context with PostgreSQL
builder.Services.AddDbContext<GraphContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<WebSocketIncomeHandler>();
builder.Services.AddScoped<IGraphService, GraphService>();

// Configure controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply database migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GraphContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.Migrate();
}

// Configure middlewares
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service B API v1"));
}

app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.MapControllers();

app.Run();
