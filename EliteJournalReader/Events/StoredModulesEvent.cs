using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //    When written: when first visiting Outfitting, and when the set of stored modules has changed
    //Parameters:
    //•	MarketID: current market
    //•	Items: (array of objects)
    //o Name
    //o StarSystem
    //o MarketID: where the module is stored
    //o   StorageSlot
    //o   TransferCost
    //o   TransferTime
    //o   EngineerModifications: (recipe name)
    //o   InTransit:bool
    //The InTransit value only appears(with value true) if the module is being transferred.
    //In this case, the system, market, transfer cost and transfer time are not written.
    public class StoredModulesEvent : JournalEvent<StoredModulesEvent.StoredModulesEventArgs>
    {
        public StoredModulesEvent() : base("StoredModules") { }

        public class StoredModulesEventArgs : JournalEventArgs
        {
            public struct StoredModule
            {
                public string Name;
                public string StarSystem;
                public long MarketID;
                public string StorageSlot;
                public int TransferCost;
                public int TransferTime;
                public string EngineerModifications;
                public bool InTransit;
            }

            public long MarketID { get; set; }
            public List<StoredModule> Items { get; set; }
        }
    }
}
