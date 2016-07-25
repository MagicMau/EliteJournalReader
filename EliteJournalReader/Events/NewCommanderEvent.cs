using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class NewCommanderEvent : JournalEvent<NewCommanderEvent.NewCommanderEventArgs>
    {
        public NewCommanderEvent() : base("NewCommander") { }

        public class NewCommanderEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.StringValue("Name");
                Package = evt.StringValue("Package");
            }

            public string Name { get; set; }
            public string Package { get; set; }
        }
    }
}
