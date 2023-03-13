using Npgsql.GeoQuery.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGeoQuery();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGeoQuery(app.Configuration.GetConnectionString("Template"));

app.Run();

public partial class Program
{ }