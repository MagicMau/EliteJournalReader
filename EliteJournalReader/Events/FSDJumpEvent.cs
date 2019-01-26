using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EliteJournalReader.Events
{
    //When written: when jumping from one star system to another
    //Parameters:
    //•	StarSystem: name of destination starsystem
    //•	StarPos: star position, as a Json array [x, y, z], in light years
    //•	Body: star’s body name
    //•	JumpDist: distance jumped
    //•	FuelUsed
    //•	FuelLevel
    //•	BoostUsed: whether FSD boost was used
    //•	SystemFaction: system controlling faction
    //•	FactionState
    //•	SystemAllegiance
    //•	SystemEconomy
    //•	SystemSecondEconomy
    //•	SystemGovernment
    //•	SystemSecurity
    //•	Population
    //•	Wanted
    //•	Factions: an array of info for the local minor factions
    //    o Name
    //    o FactionState
    //    o Government
    //    o Influence
    //    o MyReputation
    //    o PendingStates: array(if any) with State name and Trend value
    //    o RecovingStates: array(if any)with State name and Trend value
    //    o ActiveStates: array with State names and Trend value
    //If the player is pledged to a Power in Powerplay, and the star system is involved in powerplay,
    //•	Powers: a json array with the names of any powers contesting the system, or the name of the controlling power
    //•	PowerplayState: the system state – one of("InPrepareRadius", "Prepared", "Exploited", "Contested", "Controlled", "Turmoil", "HomeSystem")
    public class FSDJumpEvent : JournalEvent<FSDJumpEvent.FSDJumpEventArgs>
    {
        public FSDJumpEvent() : base("FSDJump") { }

        public class FSDJumpEventArgs : JournalEventArgs
        {
            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }

            [JsonConverter(typeof(SystemPositionConverter))]
            public SystemPosition StarPos { get; set; }

            public string Body { get; set; }
            public double JumpDist { get; set; }
            public double FuelUsed { get; set; }
            public double FuelLevel { get; set; }
            public bool BoostUsed { get; set; }
            public Faction SystemFaction { get; set; }
            public string SystemAllegiance { get; set; }
            public string SystemEconomy { get; set; }
            public string SystemEconomy_Localised { get; set; }
            public string SystemSecondEconomy { get; set; }
            public string SystemSecondEconomy_Localised { get; set; }
            public string SystemGovernment { get; set; }
            public string SystemGovernment_Localised { get; set; }
            public string SystemSecurity { get; set; }
            public string SystemSecurity_Localised { get; set; }
            public long Population { get; set; }
            public bool Wanted { get; set; }
            public string[] Powers { get; set; }

            [JsonConverter(typeof(ExtendedStringEnumConverter<PowerplayState>))]
            public PowerplayState PowerplayState { get; set; }

            public List<Faction> Factions { get; set; }
        }
    }
}
