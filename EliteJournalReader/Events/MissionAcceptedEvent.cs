using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when starting a mission
    //Parameters:
    //•	Name: name of mission
    //•	Faction: faction offering mission
    //Optional Parameters (depending on mission type)
    //•	Commodity: commodity type
    //•	Count: number required / to deliver
    //•	Target: name of target
    //•	TargetType: type of target
    //•	TargetFaction: target’s faction
    public class MissionAcceptedEvent : JournalEvent<MissionAcceptedEvent.MissionAcceptedEventArgs>
    {
        public MissionAcceptedEvent() : base("MissionAccepted") { }

        public class MissionAcceptedEventArgs : JournalEventArgs
        {
            public long MissionID { get; set; }
            public string Name { get; set; }
            public string Faction { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public InfluenceLevel Influence { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public ReputationLevel Reputation { get; set; }

            public string Commodity { get; set; }
            public string Commodity_Localised { get; set; }
            public int? Count { get; set; }
            public string Target { get; set; }
            public string TargetType { get; set; }
            public string TargetFaction { get; set; }
            public int? KillCount { get; set; }
            public DateTime? Expiry { get; set; }
            public string DestinationSystem { get; set; }
            public string DestinationStation { get; set; }
            public int? PassengerCount { get; set; }
            public bool? PassengerVIPs { get; set; }
            public bool? PassengerWanted { get; set; }
            public string PassengerType { get; set; }

        }
    }
}
