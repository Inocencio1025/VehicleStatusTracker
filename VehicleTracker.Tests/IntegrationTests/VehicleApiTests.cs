using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;

public class VehicleApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VehicleApiTests()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Vehicle_Status_Endpoint_Returns_OK()
    {
        var response = await _client.GetAsync("/api/vehicle/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}