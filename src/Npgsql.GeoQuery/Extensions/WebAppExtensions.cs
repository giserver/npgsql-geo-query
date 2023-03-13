using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Npgsql.GeoQuery.Extensions;

public static class WebAppExtensions
{
    public static WebApplication UseGeoQuery(this WebApplication app,
        string connectionStringTemplate,
        string prefix = "geo",
        Action<RouteHandlerBuilder>? builderAction = null)
    {
        connectionStringTemplate.ThrowIfNullOrWhiteSpace(nameof(connectionStringTemplate));

        var routeHandlerBuilders = new RouteHandlerBuilder[3];

        routeHandlerBuilders[0] = app.MapGet
        (prefix + "/mvt/{database}/{table}/{geomColumn}/{z:int}/{x:int}/{y:int}.pbf", async (
            string database,
            string table,
            string geomColumn,
            int z,
            int x,
            int y,
            string? columns,
            string? filter,
            [FromServices] IGeoQuery geoQuery
        ) =>
        {
            var bytes = await geoQuery.GetMvtBufferAsync(
                connectionStringTemplate.Format(database),
                table,
                geomColumn,
                z,
                x,
                y,
                columns.SpliteByComma(),
                filter);
            return Results.Bytes(bytes, "application/x-protobuf");
        });

        routeHandlerBuilders[1] = app.MapGet
        (prefix + "/geobuf/{database}/{table}/{geomColumn}.pbf", async (
            string database,
            string table,
            string geomColumn,
            string? columns,
            string? filter,
            [FromServices] IGeoQuery geoQuery
        ) =>
        {
            var bytes = await geoQuery.GetGeoBufferAsync(
                connectionStringTemplate.Format(database),
                table,
                geomColumn,
                columns.SpliteByComma(),
                filter);
            return Results.Bytes(bytes, "application/x-protobuf");
        });

        routeHandlerBuilders[2] = app.MapGet
        (prefix + "/geojson/{database}/{table}/{geomColumn}", async (
            string database,
            string table,
            string geomColumn,
            string? idColumn,
            string? columns,
            string? filter,
            [FromServices] IGeoQuery geoQuery
        ) =>
        {
            var geoJson =
                await geoQuery.GetGeoJsonAsync(
                connectionStringTemplate.Format(database),
                table,
                geomColumn,
                idColumn,
                columns.SpliteByComma(),
                filter);
            return Results.Text(geoJson, "application/json");
        });

        if (builderAction != null)
            foreach (var builder in routeHandlerBuilders)
            {
                builderAction(builder);
            }

        return app;
    }
}