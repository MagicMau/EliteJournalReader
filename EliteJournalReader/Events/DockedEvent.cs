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
            public string StarSystem { get; set; }
            public string StationName { get; set; }
            public long SystemAddress { get; set; }
            public long MarketID { get; set; }
            public string StationType { get; set; }
            public bool CockpitBreach { get; set; } = false;
            public string StationFaction { get; set; }
            public string FactionState { get; set; }
            public string StationAllegiance { get; set; }
            public string StationEconomy { get; set; }
            public string StationEconomy_Localised { get; set; }
            public string StationGovernment { get; set; }
            public string StationGovernment_Localised { get; set; }
            public double? DistFromStarLS { get; set; }
            public string[] StationServices { get; set; }
            public bool Wanted { get; set; } = false;
        }
    }
}
