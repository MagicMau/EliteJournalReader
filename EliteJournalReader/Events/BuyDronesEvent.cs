namespace EliteJournalReader.Events
{
    //When Written: when purchasing drones
    //Parameters:
    //�	Type
    //�	Count
    //�	BuyPrice
    //�	TotalCost
    public class BuyDronesEvent : JournalEvent<BuyDronesEvent.BuyDronesEventArgs>
    {
        public BuyDronesEvent() : base("BuyDrones") { }

        public class BuyDronesEventArgs : JournalEventArgs
        {
            public string Type { get; set; }
            public string Type_Localised { get; set; }
            public int Count { get; set; }
            public int BuyPrice { get; set; }
            public int TotalCost { get; set; }
        }
    }
}
