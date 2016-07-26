using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player was interdicted by player or npc
    //Parameters: 
    //•	Submitted: true or false
    public class InterdictedEvent : JournalEvent<InterdictedEvent.InterdictedEventArgs>
    {
        public InterdictedEvent() : base("Interdicted") { }

        public class InterdictedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Submitted = evt.Value<bool>("Submitted");
            }

            public bool Submitted { get; set; }
        }
    }
}
