using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player is awarded a bounty for a kill
    //Parameters: 
    //•	Faction: the faction awarding the bounty
    //•	Reward: the reward value
    //•	VictimFaction: the victim’s faction
    //•	SharedWithOthers: whether shared with other players
    public class BountyEvent : JournalEvent<BountyEvent.BountyEventArgs>
    {
        public BountyEvent() : base("Bounty") { }

        public class BountyEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Faction = evt.Value<string>("Faction");
                Reward = evt.Value<int>("Reward");
                VictimFaction = evt.Value<string>("VictimFaction");
                SharedWithOthers = evt.Value<bool?>("SharedWithOthers");
            }

            public string Faction { get; set; }
            public int Reward { get; set; }
            public string VictimFaction { get; set; }
            public bool? SharedWithOthers { get; set; }
        }
    }
}
