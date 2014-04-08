using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guerrilla.LastFm
{
    public class LastFMTrack
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mbid")]
        public string Mbid { get; set; }
        [JsonProperty("artist.name")]
        public string Artist { get; set; }
        [JsonProperty("listeners")]
        public long Listeners { get; set; }
        [JsonProperty("playcount")]
        public long PlayCount { get; set; }
        [JsonProperty("album")]
        public LastFMAlbum Album { get; set; }
    }
}
