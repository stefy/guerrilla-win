using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guerrilla.LastFm
{
    public class LastFMResponse
    {
        [JsonProperty("artist")]
        public LastFMArtist Artist { get; set; }

        [JsonProperty("track")]
        public LastFMTrack Track { get; set; }
    }
}
