using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql.GeoQuery.Querys;
using System.Xml.Linq;

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
            [FromServices] IGeoQuery geoQuery,
            string database,
            string table,
            string geomColumn,
            int z,
            int x,
            int y,
            string? schema,
            string? columns,
            string? filter,
            bool? centroid
        ) =>
        {
            var bytes = await geoQuery.GetMvtBufferAsync(
                connectionStringTemplate.Format(database),
                table,
                geomColumn,
                z,
                x,
                y,
                schema ?? "public",
                columns.SpliteByComma(),
                filter,
                centroid ?? false);
            return Results.Bytes(bytes, "application/x-protobuf");
        });

        routeHandlerBuilders[1] = app.MapGet
        (prefix + "/geobuf/{database}/{table}/{geomColumn}.pbf", async (
            [FromServices] IGeoQuery geoQuery,
            string database,
            string table,
            string geomColumn,
            string? schema,
            string? columns,
            string? filter,
            bool? centroid
        ) =>
        {
            var bytes = await geoQuery.GetGeoBufferAsync(
                connectionStringTemplate.Format(database),
                table,
                geomColumn,
                schema ?? "public",
                columns.SpliteByComma(),
                filter,
                centroid ?? false);
            return Results.Bytes(bytes, "application/x-protobuf");
        });

        routeHandlerBuilders[2] = app.MapGet
        (prefix + "/geojson/{database}/{table}/{geomColumn}", async (
            [FromServices] IGeoQuery geoQuery,
            string database,
            string table,
            string geomColumn,
            string? schema,
            string? idColumn,
            string? columns,
            string? filter,
            bool? centroid
        ) =>
        {
            var geoJson =
                await geoQuery.GetGeoJsonAsync(
                connectionStringTemplate.Format(database),
                table,
                geomColumn,
                schema ?? "public",
                idColumn,
                columns.SpliteByComma(),
                filter,
                centroid ?? false);
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