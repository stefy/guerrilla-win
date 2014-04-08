using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guerrilla.LastFm
{
    public class LastFMArtist
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mbid")]
        public string Mbid { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("image")]
        public List<LastFMImage> Images { get; set; }

        public string MegaImage()
        {
            foreach (LastFMImage img in Images)
            {
                if (img.Size == "mega")
                {
                    return img.Url;
                }
            }
            return null;
        }
    }
}
