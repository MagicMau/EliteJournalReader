namespace EliteJournalReader.Events
{
    public class CarrierJumpCancelledEvent : JournalEvent<CarrierJumpCancelledEvent.CarrierJumpCancelledEventArgs>
    {
        public CarrierJumpCancelledEvent() : base("CarrierJumpCancelled") { }

        public class CarrierJumpCancelledEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
        }
    }
}