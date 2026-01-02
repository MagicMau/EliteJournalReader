namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //�	Name: commander name
    public class CarrierShipPackEvent : JournalEvent<CarrierShipPackEvent.CarrierShipPackEventArgs>
    {
        public CarrierShipPackEvent() : base("CarrierShipPack") { }

        public class CarrierShipPackEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public string Operation { get; set; }
            public string PackTheme { get; set; }
            public int PackTier { get; set; }
            public long Cost { get; set; }
            public long Refund { get; set; }
        }
    }
}
