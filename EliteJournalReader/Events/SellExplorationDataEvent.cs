using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when selling exploration data in Cartographics
    //Parameters:
    //•	Systems: JSON array of system names
    //•	Discovered: JSON array of discovered bodies
    //•	BaseValue: value of systems
    //•	Bonus: bonus for first discoveries
    public class SellExplorationDataEvent : JournalEvent<SellExplorationDataEvent.SellExplorationDataEventArgs>
    {
        public SellExplorationDataEvent() : base("SellExplorationData") { }

        public class SellExplorationDataEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Systems = evt.Value<JArray>("Systems").Values<string>().ToArray();
                Discovered = evt.Value<JArray>("Discovered").Values<string>().ToArray();
                BaseValue = evt.Value<int>("BaseValue");
                Bonus = evt.Value<int>("Bonus");
            }

            public string[] Systems { get; set; }
            public string[] Discovered { get; set; }
            public int BaseValue { get; set; }
            public int Bonus { get; set; }
        }
    }
}
