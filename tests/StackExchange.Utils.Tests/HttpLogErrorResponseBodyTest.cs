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
                .ExpectJson<SomeResponseObject>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<SomeResponseObject>>(response);
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
                .ExpectJson<SomeResponseObject>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<SomeResponseObject>>(response);
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
                .ExpectJson<SomeResponseObject>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<SomeResponseObject>>(response);
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
                .ExpectJson<SomeResponseObject>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<SomeResponseObject>>(response);
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
                .ExpectJson<SomeResponseObject>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.UnprocessableEntity,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<SomeResponseObject>>(response);
            Assert.Null(httpCallResponses.Error.Data[Http.DefaultSettings.ErrorDataPrefix + "Response.Body"]);
        }
        
        [Fact]
        public async Task WithErrorResponseBodyLogging_IfResponseSuccess_DoesNotIncludeResponseBodyInExceptionAndDeserializesCorrectly()
        {
            const string successResponseBody = @"{""SomeAttribute"": ""some value""}";
            _stubHttp.Stub(x => x.Get("/some-endpoint"))
                .Return(successResponseBody)
                .WithStatus(HttpStatusCode.OK);

            var response = await Http
                .Request("http://localhost:9191/some-endpoint")
                .WithErrorResponseBodyLogging(HttpStatusCode.UnprocessableEntity)
                .ExpectJson<SomeResponseObject>()
                .GetAsync();

            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            
            var httpCallResponses = Assert.IsType<HttpCallResponse<SomeResponseObject>>(response);
            Assert.Null(httpCallResponses.Error);
            Assert.Equal("some value", httpCallResponses.Data.SomeAttribute);
        }
    }

    public class SomeResponseObject
    {
        public string SomeAttribute { get; set; }
    }
}
