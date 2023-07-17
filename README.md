# npgsql-geo-query

[![Nuget](https://img.shields.io/nuget/v/Npgisql.GeoQuery)](https://www.nuget.org/packages/Npgisql.GeoQuery)
![Nuget](https://img.shields.io/nuget/dt/Npgisql.GeoQuery)

query geo-data : `mvt` `geobuf` `geojson` from postgresql(with postgis)
you can use `GeoQuery` class ,also interface `IGeoQuery` ,last `AddGeoQuery` in IServiceCollection Extension

## Usage

```
dotnet add package Npgisql.GeoQuery
```

### asp net core & dependency injection

```cs
builder.Services.AddGeoQuery();

app.UseGeoQuery(app.Configuration.GetConnectionString("Template"), options =>
{
    options.Prefix = "api/geo";
    options.IsConnectionStringTemplate = false;
    options.GeoJsonRouteHandlerOption.Allowed = false;
});
```