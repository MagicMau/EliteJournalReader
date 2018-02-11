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
    //•	BrokerPercentage (present if paid via a Broker)
    public class PayFinesEvent : JournalEvent<PayFinesEvent.PayFinesEventArgs>
    {
        public PayFinesEvent() : base("PayFines") { }

        public class PayFinesEventArgs : JournalEventArgs
        {
            public int Amount { get; set; }
            public double? BrokerPercentage { get; set; }
        }
    }
}
