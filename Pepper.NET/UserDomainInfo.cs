using Newtonsoft.Json;

namespace PepperNET
{
    internal class UserDomainInfo
    {
        readonly SetOnce<string> _Domain = new SetOnce<string>();
        readonly SetOnce<int> _BusinessID = new SetOnce<int>();
        readonly SetOnce<string> _ERI = new SetOnce<string>();
        readonly SetOnce<int> _StudioLite = new SetOnce<int>();
        readonly SetOnce<string> _ResellerInfo = new SetOnce<string>();

        [JsonProperty("domain")]
        public string Domain
        {
            get { return _Domain; }
            set { _Domain.Value = value; }
        }
        [JsonProperty("businessId")]
        public int BusinessID
        {
            get { return _BusinessID; }
            set { _BusinessID.Value = value; }
        }
        [JsonProperty("eri")]
        public string ERI
        {
            get { return _ERI; }
            set { _ERI.Value = value; }
        }
        [JsonProperty("studioLite")]
        public int StudioLite
        {
            get { return _StudioLite; }
            set { _StudioLite.Value = value; }
        }
        [JsonProperty("resellerInfo")]
        public string ResellerInfo
        {
            get { return _ResellerInfo; }
            set { _ResellerInfo.Value = value; }
        }

        public static implicit operator UserDomainInfo(string httpResponse)
        {
            httpResponse = httpResponse.Substring(1, httpResponse.Length - 2);
            return JsonConvert.DeserializeObject<UserDomainInfo>(httpResponse);
        }
    }
}
