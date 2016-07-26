using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when scooping cargo from space or planet surface
    //Parameters:
    //•	Type: cargo type
    //•	Stolen: whether stolen goods
    public class CollectCargoEvent : JournalEvent<CollectCargoEvent.CollectCargoEventArgs>
    {
        public CollectCargoEvent() : base("CollectCargo") { }

        public class CollectCargoEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Type = evt.Value<string>("Type");
                Stolen = evt.Value<bool>("Stolen");
            }

            public string Type { get; set; }
            public bool Stolen { get; set; }
        }
    }
}
