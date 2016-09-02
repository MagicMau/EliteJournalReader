using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
    //•	Faction: system controlling faction
    //•	FactionState
    //•	Allegiance
    //•	Economy
    //•	Government
    //•	Security
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
                Faction = evt.Value<string>("Faction");
                FactionState = evt.Value<string>("FactionState");
                Allegiance = evt.Value<string>("Allegiance");
                Economy = evt.Value<string>("Economy");
                Economy_Localised = evt.Value<string>("Economy_Localised");
                Government = evt.Value<string>("Government");
                Government_Localised = evt.Value<string>("Government_Localised");
                Security = evt.Value<string>("Security");
                Security_Localised = evt.Value<string>("Security_Localised");
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
        }
    }
}
