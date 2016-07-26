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
    //•	KillerNames: a JSON array of player names
    //•	KillerShips: a JSON array of ship names
    //•	KillerRanks: a JSON array of rank names
    public class DiedEvent : JournalEvent<DiedEvent.DiedEventArgs>
    {
        public DiedEvent() : base("Died") { }

        public class DiedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);

                string killerName = evt.Value<string>("KillerName");
                if (string.IsNullOrEmpty(killerName))
                {
                    // it was a wing
                    KillerNames = evt.Value<JArray>("KillerNames").Values<string>().ToArray();
                    KillerShips = evt.Value<JArray>("KillerShips").Values<string>().ToArray();
                    KillerRanks = evt.Value<JArray>("KillerRanks").Values<string>().ToArray();
                }
                else
                {
                    // it was an individual
                    KillerNames = new string[1] { killerName };
                    KillerShips = new string[1] { evt.Value<string>("KillerShip") };
                    KillerRanks = new string[1] { evt.Value<string>("KillerRank") };
                }
            }

            public string[] KillerNames { get; set; }
            public string[] KillerShips { get; set; }
            public string[] KillerRanks { get; set; }
        }
    }
}
