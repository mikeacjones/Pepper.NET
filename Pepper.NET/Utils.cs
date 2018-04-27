using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace PepperNET
{
    public class Utils
    {
        public static int GetChangelistID(StandardReturn ret)
        {
            int changeListID = -1;
            try
            {
                string xml = Utils.Base64Decode(ret.Ret);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                int.TryParse(xDoc.ChildNodes[0].Attributes["lastChangelistId"].Value, out changeListID);
            }
            catch { }
            return changeListID;
        }
        public static string GetHandleResult(StandardReturn ret, string table, string handle, string field)
        {
            try
            {
                string xml = Utils.Base64Decode(ret.Ret);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                XmlNode rootNode = xDoc.ChildNodes[0];
                foreach (XmlNode tableNode in rootNode.ChildNodes)
                {
                    string nodeName = tableNode.Attributes["name"].Value;
                    if (nodeName != table) continue;
                    foreach (XmlNode recNode in tableNode.ChildNodes)
                    {
                        if (recNode.Attributes["handle"].Value != handle) continue;
                        return recNode.Attributes[field].Value;
                    }
                }
            }
            catch {
                return null;
            }
            return null;
        }
        //public 
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string GetHttpResponse(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }
        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }
        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }
        public static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }

        public static int GetDefaultPlayer(string file)
        {
            switch (Path.GetExtension(file).Remove(0,1))
            {
                case "flv":
                case "mp4":
                case "m4v":
                case "mov":
                case "3gp":
                    return 3100;
                case "jpg":
                case "jpeg":
                case "gif":
                case "png":
                case "bmp":
                case "swf":
                case "pdf":
                    return 3130;
            }
            return -1;
        }
    }
}
