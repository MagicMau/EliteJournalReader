using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player has (attempted to) interdict another player or npc
    //Parameters: 
    //•	Success : true or false
    //•	Interdicted: victim pilot name
    //•	IsPlayer: whether player or npc
    //•	CombatRank: if a player
    //•	Faction: if an npc
    //•	Power: if npc working for power
    public class InterdictionEvent : JournalEvent<InterdictionEvent.InterdictionEventArgs>
    {
        public InterdictionEvent() : base("Interdiction") { }

        public class InterdictionEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Success = evt.Value<bool>("Success");
                Interdicted = evt.Value<string>("Interdicted");
                IsPlayer = evt.Value<bool>("IsPlayer");
                CombatRank = (CombatRank)(evt.Value<int?>("CombatRank") ?? 0);
                Faction = evt.Value<string>("Faction");
                Power = evt.Value<string>("Power");
            }

            public bool Success { get; set; }
            public string Interdicted { get; set; }
            public bool IsPlayer { get; set; }
            public CombatRank CombatRank { get; set; }
            public string Faction { get; set; }
            public string Power { get; set; }
        }
    }
}
