using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class LoadGameEvent : JournalEvent<LoadGameEvent.LoadGameEventArgs>
    {
        //When written: at startup, when loading from main menu into game
        //Parameters:
        //•	Commander: commander name
        //•	Ship: current ship type
        //•	ShipID: ship id number
        //•	StartLanded: true (only present if landed)
        //•	StartDead:true (only present if starting dead: see “Resurrect”)
        //•	GameMode: Open, Solo or Group
        //•	Group: name of group (if in a group)
        //•	Credits: current credit balance
        //•	Loan: current loan
        public LoadGameEvent() : base("LoadGame") { }

        public class LoadGameEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Commander = evt.Value<string>("Commander");
                Ship = evt.Value<string>("Ship");
                ShipId = evt.Value<int>("ShipID");
                StartLanded = evt.Value<bool?>("StartLanded");
                StartDead = evt.Value<bool?>("StartDead");
                GameMode = evt.Value<string>("GameMode").ToEnum(GameMode.Unknown);
                Group = evt.Value<string>("Group");
                Credits = evt.Value<int>("Credits");
                Loan = evt.Value<int>("Loan");
            }

            public string Commander { get; set; }
            public string Ship { get; set; }
            public int ShipId { get; set; }
            public bool? StartLanded { get; set; }
            public bool? StartDead { get; set; }
            public GameMode GameMode { get; set; }
            public string Group { get; set; }
            public int Credits { get; set; }
            public int Loan { get; set; }
        }
    }
}
