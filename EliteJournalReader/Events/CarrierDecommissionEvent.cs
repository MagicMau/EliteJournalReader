namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //�	Name: commander name
    public class CarrierDecommissionEvent : JournalEvent<CarrierDecommissionEvent.CarrierDecommissionEventArgs>
    {
        public CarrierDecommissionEvent() : base("CarrierDecommission") { }

        public class CarrierDecommissionEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public long ScrapRefund { get; set; }
            public long ScrapTime { get; set; }
        }
    }
}
