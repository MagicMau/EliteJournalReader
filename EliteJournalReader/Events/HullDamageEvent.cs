using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player was HullDamage by player or npc
    //Parameters: 
    //•	Submitted: true or false
    public class HullDamageEvent : JournalEvent<HullDamageEvent.HullDamageEventArgs>
    {
        public HullDamageEvent() : base("HullDamage") { }

        public class HullDamageEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Health = evt.Value<decimal>("Health");
            }

            public decimal Health { get; set; }
        }
    }
}
