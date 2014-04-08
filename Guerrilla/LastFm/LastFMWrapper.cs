using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Guerrilla.LastFm
{
    public class LastFMWrapper
    {
        private const string LastFMBaseUrl = "http://ws.audioscrobbler.com/2.0/";
        private const string LastFMApiKey = "2a739f5f6bc54b91b487601e831a44b3";

        private static WebRequest CreateWebRequest(string methodName, Dictionary<string, string> p)
        {
            StringBuilder sb = new StringBuilder(LastFMBaseUrl + "?method=" + methodName);
            if (p != null)
            {
                foreach (string key in p.Keys)
                {
                    sb.Append("&" + key + "=" + p[key]);
                }
            }
            sb.Append("&api_key=" + LastFMApiKey);
            sb.Append("&format=json");
            return HttpWebRequest.Create(sb.ToString());
        }

        private static void ProcessRequest(WebRequest request, Action<LastFMResponse, Exception> callback)
        {
            request.BeginGetResponse((ar) =>
            {
                WebResponse response = request.EndGetResponse(ar);
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string responseString = reader.ReadToEnd();

                    LastFMResponse result = JsonConvert.DeserializeObject<LastFMResponse>(responseString);

                    if (callback != null)
                    {
                        callback(result, null);
                    }
                }
            }, null);
        }

        public static void GetArtist(string artistName, Action<LastFMArtist, Exception> callback)
        {
            Dictionary<string, string> p = new Dictionary<string, string>();
            p.Add("artist", artistName);
            WebRequest request = CreateWebRequest("artist.getInfo", p);
            ProcessRequest(request, (result, exception) => {
                if (callback != null)
                {
                    callback(result.Artist, exception);
                }
            });
        }

        public static void GetTrack(string trackName, string artistName, Action<LastFMTrack, Exception> callback)
        {
            Dictionary<string, string> p = new Dictionary<string, string>();
            p.Add("track", trackName);
            p.Add("artist", trackName);
            WebRequest request = CreateWebRequest("track.getInfo", p);
            ProcessRequest(request, (result, exception) =>
            {
                if (callback != null)
                {
                    callback(result.Track, exception);
                }
            });
        }
    }
}