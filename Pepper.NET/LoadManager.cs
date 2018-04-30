using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace PepperNET
{
    internal class LoadManager
    {
        readonly SetOnce<UserDomainInfo> _UserDomainInfo = new SetOnce<UserDomainInfo>();
        readonly SetOnce<string> _UserPass = new SetOnce<string>();
        public UserDomainInfo UserDomainInfo
        {
            get { return _UserDomainInfo; }
            set { _UserDomainInfo.Value = value; }
        }
        public string UserPass
        {
            get { return _UserPass; }
            set { _UserPass.Value = value; }
        }
        public LoadManager(string user, string pass)
        {
            var userPass = user + "," + pass;
            userPass = Utils.Base64Encode(userPass);
            userPass = userPass.Replace('=', '.').Replace('+', '_').Replace('/', '-');
            this.UserPass = userPass;
        }
        public void Init()
        {
            var url = string.Format("https://galaxy.signage.me/WebService/getUserDomain.ashx?i_userpass={0}", UserPass);
            UserDomainInfo = Utils.GetHttpResponse(url);
        }
        public XmlDocument LoadData()
        {
            string url = string.Format("https://{0}/WebService/RequestData.ashx?businessId={1}", UserDomainInfo.Domain, UserDomainInfo.BusinessID);
            StandardReturn ret = Utils.GetHttpResponse(url);
            if (ret == null) return null;
            string xml = Utils.Base64Decode(ret);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            return xmlDoc;
        }
        public StandardReturn SaveData(XDocument changeList, Stack<string> filesToUpload)
        {
            StandardReturn cookieRet = BeginPostSession();
            string data = Utils.Base64Encode(changeList.ToString()).Replace('=', '.').Replace('+', '_').Replace('/', '-');
            var uploadURL = string.Format("https://{0}/WebService/JsUpload.ashx", UserDomainInfo.Domain);

            while (filesToUpload != null && filesToUpload.Count > 0)
            {
                var fileToUpload = filesToUpload.Pop();
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StringContent(Path.GetFileName(fileToUpload)), "filename");
                form.Add(new StringContent(cookieRet), "cookie");
                form.Add(new ByteArrayContent(File.ReadAllBytes(fileToUpload)), "file");
                var postTask = httpClient.PostAsync(uploadURL, form);
                postTask.Wait();
                HttpResponseMessage response = postTask.Result; 
                response.EnsureSuccessStatusCode();
                httpClient.Dispose();
            }

            StandardReturn saveResult = MultiPost(cookieRet, data, 0);
            int changeListID = -1;
            try
            {
                string xml = Utils.Base64Decode(saveResult.Ret);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                int.TryParse(xDoc.ChildNodes[0].Attributes["lastChangelistId"].Value, out changeListID);
            }
            catch { }

            return saveResult;
        }
        private StandardReturn BeginPostSession()
        {
            string url = string.Format("https://{0}/WebService/SubmitData.ashx?command=Begin&userpass={1}&appVersion=4.12&appType=StudioLite", UserDomainInfo.Domain, UserPass);

            StandardReturn cookieRet = Utils.GetHttpResponse(url);
            return cookieRet;
        }
        private StandardReturn MultiPost(string cookie, string data, int i)
        {
            var j = Math.Min(i + 1500, data.Length);
            var dChunk = (i < data.Length) ? data.Substring(i, j - i) : null;

            string url = string.Format(
                "https://{0}/WebService/SubmitData.ashx?command=Commit&cookie={1}&prm={2}&_={3}", 
                UserDomainInfo.Domain, 
                cookie, 
                dChunk, 
                Utils.GetCurrentUnixTimestampSeconds());
            StandardReturn response = Utils.GetHttpResponse(url);
            if (i == j)  return response;
            else return MultiPost(cookie, data, j);
        }
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //public async Task<bool> UploadResource(string resourceName, string resourcePath)
        //{
        //    try
        //    {
        //        var cookie = BeginPostSession();
        //        var url = string.Format("https://{0}/WebService/JsUpload.ashx", UserDomainInfo.Domain);

        //        HttpClient httpClient = new HttpClient();
        //        MultipartFormDataContent form = new MultipartFormDataContent();
        //        form.Add(new StringContent(resourceName), "filename");
        //        form.Add(new StringContent(cookie), "cookie");
        //        form.Add(new ByteArrayContent(File.ReadAllBytes(resourcePath)), "file");
        //        HttpResponseMessage response = await httpClient.PostAsync(url, form);
        //        response.EnsureSuccessStatusCode();
        //        httpClient.Dispose();
        //        string sd = response.Content.ReadAsStringAsync().Result;
        //        System.Diagnostics.Debug.WriteLine(sd);
        //        MultiPost(cookie, "", 0);
        //        return true;
        //    }
        //    catch(Exception ex) {
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //        return false;
        //    }
        //}
    }
}
