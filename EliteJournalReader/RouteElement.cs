using Newtonsoft.Json;

namespace EliteJournalReader
{

    public class RouteElement
    {
        public string StarSystem { get; set; }
        
        public long SystemAddress { get; set; }

        [JsonConverter(typeof(SystemPositionConverter))]
        public SystemPosition StarPos { get; set; }

        public string StarClass { get; set; }
    }
}