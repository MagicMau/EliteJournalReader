using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when a mission has failed
    //Parameters:
    //•	Name: name of mission
    public class MissionFailedEvent : JournalEvent<MissionFailedEvent.MissionFailedEventArgs>
    {
        public MissionFailedEvent() : base("MissionFailed") { }

        public class MissionFailedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");
            }

            public string Name { get; set; }
        }
    }
}
