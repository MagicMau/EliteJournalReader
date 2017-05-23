using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");
                Faction = evt.Value<string>("Faction");
                MissionId = evt.Value<int>("MissionID");
                Influence = evt.Value<string>("Influence").ToEnum(InfluenceLevel.Unknown);
                Reputation = evt.Value<string>("Reputation").ToEnum(ReputationLevel.Unknown);

                Commodity = evt.Value<string>("Commodity");
                Commodity_Localised = evt.Value<string>("Commodity_Localised");
                Count = evt.Value<int?>("Count");
                Target = evt.Value<string>("Target");
                TargetType = evt.Value<string>("TargetType");
                TargetFaction = evt.Value<string>("TargetFaction");
                KillCount = evt.Value<int?>("KillCount");
                Expiry = evt.Value<DateTime?>("Expiry");
                DestinationSystem = evt.Value<string>("DestinationSystem");
                DestinationStation = evt.Value<string>("DestinationStation");
                PassengerCount = evt.Value<int?>("PassengerCount");
                PassengerVIPs = evt.Value<bool?>("PassengerVIPs");
                PassengerWanted = evt.Value<bool?>("PassengerWanted");
                PassengerType = evt.Value<string>("PassengerType");
            }

            public int MissionId { get; set; }
            public string Name { get; set; }
            public string Faction { get; set; }
            public InfluenceLevel Influence { get; set; }
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
