using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when scanning a data link
    //Parameters:
    //•	Message: message from data link
    public class ContinuedEvent : JournalEvent<ContinuedEvent.ContinuedEventArgs>
    {
        public ContinuedEvent() : base("Continued") { }

        public class ContinuedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Part = evt.Value<int>("Part");
            }

            public int Part { get; set; }
        }
    }
}
