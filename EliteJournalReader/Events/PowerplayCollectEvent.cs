namespace EliteJournalReader.Events
{
    //When written: when collecting powerplay commodities for delivery
    //Parameters:
    //�	Power: name of power
    //�	Type: type of commodity
    //�	Count: number of units
    public class PowerplayCollectEvent : JournalEvent<PowerplayCollectEvent.PowerplayCollectEventArgs>
    {
        public PowerplayCollectEvent() : base("PowerplayCollect") { }

        public class PowerplayCollectEventArgs : JournalEventArgs
        {
            public string Power { get; set; }
            public string Type { get; set; }
            public int Count { get; set; }
        }
    }
}
