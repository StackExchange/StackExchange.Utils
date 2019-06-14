using System;
using System.Collections.Generic;
using System.Net;

namespace StackExchange.Utils
{
    /// <summary>
    /// Settings for a Web Proxy to be used for requests
    /// </summary>
    public class HttpProxySettings : IEquatable<HttpProxySettings>, ICloneable
    {
        /// <summary>
        /// Should the Request use a Proxy?
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// The Proxy to use for the Request
        /// </summary>
        public IWebProxy Proxy { get; set; }

        object ICloneable.Clone()
            => new HttpProxySettings
            {
                UseProxy = UseProxy,
                Proxy = Proxy
            };

        /// <see cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return Equals(obj as HttpProxySettings);
        }

        /// <see cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(HttpProxySettings other)
        {
            return other != null &&
                   UseProxy == other.UseProxy &&
                   EqualityComparer<IWebProxy>.Default.Equals(Proxy, other.Proxy);
        }

        /// <see cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            var hashCode = 1360686298;
            hashCode = hashCode * -1521134295 + UseProxy.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IWebProxy>.Default.GetHashCode(Proxy);
            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="HttpProxySettings"/> by comparing their members for equality
        /// </summary>
        public static bool operator ==(HttpProxySettings left, HttpProxySettings right)
        {
            return EqualityComparer<HttpProxySettings>.Default.Equals(left, right);
        }

        /// <summary>
        /// Compares two <see cref="HttpProxySettings"/> by comparing their members for non-equality
        /// </summary>
        public static bool operator !=(HttpProxySettings left, HttpProxySettings right)
        {
            return !(left == right);
        }
    }
}
