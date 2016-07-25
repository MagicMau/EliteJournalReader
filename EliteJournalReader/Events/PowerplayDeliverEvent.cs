using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class PowerplayDeliverEvent : JournalEvent<PowerplayDeliverEvent.PowerplayDeliverEventArgs>
    {
        public PowerplayDeliverEvent() : base("PowerplayDeliver") { }

        public class PowerplayDeliverEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                GameVersion = evt.StringValue("gameversion");
                Build = evt.StringValue("build");
            }

            public string GameVersion { get; set; }
            public string Build { get; set; }
        }
    }
}
