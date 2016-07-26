using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when nominating a system for expansion
    //Parameters:
    //•	Power
    //•	System
    //•	Votes
    public class PowerplayNominateEvent : JournalEvent<PowerplayNominateEvent.PowerplayNominateEventArgs>
    {
        public PowerplayNominateEvent() : base("PowerplayNominate") { }

        public class PowerplayNominateEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Power = evt.Value<string>("Power");
                System = evt.Value<string>("System");
                Votes = evt.Value<int>("Votes");
            }

            public string Power { get; set; }
            public string System { get; set; }
            public int Votes { get; set; }
        }
    }
}
