using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class ClearSavedGameEvent : JournalEvent<ClearSavedGameEvent.ClearSavedGameEventArgs>
    {
        public ClearSavedGameEvent() : base("ClearSavedGame") { }

        public class ClearSavedGameEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.StringValue("Name");
            }

            public string Name { get; set; }
        }
    }
}
