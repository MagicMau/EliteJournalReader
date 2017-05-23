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

                var ingredients = evt["Ingredients"];
                if (ingredients != null)
                {
                    if (ingredients.Type == JTokenType.Object)
                        Ingredients = ingredients.ToObject<Dictionary<string, int>>();
                    else if (ingredients.Type == JTokenType.Array)
                    {
                        Ingredients = new Dictionary<string, int>();
                        foreach (var jo in (JArray)ingredients)
                        {
                            Ingredients[jo.Value<string>("Name")] = jo.Value<int>("Count");
                        }
                    }
                }

            }

            public string Engineer { get; set; }
            public string Blueprint { get; set; }
            public int Level { get; set; }
            public Dictionary<string, int> Ingredients { get; set; }
        }
    }
}
