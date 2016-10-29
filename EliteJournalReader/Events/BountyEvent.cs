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
            public struct FactionReward
            {
                public string Faction;
                public int Reward;
            }

            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Rewards = evt["Rewards"].ToObject<FactionReward[]>();
                VictimFaction = evt.Value<string>("VictimFaction");
                TotalReward = evt.Value<int>("TotalReward");
                SharedWithOthers = evt.Value<bool?>("SharedWithOthers");
            }

            public FactionReward[] Rewards { get; set; }
            public string VictimFaction { get; set; }
            public int TotalReward { get; set; }
            public bool? SharedWithOthers { get; set; }
        }
    }
}
