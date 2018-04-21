using System;
using System.IO;
using System.Net;
using System.Text;

namespace Pepper.NET
{
    internal class Utils
    {
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
        public static int GetDefaultPlayer(string file)
        {
            switch (Path.GetExtension(file))
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
