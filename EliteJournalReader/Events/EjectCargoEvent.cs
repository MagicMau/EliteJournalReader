using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written:
    //Parameters:
    //•	Type: cargo type
    //•	Count: number of units
    //•	Abandoned: whether ‘abandoned’
    public class EjectCargoEvent : JournalEvent<EjectCargoEvent.EjectCargoEventArgs>
    {
        public EjectCargoEvent() : base("EjectCargo") { }

        public class EjectCargoEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Type = evt.Value<string>("Type");
                Count = evt.Value<int>("Count");
                Abandoned = evt.Value<bool>("Abandoned");
            }

            public string Type { get; set; }
            public int Count { get; set; }
            public bool Abandoned { get; set; }
        }
    }
}
