namespace Npgsql.GeoQuery.Test;

public class WebApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GeojsonQueryTest()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Template",
                "Host=192.168.2.100;Port=54322;UserId=test;Password=123456;Database={0}");
        }).CreateClient();

        var res = await client.GetAsync("/geo/geojson/ysyd/water/geom");
        var featureCollection = await res.Content.ReadFromJsonAsync<object>();

        Assert.NotNull(featureCollection);
    }
}