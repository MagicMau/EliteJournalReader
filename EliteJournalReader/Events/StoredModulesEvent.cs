namespace EliteJournalReader.Events
{
    //    When written: when first visiting Outfitting, and when the set of stored modules has changed
    //Parameters:
    //�	MarketID: current market
    //�	Items: (array of objects)
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
            public class StoredModule
            {
                public string Name { get; set; }
                public string Name_Localised { get; set; }
                public string StarSystem { get; set; }
                public long MarketID { get; set; }
                public int StorageSlot { get; set; }
                public int TransferCost { get; set; }
                public int TransferTime { get; set; }
                public string EngineerModifications { get; set; }
                public int Level { get; set; }
                public double Quality { get; set; }
                public string ExperimentalEffect { get; set; }
                public int BuyPrice { get; set; }
                public bool InTransit { get; set; }
                public bool Hot { get; set; }
            }

            public string StarSystem { get; set; }
            public string StationName { get; set; }
            public long MarketID { get; set; }
            public StoredModule[] Items { get; set; }
        }
    }
}
