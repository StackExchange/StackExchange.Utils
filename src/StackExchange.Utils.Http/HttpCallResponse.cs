using System;
using System.Net;
using System.Net.Http;

namespace StackExchange.Utils
{
    /// <summary>
    /// The result of an HTTP call.
    /// </summary>
    public class HttpCallResponse
    {
        /// <summary>
        /// Whether the call was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// The URL of the request.
        /// </summary>
        public string RequestUri => RawRequest?.RequestUri?.AbsoluteUri;

        /// <summary>
        /// The raw <see cref="HttpRequestMessage"/> attempted.
        /// </summary>
        public HttpRequestMessage RawRequest { get; }

        /// <summary>
        /// The raw <see cref="HttpResponseMessage"/> to the request.
        /// </summary>
        public HttpResponseMessage RawResponse { get; }

        /// <summary>
        /// The error that occured on the request, if any.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// The status code of the response.
        /// </summary>
        public HttpStatusCode? StatusCode => RawResponse?.StatusCode;

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse"/>, when an error was thrown.
        /// </summary>
        /// <param name="request">The request message that was attempted.</param>
        /// <param name="error">The error that was thrown.</param>
        protected HttpCallResponse(HttpRequestMessage request, Exception error)
        {
            RawRequest = request;
            Success = false;
            Error = error;
        }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse"/> from a response.
        /// </summary>
        /// <param name="response">The response to create the <see cref="HttpCallResponse"/> from.</param>
        protected HttpCallResponse(HttpResponseMessage response)
        {
            Success = response.IsSuccessStatusCode;
            RawResponse = response;
            RawRequest = response.RequestMessage;
        }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse"/> from a response, when an error was thrown.
        /// </summary>
        /// <param name="response">The response to create the <see cref="HttpCallResponse"/> from.</param>
        /// <param name="error">The error that was thrown.</param>
        protected HttpCallResponse(HttpResponseMessage response, Exception error) : this(response)
        {
            Success = false;
            Error = error;
        }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse{T}"/> from a typed request.
        /// </summary>
        /// <typeparam name="T">The type of payload in the request result.</typeparam>
        /// <param name="request">The request to create the response wrapper from.</param>
        /// <param name="error">The error that was thrown, if any.</param>
        /// <returns>The created <see cref="HttpCallResponse{T}"/>.</returns>
        public static HttpCallResponse<T> Create<T>(HttpRequestMessage request, Exception error = null)
        {
            error = (error ?? new HttpClientException("Failed to send request for " + request.RequestUri, request.RequestUri))
                // Add these regardless of source
                .AddLoggedData("Request URI", request.RequestUri);

            return new HttpCallResponse<T>(request, error);
        }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse{T}"/> from a typed request response.
        /// </summary>
        /// <typeparam name="T">The type of payload in the request result.</typeparam>
        /// <param name="response">The response to create the response wrapper from.</param>
        /// <param name="error">The error that was thrown, if any.</param>
        /// <returns>The created <see cref="HttpCallResponse{T}"/>.</returns>
        public static HttpCallResponse<T> Create<T>(HttpResponseMessage response, Exception error)
        {
            // Add these regardless of source
            error.AddLoggedData("Response.Code", ((int)response.StatusCode).ToString())
                 .AddLoggedData("Response.Status", response.StatusCode.ToString())
                 .AddLoggedData("Response.ReasonPhrase", response.ReasonPhrase)
                 .AddLoggedData("Response.ContentType", response.Content.Headers.ContentType)
                 .AddLoggedData("Request.URI", response.RequestMessage.RequestUri);

            return new HttpCallResponse<T>(response, error);
        }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse{T}"/> from a typed request response.
        /// </summary>
        /// <typeparam name="T">The type of payload in the request result.</typeparam>
        /// <param name="response">The response to create the response wrapper from.</param>
        /// <param name="data">The (deserialized, if necessary) payload returned on the request.</param>
        /// <returns>The created <see cref="HttpCallResponse{T}"/>.</returns>
        public static HttpCallResponse<T> Create<T>(HttpResponseMessage response, T data) => new HttpCallResponse<T>(response, data);
    }

    /// <summary>
    /// A typed version of <see cref="HttpCallResponse"/> which has a payload.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpCallResponse<T> : HttpCallResponse
    {
        /// <summary>
        /// The response payload.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse{T}"/> from a typed request response.
        /// </summary>
        /// <param name="response">The response to create the response wrapper from.</param>
        /// <param name="data">The (deserialized, if necessary) payload returned on the request.</param>
        /// <returns>The created <see cref="HttpCallResponse{T}"/>.</returns>
        public HttpCallResponse(HttpResponseMessage response, T data) : base(response)
        {
            Data = data;
        }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse{T}"/> from a typed request.
        /// </summary>
        /// <param name="request">The request to create the response wrapper from.</param>
        /// <param name="error">The error that was thrown, if any.</param>
        /// <returns>The created <see cref="HttpCallResponse{T}"/>.</returns>
        public HttpCallResponse(HttpRequestMessage request, Exception error) : base(request, error) { }

        /// <summary>
        /// Creates a new <see cref="HttpCallResponse{T}"/> from a typed request response.
        /// </summary>
        /// <param name="response">The response to create the response wrapper from.</param>
        /// <param name="error">The error that was thrown, if any.</param>
        /// <returns>The created <see cref="HttpCallResponse{T}"/>.</returns>
        public HttpCallResponse(HttpResponseMessage response, Exception error) : base(response, error) { }
    }
}
