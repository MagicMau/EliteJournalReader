namespace EliteJournalReader.Events
{
    public class CarrierNameChangedEvent : JournalEvent<CarrierNameChangedEvent.CarrierNameChangedEventArgs>
    {
        public CarrierNameChangedEvent() : base("CarrierNameChanged") { }

        public class CarrierNameChangedEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public string Callsign { get; set; }
            public string Name { get; set; }
        }
    }
}