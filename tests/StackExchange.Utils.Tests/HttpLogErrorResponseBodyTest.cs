using System.Net;
using System.Threading.Tasks;
using HttpMock;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class HttpLogErrorResponseBodyTest
    {
        private readonly IHttpServer _stubHttp = HttpMockRepository.At("http://localhost:9191");

        [Fact]
        public async Task WithErrorResponseBodyLogging_IfStatusCodeMatchesTheGivenOne_IncludeResponseBodyInException()
        {
            const string errorResponseBody = "{'Foo': 'Bar'}";
        	_stubHttp.Stub(x => x.Get("/some-endpoint"))
        			.Return(errorResponseBody)
        			.WithStatus(HttpStatusCode.UnprocessableEntity);

            var response = await Http
                .Request("http://localhost:9191/some-endpoint")
                .WithErrorResponseBodyLogging(HttpStatusCode.UnprocessableEntity)
                .ExpectString()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<string>>(response);
            Assert.Equal(errorResponseBody, httpCallResponses.Error.Data[Http.DefaultSettings.ErrorDataPrefix + "Response.Body"]);
        }
        
        [Fact]
        public async Task WithErrorResponseBodyLogging_IfStatusCodeMatchesOneOfTheGiven_IncludeResponseBodyInException()
        {
            const string errorResponseBody = "{'Foo': 'Bar'}";
            _stubHttp.Stub(x => x.Get("/some-endpoint"))
                .Return(errorResponseBody)
                .WithStatus(HttpStatusCode.UnprocessableEntity);

            var response = await Http
                .Request("http://localhost:9191/some-endpoint")
                .WithErrorResponseBodyLogging(HttpStatusCode.NotAcceptable, HttpStatusCode.UnprocessableEntity)
                .ExpectString()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<string>>(response);
            Assert.Equal(errorResponseBody, httpCallResponses.Error.Data[Http.DefaultSettings.ErrorDataPrefix + "Response.Body"]);
        }
        
        [Fact]
        public async Task WithErrorResponseBodyLogging_IfStatusCodeDoesNotMatchAnyOfTheGiven_DoesNotIncludeResponseBodyInException()
        {
            const string errorResponseBody = "{'Foo': 'Bar'}";
            _stubHttp.Stub(x => x.Get("/some-endpoint"))
                .Return(errorResponseBody)
                .WithStatus(HttpStatusCode.UnprocessableEntity);

            var response = await Http
                .Request("http://localhost:9191/some-endpoint")
                .WithErrorResponseBodyLogging(HttpStatusCode.NotAcceptable, HttpStatusCode.BadRequest)
                .ExpectString()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<string>>(response);
            Assert.Null(httpCallResponses.Error.Data[Http.DefaultSettings.ErrorDataPrefix + "Response.Body"]);
        }

        [Fact]
        public async Task WithErrorResponseBodyLogging_IfNoStatusCodesGiven_DoesNotIncludeResponseBodyInException()
        {
            const string errorResponseBody = "{'Foo': 'Bar'}";
            _stubHttp.Stub(x => x.Get("/some-endpoint"))
                .Return(errorResponseBody)
                .WithStatus(HttpStatusCode.UnprocessableEntity);

            var response = await Http
                .Request("http://localhost:9191/some-endpoint")
                .WithErrorResponseBodyLogging()
                .ExpectString()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<string>>(response);
            Assert.Null(httpCallResponses.Error.Data[Http.DefaultSettings.ErrorDataPrefix + "Response.Body"]);
        }
        
        [Fact]
        public async Task WithErrorResponseBodyLogging_WithoutCallingWithErrorResponseBodyLogging_DoesNotIncludeResponseBodyInException()
        {
            const string errorResponseBody = "{'Foo': 'Bar'}";
            _stubHttp.Stub(x => x.Get("/some-endpoint"))
                .Return(errorResponseBody)
                .WithStatus(HttpStatusCode.UnprocessableEntity);

            var response = await Http
                .Request("http://localhost:9191/some-endpoint")
                .ExpectString()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<string>>(response);
            Assert.Null(httpCallResponses.Error.Data[Http.DefaultSettings.ErrorDataPrefix + "Response.Body"]);
        }
    }
}
