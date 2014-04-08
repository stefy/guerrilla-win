using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guerrilla.LastFm
{
    public class LastFMImage
    {
        [JsonProperty("#text")]
        public string Url { get; set; }
        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
