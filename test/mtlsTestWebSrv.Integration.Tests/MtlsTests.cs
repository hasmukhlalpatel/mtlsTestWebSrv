using System.Net;

namespace mtlsTestWebSrv.Integration.Tests
{
    public class MtlsTests : IClassFixture<MtlsWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MtlsTests(MtlsWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Allow_Access_With_Valid_Client_Certificate()
        {
            var response = await _client.GetAsync("/secure-endpoint");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

}