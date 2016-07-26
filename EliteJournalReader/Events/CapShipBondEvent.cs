using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: The player has been rewarded for a capital ship combat
    //Parameters:
    //•	Reward: value of award
    //•	AwardingFaction
    //•	VictimFaction
    public class CapShipBondEvent : JournalEvent<CapShipBondEvent.CapShipBondEventArgs>
    {
        public CapShipBondEvent() : base("CapShipBond") { }

        public class CapShipBondEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                AwardingFaction = evt.Value<string>("AwardingFaction");
                VictimFaction = evt.Value<string>("VictimFaction");
                Reward = evt.Value<int>("Reward");
            }

            public string AwardingFaction { get; set; }
            public string VictimFaction { get; set; }
            public int Reward { get; set; }
        }
    }
}
