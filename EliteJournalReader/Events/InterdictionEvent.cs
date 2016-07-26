using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player has (attempted to) interdict another player or npc
    //Parameters: 
    //•	Success
    public class InterdictionEvent : JournalEvent<InterdictionEvent.InterdictionEventArgs>
    {
        public InterdictionEvent() : base("Interdiction") { }

        public class InterdictionEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Success = evt.Value<bool>("Success");
            }

            public bool Success { get; set; }
        }
    }
}
