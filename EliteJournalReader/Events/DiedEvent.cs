using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player was killed
    //Parameters: 
    //•	KillerName
    //•	KillerShip
    //•	KillerRank
    //When written: player was killed by a wing
    //Parameters:
    //•	Killers: a JSON array of objects containing player name, ship, and rank
    public class DiedEvent : JournalEvent<DiedEvent.DiedEventArgs>
    {
        public DiedEvent() : base("Died") { }

        public class DiedEventArgs : JournalEventArgs
        {
            public struct Killer
            {
                public string Name;
                public string Ship;
                public string Rank;
            }

            public string KillerName { get; set; }
            public string KillerShip { get; set; }
            public string KillerRank { get; set; }

            public override void PostProcess(JObject evt, JournalWatcher journalWatcher)
            {
                if (!string.IsNullOrEmpty(KillerName))
                {
                    // it was an individual
                    Killers = new Killer[1]
                    {
                        new Killer
                        {
                            Name = KillerName,
                            Ship = KillerShip,
                            Rank = KillerRank,
                        }
                    };
                }
            }

            public Killer[] Killers { get; set; }
        }
    }
}
