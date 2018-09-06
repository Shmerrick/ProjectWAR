using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


namespace Launcher
{
    public static class HttpUtil
    {
        public delegate void StreamProgress(long current, long total, int chunkSize);

        public static async Task<T> Request<T>(string address, string endpoint, int retry = 5)
        {
            Exception ex = null;
            for (int i = 0; i < retry; i++)
            {
                try
                {
                    System.Net.ServicePointManager.DefaultConnectionLimit = 1600;
                    string path = $@"http://{address}/{endpoint}";

                    var request = HttpWebRequest.Create(path);

                    request.Proxy = null;
                    request.Method = "POST";
                    request.ContentLength = 0;
                    var response = await request.GetResponseAsync();

                    using (var stream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(stream);
                        return (T)new JavaScriptSerializer().Deserialize(reader.ReadToEnd(), typeof(T));
                    }
                }
                catch (Exception e)
                {
                    ex = e;
                }
                await Task.Delay(2000);
            }
            throw ex;
        }

        public static void RequestStream(string address, string endpoint, Stream output, StreamProgress progress, int blockSize = 0x8FFFF)
        {
            try
            {
                var request = HttpWebRequest.Create($@"http://{address}/{endpoint}");
                request.ContentType = "application/json";
                request.Method = "POST";
                var bodyStream = request.GetRequestStream();

                var response = request.GetResponse();

                using (var stream = response.GetResponseStream())
                {

                    long current = 0;
                    long total = response.ContentLength;
                    byte[] block = new byte[blockSize];
                    while (current < total)
                    {
                        int read = stream.Read(block, 0, blockSize);
                        if (read > 0)
                        {
                            output.Write(block, 0, read);
                            current += read;
                            progress(current, total, read);
                        }
                        else
                        {
                            progress(current, total, read);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
