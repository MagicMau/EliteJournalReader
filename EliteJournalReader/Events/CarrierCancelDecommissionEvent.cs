namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //�	Name: commander name
    public class CarrierCancelDecommissionEvent : JournalEvent<CarrierCancelDecommissionEvent.CarrierCancelDecommissionEventArgs>
    {
        public CarrierCancelDecommissionEvent() : base("CarrierCancelDecommission") { }

        public class CarrierCancelDecommissionEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
        }
    }
}
