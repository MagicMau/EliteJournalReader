using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: this player has joined a wing
    //Parameters:
    //•	Others: JSON array of other player names already in wing
    public class WingJoinEvent : JournalEvent<WingJoinEvent.WingJoinEventArgs>
    {
        public WingJoinEvent() : base("WingJoin") { }

        public class WingJoinEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Others = evt.Value<JArray>("Others").Values<string>().ToArray();
            }

            public string[] Others { get; set; }
        }
    }
}
