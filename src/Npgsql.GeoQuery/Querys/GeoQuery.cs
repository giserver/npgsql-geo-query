namespace Npgsql.GeoQuery.Querys;

internal class GeoQuery : IGeoQuery
{
    public async Task<byte[]> GetGeoBufferAsync(string connectionString, string table, string geomColumn, string[]? columns, string? filter)
    {
        connectionString.ThrowIfNullOrWhiteSpace(nameof(connectionString));
        table.ThrowIfNullOrWhiteSpace(nameof(table));
        geomColumn.ThrowIfNullOrWhiteSpace(nameof(geomColumn));

        var sql = $@"SELECT ST_AsGeobuf(q, 'geom')
                          FROM (SELECT
                                  ST_Transform({geomColumn}, 4326) as geom
                                  {(columns != null ? $", {columns}" : "")}
                                FROM
                                  {table}
                                {(filter != null ? $"WHERE {filter}" : "")}
                          ) as q;";

        return await QuerySingleValueAsync<byte[]>(connectionString, sql);
    }

    public async Task<string> GetGeoJsonAsync(string connectionString, string table, string geomColumn, string? idColumn, string[]? columns, string? filter)
    {
        connectionString.ThrowIfNullOrWhiteSpace(nameof(connectionString));
        table.ThrowIfNullOrWhiteSpace(nameof(table));
        geomColumn.ThrowIfNullOrWhiteSpace(nameof(geomColumn));

        var columnString = columns == null ? null : string.Join(',', columns.Select(x => $"\"{x}\""));

        var sql = $@"
            SELECT
                row_to_json(fc)
            FROM (
                SELECT
                    'FeatureCollection' AS type
                    ,COALESCE (array_to_json(array_agg(f)),'[]'::json) AS features
                FROM (
                    SELECT
                        'Feature' AS type
                        {(idColumn != null ? $",{idColumn} as id" : "")}
                        , ST_AsGeoJSON({geomColumn})::json as geometry  --geom表中的空间字段
                        , (
                            SELECT
                                row_to_json(t)
                            FROM (
                                SELECT
                                   {columnString ?? ""}
                                ) AS t
                            ) AS properties
                    FROM {table}
                    {(filter != null ? $"WHERE {filter}" : "")} ) AS f
               ) AS fc";

        return await QuerySingleValueAsync<string>(connectionString, sql);
    }

    public async Task<byte[]> GetMvtBufferAsync(string connectionString, string table, string geomColumn, int z, int x, int y, string[]? columns, string? filter)
    {
        connectionString.ThrowIfNullOrWhiteSpace(nameof(connectionString));
        table.ThrowIfNullOrWhiteSpace(nameof(table));
        geomColumn.ThrowIfNullOrWhiteSpace(nameof(geomColumn));

        var sql = $@"
            WITH mvt_geom as (
              SELECT
                ST_AsMVTGeom (
                  ST_Transform({geomColumn}, 3857),
                  ST_TileEnvelope({z}, {x}, {y})
                ) as geom
                {(columns != null ? $",{columns}" : "")}
              FROM
                {table},
                (SELECT ST_SRID({geomColumn}) AS srid FROM {table} LIMIT 1) a
              WHERE
                ST_Intersects(
                  {geomColumn},
                  ST_Transform(ST_TileEnvelope({z}, {x}, {y}),srid)
                ) {(filter != null ? $" AND {filter}" : "")}
            )
            SELECT ST_AsMVT(mvt_geom.*, '{table}', 4096, 'geom') AS mvt from mvt_geom;";

        return await QuerySingleValueAsync<byte[]>(connectionString, sql);
    }

    private static async Task<T> QuerySingleValueAsync<T>(string connectionString, string sql, Array? parameters = null)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        if (parameters != null)
            cmd.Parameters.AddRange(parameters);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        return (T)reader[0];
    }
}