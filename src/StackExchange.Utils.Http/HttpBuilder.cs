using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StackExchange.Utils
{
    internal class HttpBuilder : IRequestBuilder
    {
        public HttpSettings Settings { get; }
        public HttpRequestMessage Message { get; }
        public bool LogErrors { get; set; } = true;
        public IEnumerable<HttpStatusCode> IgnoredResponseStatuses { get; set; } = Enumerable.Empty<HttpStatusCode>();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3); // We've defaulted to 3 seconds since forever
        public event EventHandler<HttpExceptionArgs> BeforeExceptionLog;
        private readonly string _callerName, _callerFile;
        private readonly int _callerLine;

        public HttpBuilder(string uri, HttpSettings settings, string callerName, string callerFile, int callerLine)
        {
            Message = new HttpRequestMessage
            {
                RequestUri = new Uri(uri, UriKind.RelativeOrAbsolute)
            };
            Settings = settings;
            _callerName = callerName;
            _callerFile = callerFile;
            _callerLine = callerLine;
        }

        public void OnBeforeExceptionLog(HttpExceptionArgs args)
        {
            BeforeExceptionLog?.Invoke(this, args);
        }

        internal void AddExceptionData(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            try
            {
                var servicePoint = ServicePointManager.FindServicePoint(Message.RequestUri);
                if (servicePoint != null)
                {
                    ex.AddLoggedData("ServicePoint.ConnectionLimit", servicePoint.ConnectionLimit)
                      .AddLoggedData("ServicePoint.CurrentConnections", servicePoint.CurrentConnections)
                      .AddLoggedData("ServicePointManager.CurrentConnections", ServicePointManager.DefaultConnectionLimit);
                }
            }
            catch { }

            ex.AddLoggedData("Caller.Name", _callerName)
                .AddLoggedData("Caller.File", _callerFile)
                .AddLoggedData("Caller.Line", _callerLine.ToString());
        }

        public IRequestBuilder<T> WithHandler<T>(Func<HttpResponseMessage, Task<T>> handler) => new HttpBuilder<T>(this, handler);
    }

    internal class HttpBuilder<T> : IRequestBuilder<T>
    {
        public IRequestBuilder Inner { get; }
        public Func<HttpResponseMessage, Task<T>> Handler { get; }

        public HttpBuilder(HttpBuilder builder, Func<HttpResponseMessage, Task<T>> handler)
        {
            Inner = builder;
            Handler = handler;
        }
    }
}
