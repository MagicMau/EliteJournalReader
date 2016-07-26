using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when purchasing ammunition
    //Parameters:
    //•	Cost
    public class BuyAmmoEvent : JournalEvent<BuyAmmoEvent.BuyAmmoEventArgs>
    {
        public BuyAmmoEvent() : base("BuyAmmo") { }

        public class BuyAmmoEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Cost = evt.Value<int>("Cost");
            }

            public int Cost { get; set; }
        }
    }
}
