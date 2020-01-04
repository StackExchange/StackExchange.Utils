using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StackExchange.Utils.Tests
{
    [DataContract]
    public class HttpBinResponse
    {
        [DataMember(Name = "args")]
        public Dictionary<string, string> Args { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }

        [DataMember(Name = "files")]
        public Dictionary<string, string> Files { get; set; }

        [DataMember(Name = "form")]
        public Dictionary<string, string> Form { get; set; }

        [DataMember(Name = "headers")]
        public Dictionary<string, string> Headers { get; set; }

        [DataMember(Name = "json")]
        public object JSON { get; set; }

        [DataMember(Name = "origin")]
        public string Origin { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
