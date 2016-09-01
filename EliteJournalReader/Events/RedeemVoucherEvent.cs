using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when claiming payment for combat bounties and bonds
    //Parameters:
    //•	Type
    //•	Amount
    public class RedeemVoucherEvent : JournalEvent<RedeemVoucherEvent.RedeemVoucherEventArgs>
    {
        public RedeemVoucherEvent() : base("RedeemVoucher") { }

        public class RedeemVoucherEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Type = evt.Value<string>("Type");
                Amount = evt.Value<int>("Amount");
                BrokerPercentage = evt.Value<decimal?>("BrokerPercentage");
            }

            public string Type { get; set; }
            public int Amount { get; set; }
            public decimal? BrokerPercentage { get; set; }
        }
    }
}
