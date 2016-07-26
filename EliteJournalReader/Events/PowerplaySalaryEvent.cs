using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when receiving salary payment from a power
    //Parameters:
    //•	Power
    //•	Amount
    public class PowerplaySalaryEvent : JournalEvent<PowerplaySalaryEvent.PowerplaySalaryEventArgs>
    {
        public PowerplaySalaryEvent() : base("PowerplaySalary") { }

        public class PowerplaySalaryEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Power = evt.Value<string>("Power");
                Amount = evt.Value<int>("Amount");
            }

            public string Power { get; set; }
            public int Amount { get; set; }
        }
    }
}
