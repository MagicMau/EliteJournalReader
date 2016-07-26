using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when the player restarts after death
    //Parameters:
    //•	Option: the option selected on the insurance rebuy screen
    //•	Cost: the price paid
    //•	Bankrupt: whether the commander declared bankruptcy
    public class ResurrectEvent : JournalEvent<ResurrectEvent.ResurrectEventArgs>
    {
        public ResurrectEvent() : base("Resurrect") { }

        public class ResurrectEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Option = evt.Value<string>("Option");
                Cost = evt.Value<int>("Cost");
                Bankrupt = evt.Value<bool>("Bankrupt");
            }

            public string Option { get; set; }
            public int Cost { get; set; }
            public bool Bankrupt { get; set; }
        }
    }
}
