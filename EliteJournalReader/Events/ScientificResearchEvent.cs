using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when contributing materials to a "research" community goal
    //Parameters:
    //•	Name: material name
    //•	Category
    //•	Count
    public class ScientificResearchEvent : JournalEvent<ScientificResearchEvent.ScientificResearchEventArgs>
    {
        public ScientificResearchEvent() : base("ScientificResearch") { }

        public class ScientificResearchEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");
                Category = evt.Value<string>("Category");
                Count = evt.Value<int>("Count");
            }

            public string Name { get; set; }
            public string Category { get; set; }
            public int Count { get; set; }
        }
    }
}
