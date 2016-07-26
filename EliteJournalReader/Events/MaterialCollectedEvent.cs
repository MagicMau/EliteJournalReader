using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: whenever materials are collected 
    //Parameters: 
    //•	Category: type of material (Raw/Encoded/Manufactured)
    //•	Name: name of material
    public class MaterialCollectedEvent : JournalEvent<MaterialCollectedEvent.MaterialCollectedEventArgs>
    {
        public MaterialCollectedEvent() : base("MaterialCollected") { }

        public class MaterialCollectedEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Category = evt.Value<string>("Category");
                Name = evt.Value<string>("Name");
            }

            public string Category { get; set; }
            public string Name { get; set; }
        }
    }
}
