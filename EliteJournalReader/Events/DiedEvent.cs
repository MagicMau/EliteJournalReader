using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: player was killed
    //Parameters: 
    //�	KillerName
    //�	KillerShip
    //�	KillerRank
    //When written: player was killed by a wing
    //Parameters:
    //�	Killers: a JSON array of objects containing player name, ship, and rank
    public class DiedEvent : JournalEvent<DiedEvent.DiedEventArgs>
    {
        public DiedEvent() : base("Died") { }

        public class DiedEventArgs : JournalEventArgs
        {
            public class Killer
            {
                public string Name { get; set; }
                public string Ship { get; set; }
                public string Rank { get; set; }
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
