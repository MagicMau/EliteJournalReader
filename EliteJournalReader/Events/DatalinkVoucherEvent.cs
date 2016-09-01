using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when scanning a datalink generates a reward
    //Parameters:
    //•	Reward: value in credits
    //•	VictimFaction
    //•	PayeeFaction
    public class DatalinkVoucherEvent : JournalEvent<DatalinkVoucherEvent.DatalinkVoucherEventArgs>
    {
        public DatalinkVoucherEvent() : base("DatalinkVoucher") { }

        public class DatalinkVoucherEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Reward = evt.Value<decimal>("Reward");
                VictimFaction = evt.Value<string>("VictimFaction");
                PayeeFaction = evt.Value<string>("PayeeFaction");
            }

            public decimal Reward { get; set; }
            public string VictimFaction { get; set; }
            public string PayeeFaction { get; set; }
        }
    }
}
