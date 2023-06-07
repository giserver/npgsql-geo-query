# npgsql-geo-query
[![Nuget](https://img.shields.io/nuget/v/Npgisql.GeoQuery)](https://www.nuget.org/packages/Npgisql.GeoQuery)
![Nuget](https://img.shields.io/nuget/dt/Npgisql.GeoQuery)

query `mvt` `pbf` `geojson` data from postgresql(+postgis)

## Usage 
### aspnetcore webapi
``` csharp
// inject GeoQuery
builder.Services.AddGeoQuery();

// config route
app.UseGeoQuery("Host=localhost;Port=5432;UserId=test;Password=123456;Database={0}");
```