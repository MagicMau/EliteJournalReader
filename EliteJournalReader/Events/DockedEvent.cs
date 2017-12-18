using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when landing at landing pad in a space station, outpost, or surface settlement
    //Parameters:
    //•	StationName: name of station
    //•	StationType: type of station
    //•	StarSystem: name of system
    //•	CockpitBreach:true (only if landing with breached cockpit)
    //•	StationFaction: station’s controlling faction
    //•	FactionState
    //•	StationAllegiance
    //•	StationEconomy
    //•	StationGovernment
    //•	DistFromLS
    //•	StationServices
    //    StationServices can include: 
    //          Dock, Autodock, BlackMarket, Commodities, Contacts, Exploration, Initiatives, Missions, 
    //          Outfitting,CrewLounge, Rearm, Refuel, Repair, Shipyard, Tuning, Workshop, MissionsGenerated, 
    //          Facilitator, Research, FlightController, StationOperations, OnDockMission, Powerplay, SearchAndRescue,

    public class DockedEvent : JournalEvent<DockedEvent.DockedEventArgs>
    {
        public DockedEvent() : base("Docked") { }

        public class DockedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StarSystem = evt.Value<string>("StarSystem");
                StationName = evt.Value<string>("StationName");
                StationType = evt.Value<string>("StationType");
                CockpitBreach = evt.Value<bool?>("CockpitBreach");
                Faction = evt.Value<string>("StationFaction");
                FactionState = evt.Value<string>("FactionState");
                Allegiance = evt.Value<string>("StationAllegiance");
                Economy = evt.Value<string>("StationEconomy");
                Economy_Localised = evt.Value<string>("StationEconomy_Localised");
                Government = evt.Value<string>("StationGovernment");
                Government_Localised = evt.Value<string>("StationGovernment_Localised");
                DistFromStarLS = evt.Value<double?>("DistFromStarLS");
                StationServices = evt["StationServices"]?.ToObject<string[]>();
            }

            public string StarSystem { get; set; }
            public string StationName { get; set; }
            public string StationType { get; set; }
            public bool? CockpitBreach { get; set; }
            public string Faction { get; set; }
            public string FactionState { get; set; }
            public string Allegiance { get; set; }
            public string Economy { get; set; }
            public string Economy_Localised { get; set; }
            public string Government { get; set; }
            public string Government_Localised { get; set; }
            public double? DistFromStarLS { get; set; }
            public string[] StationServices { get; set; }
        }
    }
}
