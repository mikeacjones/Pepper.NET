using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
        public StandardReturn SaveData(XDocument changeList, Stack<FilePack> filesToUpload)
        {
            StandardReturn cookieRet = BeginPostSession();
            string data = Utils.Base64Encode(changeList.ToString()).Replace('=', '.').Replace('+', '_').Replace('/', '-');
            var uploadURL = string.Format("https://{0}/WebService/JsUpload.ashx", UserDomainInfo.Domain);

            while (filesToUpload != null && filesToUpload.Count > 0)
            {
                var fileToUpload = filesToUpload.Pop();
                var fileParm = new FormUpload.FileParameter(
                    fileToUpload.FilePath, 
                    Path.GetFileName(fileToUpload.FilePath)
                );
               
                var response = FormUpload.MultipartFormDataPost(
                    uploadURL,
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36",
                    new Dictionary<string, object> {
                        { "cookie", cookieRet.Ret },
                        { "file", fileParm },
                        { "filename", fileToUpload.FileRecord.Handle + "." + fileToUpload.FileRecord["resource_type"] }
                    });
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
        //private string UploadFile(string endpointUrl, string filePath, string cookie, string resourceName)
        //{

        //    FileStream fs = null;
        //    Stream rs = null;
        //    string result = "";
        //    try
        //    {

        //        string uploadFileName = System.IO.Path.GetFileName(filePath);

        //        fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        //        var request = (HttpWebRequest)WebRequest.Create(endpointUrl);
        //        request.Method = WebRequestMethods.Http.Post;
        //        request.AllowWriteStreamBuffering = false;
        //        request.SendChunked = true;
        //        String CRLF = "\r\n"; // Line separator required by multipart/form-data.        
        //        long timestamp = Utils.GetCurrentUnixTimestampSeconds();

        //        string boundary = "------" + timestamp.ToString("x");
        //        request.ContentType = "multipart/form-data; boundary=" + boundary;
        //        request.Accept = "*/*";

        //        long bytesAvailable = fs.Length;
        //        long maxBufferSize = 1 * 1024 * 1024;


        //        rs = request.GetRequestStream();
        //        byte[] buffer = new byte[50];
        //        int read = 0;

        //        byte[] buf = Encoding.UTF8.GetBytes(boundary + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"cookie\"" + CRLF + CRLF + cookie + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes(boundary + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"file\"; filename=\"" + uploadFileName + "\"" + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes("Content-Type: image/png" + CRLF + CRLF + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes(boundary + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"filename\"" + CRLF + CRLF + resourceName + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        buf = Encoding.UTF8.GetBytes(boundary + "--" + CRLF);
        //        rs.Write(buf, 0, buf.Length);

        //        //writer.append("Content-Type: application/octet-stream;").append(CRLF);
        //        rs.Flush();


        //        long bufferSize = Math.Min(bytesAvailable, maxBufferSize);
        //        buffer = new byte[bufferSize];
        //        while ((read = fs.Read(buffer, 0, buffer.Length)) != 0)
        //        {
        //            rs.Write(buffer, 0, read);
        //        }
        //        buf = Encoding.UTF8.GetBytes(CRLF);
        //        rs.Write(buf, 0, buf.Length);
        //        rs.Flush();


        //        // End of multipart/form-data.
        //        buffer = Encoding.UTF8.GetBytes(boundary + "--" + CRLF);
        //        rs.Write(buffer, 0, buffer.Length);

        //        using (var response = request.GetResponse())
        //        using (var responseStream = response.GetResponseStream())
        //        using (var reader = new StreamReader(responseStream))
        //        {

        //            result = reader.ReadToEnd();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        result = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }

        //    return result;
        //}
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
