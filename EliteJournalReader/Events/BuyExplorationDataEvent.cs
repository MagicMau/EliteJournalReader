using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when buying system data via the galaxy map
    //Parameters: 
    //•	System
    //•	Cost
    public class BuyExplorationDataEvent : JournalEvent<BuyExplorationDataEvent.BuyExplorationDataEventArgs>
    {
        public BuyExplorationDataEvent() : base("BuyExplorationData") { }

        public class BuyExplorationDataEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                System = evt.Value<string>("System");
                Cost = evt.Value<int>("Cost");
            }

            public string System { get; set; }
            public int Cost { get; set; }
        }
    }
}
