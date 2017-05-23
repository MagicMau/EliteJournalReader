using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using EliteJournalReader.Models;

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
    //•	SystemGovernment
    //•	SystemSecurity
    //•	Factions: an array of info for the local minor factions
    //    o Name
    //    o FactionState
    //    o Government
    //    o Influence
    //If the player is pledged to a Power in Powerplay, and the star system is involved in powerplay,
    //•	Powers: a json array with the names of any powers contesting the system, or the name of the controlling power
    //•	PowerplayState: the system state – one of("InPrepareRadius", "Prepared", "Exploited", "Contested", "Controlled", "Turmoil", "HomeSystem")
    public class FSDJumpEvent : JournalEvent<FSDJumpEvent.FSDJumpEventArgs>
    {
        public FSDJumpEvent() : base("FSDJump") { }

        public class FSDJumpEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StarSystem = evt.Value<string>("StarSystem");
                StarPos = new Position(evt.Value<JArray>("StarPos"));
                Body = evt.Value<string>("Body");
                JumpDist = evt.Value<double>("JumpDist");
                FuelUsed = evt.Value<double>("FuelUsed");
                FuelLevel = evt.Value<double>("FuelLevel");
                BoostUsed = evt.Value<bool>("BoostUsed");
                Faction = evt.Value<string>("SystemFaction");
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
                Factions = evt["Factions"]?.ToObject<List<Faction>>();
            }

            public string StarSystem { get; set; }
            public Position StarPos { get; set; }
            public string Body { get; set; }
            public double JumpDist { get; set; }
            public double FuelUsed { get; set; }
            public double FuelLevel { get; set; }
            public bool BoostUsed { get; set; }
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
            public List<Faction> Factions { get; set; }
        }
    }
}
