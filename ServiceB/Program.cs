using Microsoft.EntityFrameworkCore;
using ServiceB.Data;
using ServiceB.Services.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GraphContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<WebSocketIncomeHandler>();
builder.Services.AddScoped<IGraphService, GraphService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GraphContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.Migrate();
}

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
