using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when refuelling (full tank)
    //Parameters:
    //•	Cost: cost of fuel
    //•	Amount: tons of fuel purchased
    public class RefuelAllEvent : JournalEvent<RefuelAllEvent.RefuelAllEventArgs>
    {
        public RefuelAllEvent() : base("RefuelAll") { }

        public class RefuelAllEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Cost = evt.Value<int>("Cost");
                Amount = evt.Value<int>("Amount");
            }

            public int Cost { get; set; }
            public int Amount { get; set; }
        }
    }
}
