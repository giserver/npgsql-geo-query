using Dapper;

namespace Npgsql.GeoQuery.Test;

public class MockData
{
    public static void MockDatabase(string connectionStringTemplate, string tableName)
    {
        CreateDataBase(connectionStringTemplate, tableName);

        CreateTable(string.Format(connectionStringTemplate, tableName));
    }

    private static void CreateDataBase(string connectionStringTemplate, string tableName)
    {
        using var conn = new NpgsqlConnection(string.Format(connectionStringTemplate, "postgres"));

        var sqlDbCount = $"SELECT COUNT(*) FROM pg_database WHERE datname = '{tableName}';";
        var dbCount = conn.ExecuteScalar<int>(sqlDbCount);
        if (dbCount == 0)
        {
            var sql = $"CREATE DATABASE \"{tableName}\"";
            conn.Execute(sql);
        }
    }

    private static void CreateTable(string connectionString)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Execute($"CREATE EXTENSION IF NOT EXISTS \"postgis\"");

        var sql = """
            CREATE TABLE IF NOT EXISTS t_point (
                id int4 PRIMARY KEY,
                name TEXT,
                geom geometry(GEOMETRY) NOT NULL
            );

            CREATE TABLE IF NOT EXISTS t_line (
                id int4 PRIMARY KEY,
                name TEXT,
                geom geometry(GEOMETRY) NOT NULL
            );

            CREATE TABLE IF NOT EXISTS t_polygon (
                id int4 PRIMARY KEY,
                name TEXT,
                geom geometry(GEOMETRY) NOT NULL
            );

            TRUNCATE TABLE t_point;
            TRUNCATE TABLE t_line;
            TRUNCATE TABLE t_polygon;

            INSERT INTO t_point VALUES
            (1, '1', ST_GeomFromText('POINT(121.1 31.1)',4326)),
            (2, '2', ST_GeomFromText('POINT(121.2 31.2)',4326)),
            (3, '3', ST_GeomFromText('POINT(121.3 31.3)',4326)),
            (4, '4', ST_GeomFromText('POINT(121.4 31.4)',4326)),
            (5, '5', ST_GeomFromText('POINT(121.5 31.5)',4326));

            INSERT INTO t_line VALUES
            (1, '1', ST_GeomFromText('LINESTRING(121.1 31.1,121.2 31.2)',4326)),
            (2, '2', ST_GeomFromText('LINESTRING(121.2 31.2,121.3 31.3)',4326)),
            (3, '3', ST_GeomFromText('LINESTRING(121.3 31.3,121.4 31.4)',4326)),
            (4, '4', ST_GeomFromText('LINESTRING(121.4 31.4,121.5 31.5)',4326)),
            (5, '5', ST_GeomFromText('LINESTRING(121.5 31.5,121.6 31.6)',4326));

            INSERT INTO t_polygon VALUES
            (1, '1', ST_GeomFromText('POLYGON((121.1 31.1,121.2 31.2,121.3 31.3,121.1 31.1))',4326)),
            (2, '2', ST_GeomFromText('POLYGON((121.2 31.2,121.3 31.3,121.4 31.4,121.2 31.2))',4326)),
            (3, '3', ST_GeomFromText('POLYGON((121.3 31.3,121.4 31.4,121.5 31.5,121.3 31.3))',4326)),
            (4, '4', ST_GeomFromText('POLYGON((121.4 31.4,121.5 31.5,121.6 31.6,121.4 31.4))',4326)),
            (5, '5', ST_GeomFromText('POLYGON((121.5 31.5,121.6 31.6,121.7 31.7,121.5 31.5))',4326));
            """;

        conn.Execute(sql);
    }
}