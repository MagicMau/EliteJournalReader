using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player is awarded a ApproachSettlement for a kill
    //Parameters: 
    //•	Faction: the faction awarding the ApproachSettlement
    //•	Reward: the reward value
    //•	VictimFaction: the victim’s faction
    //•	SharedWithOthers: whether shared with other players
    public class ApproachSettlementEvent : JournalEvent<ApproachSettlementEvent.ApproachSettlementEventArgs>
    {
        public ApproachSettlementEvent() : base("ApproachSettlement") { }

        public class ApproachSettlementEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");
            }

            public string Name { get; set; }
        }
    }
}
