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
        //•	Ship: current ship
        //•	StartLanded: true (only present if landed)
        //•	StartDead:true (only present if starting dead: see “Resurrect”)
        public LoadGameEvent() : base("LoadGame") { }

        public class LoadGameEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Commander = evt.Value<string>("Commander");
                Ship = evt.Value<string>("Ship");
                StartLanded = evt.Value<bool?>("StartLanded");
                StartDead = evt.Value<bool?>("StartDead");
            }

            public string Commander { get; set; }
            public string Ship { get; set; }
            public bool? StartLanded { get; set; }
            public bool? StartDead { get; set; }
        }
    }
}
