using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;

namespace Npgsql.GeoQuery.Test;

public class WebApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private const string CONNECTION_STRING_TEMPLATE = "Host=192.168.2.100;Port=54322;UserId=test;Password=123456;Database={0}";
    private const string DATABASE_TEST = "npgisql_test";
    private readonly HttpClient _client;

    public WebApiTest(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Template", CONNECTION_STRING_TEMPLATE);
            MockData.MockDatabase(CONNECTION_STRING_TEMPLATE, DATABASE_TEST);
        }).CreateClient();
    }

    [Theory]
    [InlineData("t_point", null, null)]
    [InlineData("t_point", "id,name", true)]
    [InlineData("t_line", null, null)]
    [InlineData("t_polygon", null, null)]
    public async Task GeojsonQueryTest(string table, string? columns, bool? centroid)
    {
        var uri = $"/geo/geojson/{DATABASE_TEST}/{table}/geom";
        var querys = new List<string>();
        if (columns != null)
            querys.Add($"columns={columns}");
        if (centroid != null)
            querys.Add($"centroid={centroid}");
        if (querys.Any())
            uri += "?" + string.Join('&', querys);

        var res = await _client.GetAsync(uri);
        var featureCollection = await res.Content.ReadFromJsonAsync<FeatureCollection>(new System.Text.Json.JsonSerializerOptions
        {
            Converters = { new GeoJsonConverterFactory() }
        });

        Assert.NotNull(featureCollection);
    }

    [Fact]
    public async Task GeojsonQueryWithFilterTest()
    {
        var uri = $"/geo/geojson/{DATABASE_TEST}/t_point/geom?filter=id=1";

        var res = await _client.GetAsync(uri);
        var featureCollection = await res.Content.ReadFromJsonAsync<FeatureCollection>(new System.Text.Json.JsonSerializerOptions
        {
            Converters = { new GeoJsonConverterFactory() }
        });

        Assert.NotNull(featureCollection);
        Assert.Equal(1, featureCollection.Count!);
    }
}