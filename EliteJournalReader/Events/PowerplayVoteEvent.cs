using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when voting for a system expansion
    //Parameters:
    //•	Power
    //•	Votes
    //•	System
    public class PowerplayVoteEvent : JournalEvent<PowerplayVoteEvent.PowerplayVoteEventArgs>
    {
        public PowerplayVoteEvent() : base("PowerplayVote") { }

        public class PowerplayVoteEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Power = evt.Value<string>("Power");
                System = evt.Value<string>("System");
                Votes = evt.Value<int>("Votes");
            }

            public string Power { get; set; }
            public string System { get; set; }
            public int Votes { get; set; }
        }
    }
}
