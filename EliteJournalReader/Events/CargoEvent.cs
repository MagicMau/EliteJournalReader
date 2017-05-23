using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: at startup, when loading from main menu
    //Parameters:
    //•	Inventory: array of cargo, with Name and Count for each
    public class CargoEvent : JournalEvent<CargoEvent.CargoEventArgs>
    {
        public CargoEvent() : base("Cargo") { }

        public class CargoEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);

                var inventory = evt["Inventory"];
                if (inventory != null)
                {
                    if (inventory.Type == JTokenType.Object)
                        Inventory = inventory.ToObject<Dictionary<string, int>>();
                    else if (inventory.Type == JTokenType.Array)
                    {
                        Inventory = new Dictionary<string, int>();
                        foreach (var jo in (JArray)inventory)
                        {
                            Inventory[jo.Value<string>("Name")] = jo.Value<int>("Count");
                        }
                    }
                }
            }

            public Dictionary<string, int> Inventory { get; set; }
        }
    }
}
