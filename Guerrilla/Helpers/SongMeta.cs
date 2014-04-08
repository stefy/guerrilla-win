using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guerrilla
{
    public class SongMeta
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("song")]
        public string Song { get; set; }
        [JsonProperty("album")]
        public string Album { get; set; }
        [JsonProperty("artistImage")]
        public string ArtistImage { get; set; }
        [JsonProperty("albumImage")]
        public string AlbumArtwork { get; set; }
    }
}
