using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when receiving payment for powerplay combat
    //Parameters:
    //•	Power
    //•	Systems:[name,name]
    public class PowerplayVoucherEvent : JournalEvent<PowerplayVoucherEvent.PowerplayVoucherEventArgs>
    {
        public PowerplayVoucherEvent() : base("PowerplayVoucher") { }

        public class PowerplayVoucherEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Power = evt.Value<string>("Power");
                Systems = evt.Value<JArray>("Systems").Values<string>().ToArray();
            }

            public string Power { get; set; }
            public string[] Systems { get; set; }
        }
    }
}
