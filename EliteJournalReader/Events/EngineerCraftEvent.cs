using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when requesting an engineer upgrade
    //Parameters:
    //•	Engineer: name of engineer
    //•	Blueprint: name of blueprint
    //•	Level: crafting level
    //•	Ingredients: JSON object with names and quantities of materials required
    public class EngineerCraftEvent : JournalEvent<EngineerCraftEvent.EngineerCraftEventArgs>
    {
        public EngineerCraftEvent() : base("EngineerCraft") { }

        public class EngineerCraftEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Engineer = evt.Value<string>("Engineer");
                Blueprint = evt.Value<string>("Blueprint");
                Level = evt.Value<int>("Level");
                Ingredients = evt["Ingredients"]?.ToObject<Dictionary<string, int>>();
            }

            public string Engineer { get; set; }
            public string Blueprint { get; set; }
            public int Level { get; set; }
            public Dictionary<string, int> Ingredients { get; set; }
        }
    }
}
