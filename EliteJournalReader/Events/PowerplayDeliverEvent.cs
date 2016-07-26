using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when delivering powerplay commodities
    //Parameters:
    //•	Power
    //•	Type
    //•	Count
    public class PowerplayDeliverEvent : JournalEvent<PowerplayDeliverEvent.PowerplayDeliverEventArgs>
    {
        public PowerplayDeliverEvent() : base("PowerplayDeliver") { }

        public class PowerplayDeliverEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Power = evt.Value<string>("Power");
                Type = evt.Value<string>("Type");
                Count = evt.Value<int>("Count");
            }

            public string Power { get; set; }
            public string Type { get; set; }
            public int Count { get; set; }
        }
    }
}
