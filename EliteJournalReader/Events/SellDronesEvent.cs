namespace EliteJournalReader.Events
{
    //When Written: when selling unwanted drones back to the market
    //Parameters:
    //�	Type
    //�	Count
    //�	SellPrice
    //�	TotalSale
    public class SellDronesEvent : JournalEvent<SellDronesEvent.SellDronesEventArgs>
    {
        public SellDronesEvent() : base("SellDrones") { }

        public class SellDronesEventArgs : JournalEventArgs
        {
            public string Type { get; set; }
            public int Count { get; set; }
            public int SellPrice { get; set; }
            public int TotalSale { get; set; }
        }
    }
}
