using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when paying fines
    //Parameters:
    //•	Amount
    public class PayFinesEvent : JournalEvent<PayFinesEvent.PayFinesEventArgs>
    {
        public PayFinesEvent() : base("PayFines") { }

        public class PayFinesEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Amount = evt.Value<int>("Amount");
            }

            public int Amount { get; set; }
        }
    }
}
