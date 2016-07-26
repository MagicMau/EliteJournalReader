using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when applying an engineer’s upgrade to a module
    //Parameters:
    //•	Engineer: name of engineer
    //•	Blueprint: blueprint being applied
    //•	Level: crafting level
    //•	Override: whether overriding special effect
    public class EngineerApplyEvent : JournalEvent<EngineerApplyEvent.EngineerApplyEventArgs>
    {
        public EngineerApplyEvent() : base("EngineerApply") { }

        public class EngineerApplyEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Engineer = evt.Value<string>("Engineer");
                Blueprint = evt.Value<string>("Blueprint");
                Level = evt.Value<int>("Level");
                Override = evt.Value<bool?>("Override");
            }

            public string Engineer { get; set; }
            public string Blueprint { get; set; }
            public int Level { get; set; }
            public bool? Override { get; set; }
        }
    }
}
