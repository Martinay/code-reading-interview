using ExampleApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/v1.0/Download", (DownloadRequest req) =>
{
    return System.Text.Json.JsonSerializer.Serialize(req);
})
.WithName("Download file");

app.Run();

// neeeded for integration tests
public partial class Program
{

}