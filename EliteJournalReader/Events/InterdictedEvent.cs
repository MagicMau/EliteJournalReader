using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player was interdicted by player or npc
    //Parameters: 
    //•	Submitted: true or false
    //•	Interdictor: interdicting pilot name
    //•	IsPlayer: whether player or npc
    //•	CombatRank: if player
    //•	Faction: if npc
    //•	Power: if npc working for a power
    public class InterdictedEvent : JournalEvent<InterdictedEvent.InterdictedEventArgs>
    {
        public InterdictedEvent() : base("Interdicted") { }

        public class InterdictedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Submitted = evt.Value<bool>("Submitted");
                Interdictor = evt.Value<string>("Interdictor");
                IsPlayer = evt.Value<bool>("IsPlayer");
                CombatRank = evt.Value<int?>("CombatRank") ?? 0;
                Faction = evt.Value<string>("Faction");
                Power = evt.Value<string>("Power");
            }

            public bool Submitted { get; set; }
            public string Interdictor { get; set; }
            public bool IsPlayer { get; set; }
            public int CombatRank { get; set; }
            public string Faction { get; set; }
            public string Power { get; set; }

        }
    }
}
