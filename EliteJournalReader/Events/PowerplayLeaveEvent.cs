using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when leaving a power
    //Parameters:
    //•	Power
    public class PowerplayLeaveEvent : JournalEvent<PowerplayLeaveEvent.PowerplayLeaveEventArgs>
    {
        public PowerplayLeaveEvent() : base("PowerplayLeave") { }

        public class PowerplayLeaveEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Power = evt.Value<string>("Power");
            }

            public string Power { get; set; }
        }
    }
}
