namespace EliteJournalReader.Events
{
    //When Written: when buying system data via the galaxy map
    //Parameters: 
    //�	System
    //�	Cost
    public class BuyExplorationDataEvent : JournalEvent<BuyExplorationDataEvent.BuyExplorationDataEventArgs>
    {
        public BuyExplorationDataEvent() : base("BuyExplorationData") { }

        public class BuyExplorationDataEventArgs : JournalEventArgs
        {
            public string System { get; set; }
            public int Cost { get; set; }
        }
    }
}
