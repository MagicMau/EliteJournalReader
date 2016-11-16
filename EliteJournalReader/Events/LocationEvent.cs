using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup, or when being resurrected at a station
    //Parameters:
    //•	StarSystem: name of destination starsystem
    //•	StarPos: star position, as a Json array [x, y, z], in light years
    //•	Body: star’s body name
    //•	Docked: true (if docked)
    //•	StationName: station name, (if docked)
    //•	StationType: (if docked)
    //•	Faction: star system controlling faction
    //•	FactionState
    //•	SystemAllegiance
    //•	SystemEconomy
    //•	SystemGovernment
    //•	SystemSecurity
    public class LocationEvent : JournalEvent<LocationEvent.LocationEventArgs>
    {
        public LocationEvent() : base("Location") { }

        public class LocationEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StarSystem = evt.Value<string>("StarSystem");
                StarPos = new Position(evt.Value<JArray>("StarPos"));
                Body = evt.Value<string>("Body");
                BodyType = evt.Value<string>("BodyType").ToEnum(BodyType.Unknown);
                Docked = evt.Value<bool?>("Docked") ?? false;
                StationName = evt.Value<string>("StationName");
                StationType = evt.Value<string>("StationType");
                Faction = evt.Value<string>("Faction");
                FactionState = evt.Value<string>("FactionState");
                Allegiance = evt.Value<string>("SystemAllegiance");
                Economy = evt.Value<string>("SystemEconomy");
                Economy_Localised = evt.Value<string>("SystemEconomy_Localised");
                Government = evt.Value<string>("SystemGovernment");
                Government_Localised = evt.Value<string>("SystemGovernment_Localised");
                Security = evt.Value<string>("SystemSecurity");
                Security_Localised = evt.Value<string>("SystemSecurity_Localised");
                Powers = evt["Powers"]?.ToObject<string[]>();
                string power = evt.Value<string>("Power");
                if (!string.IsNullOrEmpty(power))
                    Powers = new string[] { power };
                PowerplayState = evt.Value<string>("PowerplayState").ToEnum(PowerplayState.Unknown);
            }

            public string StarSystem { get; set; }
            public Position StarPos { get; set; }
            public string Body { get; set; }
            public BodyType BodyType { get; set; }
            public bool Docked { get; set; }
            public string StationName { get; set; }
            public string StationType { get; set; }
            public string Faction { get; set; }
            public string FactionState { get; set; }
            public string Allegiance { get; set; }
            public string Economy { get; set; }
            public string Economy_Localised { get; set; }
            public string Government { get; set; }
            public string Government_Localised { get; set; }
            public string Security { get; set; }
            public string Security_Localised { get; set; }
            public string[] Powers { get; set; }
            public PowerplayState PowerplayState { get; set; }
        }
    }
}
