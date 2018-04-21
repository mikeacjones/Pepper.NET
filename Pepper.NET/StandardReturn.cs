using Newtonsoft.Json;

namespace PepperNET
{
    internal class StandardReturn
    {
        readonly SetOnce<string> _Ret = new SetOnce<string>();
        readonly SetOnce<string> _Error = new SetOnce<string>();
        [JsonProperty("ret")]
        public string Ret
        {
            get { return _Ret; }
            set { _Ret.Value = value; }
        }
        [JsonProperty("error")]
        public string Error
        {
            get { return _Error; }
            set { _Error.Value = value; }
        }
        public static implicit operator StandardReturn(string httpResponse)
        {
            if (string.IsNullOrEmpty(httpResponse)) return null;
            if (httpResponse.Length < 3) return null;
            httpResponse = httpResponse.Substring(1, httpResponse.Length - 2);
            return JsonConvert.DeserializeObject<StandardReturn>(httpResponse);
        }
        public static implicit operator string(StandardReturn ret)
        {
            if (!string.IsNullOrEmpty(ret.Error)) return ret.Error;
            return ret.Ret;
        }
    }
}
