using System;
using System.Net;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class DefaultHttpClientPoolTests
    {
        private bool CompareHttpClientCacheKeys(IRequestBuilder one, IRequestBuilder two)
        {
            var t = typeof(DefaultHttpClientPool);
            var cacheKeyType = t.GetNestedType("HttpClientCacheKey", System.Reflection.BindingFlags.NonPublic);
            var oneCacheKey = Activator.CreateInstance(cacheKeyType, one.Timeout, one.Proxy);
            var twoCacheKey = Activator.CreateInstance(cacheKeyType, two.Timeout, two.Proxy);
            return oneCacheKey.Equals(twoCacheKey);
        }

        [Fact]
        public void HttpClientCacheKeyEquality_Defaults_True()
        {
            var rbOne = Http.Request("http://example.com");
            var rbTwo = Http.Request("http://example.com");
            Assert.True(CompareHttpClientCacheKeys(rbOne, rbTwo));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_SameTimeout_True()
        {

            var rbOneWithTimeout = Http.Request("http://example.com").WithTimeout(TimeSpan.FromSeconds(10));
            var rbTwoWithTimeout = Http.Request("http://example.com").WithTimeout(TimeSpan.FromSeconds(10));
            Assert.True(CompareHttpClientCacheKeys(rbOneWithTimeout, rbTwoWithTimeout));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_DifferentTimeout_False()
        {

            var rbOneWithTimeout = Http.Request("http://example.com").WithTimeout(TimeSpan.FromSeconds(10));
            var rbTwoWithTimeout = Http.Request("http://example.com").WithTimeout(TimeSpan.FromSeconds(20));
            Assert.False(CompareHttpClientCacheKeys(rbOneWithTimeout, rbTwoWithTimeout));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_SameUseProxy_True()
        {
            var rbOneWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { UseProxy = true });
            var rbTwoWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { UseProxy = true });
            Assert.True(CompareHttpClientCacheKeys(rbOneWithProxy, rbTwoWithProxy));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_DifferentUseProxy_False()
        {
            var rbOneWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { UseProxy = true });
            var rbTwoWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { UseProxy = false });
            Assert.False(CompareHttpClientCacheKeys(rbOneWithProxy, rbTwoWithProxy));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_SameProxyInstances_True()
        {
            var proxy = new WebProxy();
            var rbOneWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { Proxy = proxy });
            var rbTwoWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { Proxy = proxy });
            Assert.True(CompareHttpClientCacheKeys(rbOneWithProxy, rbTwoWithProxy));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_DifferentProxyInstances_False()
        {
            // WebProxy uses Reference Equality
            var rbOneWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { Proxy = new WebProxy() });
            var rbTwoWithProxy = Http.Request("http://example.com").WithProxy(new HttpProxySettings { Proxy = new WebProxy() });
            Assert.False(CompareHttpClientCacheKeys(rbOneWithProxy, rbTwoWithProxy));
        }

        [Fact]
        public void HttpClientCacheKeyEquality_SameTimeoutAndUseProxy_True()
        {
            var rbOneWithTimeoutAndProxy = Http.Request("http://example.com").WithTimeout(TimeSpan.FromSeconds(10)).WithProxy(new HttpProxySettings { UseProxy = true });
            var rbTwoWithTimeoutAndProxy = Http.Request("http://example.com").WithTimeout(TimeSpan.FromSeconds(10)).WithProxy(new HttpProxySettings { UseProxy = true });
            Assert.True(CompareHttpClientCacheKeys(rbOneWithTimeoutAndProxy, rbTwoWithTimeoutAndProxy));
        }
    }
}
