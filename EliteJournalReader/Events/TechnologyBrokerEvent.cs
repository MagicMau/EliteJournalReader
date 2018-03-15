using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //  When written: when using the Technology Broker to unlock new purchasable technology
    //Parameters:
    //•	ItemUnlocked: the name of the new item unlocked(available in Outfitting)
    //•	Ingredients: array of objects
    //o   Name: name of item
    //o   Count: number of items used
    public class TechnologyBrokerEvent : JournalEvent<TechnologyBrokerEvent.TechnologyBrokerEventArgs>
    {
        public TechnologyBrokerEvent() : base("TechnologyBroker") { }

        public class TechnologyBrokerEventArgs : JournalEventArgs
        {
            public string ItemUnlocked { get; set; }
            public string ItemUnlocked_Localised { get; set; }
            public List<Material> Ingredients { get; set; }
        }
    }
}
