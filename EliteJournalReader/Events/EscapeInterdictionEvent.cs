using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: Player has escaped interdiction
    //Parameters: 
    //•	Interdictor: interdicting pilot name
    //•	IsPlayer: whether player or npc
    public class EscapeInterdictionEvent : JournalEvent<EscapeInterdictionEvent.EscapeInterdictionEventArgs>
    {
        public EscapeInterdictionEvent() : base("EscapeInterdiction") { }

        public class EscapeInterdictionEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Interdictor = evt.Value<string>("Interdictor");
                IsPlayer = evt.Value<bool>("IsPlayer");
            }

            public string Interdictor { get; set; }
            public bool IsPlayer { get; set; }
        }
    }
}
