using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class LoadGameEvent : JournalEvent<LoadGameEvent.LoadGameEventArgs>
    {
        public LoadGameEvent() : base("LoadGame") { }

        public class LoadGameEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Commander = evt.StringValue("Commander");
                Ship = evt.StringValue("Ship");
                StartLanded = evt["StartLanded"]?.Value<bool>();
                StartDead = evt["StartDead"]?.Value<bool>();
            }

            public string Commander { get; set; }
            public string Ship { get; set; }
            public bool? StartLanded { get; set; }
            public bool? StartDead { get; set; }
        }
    }
}
