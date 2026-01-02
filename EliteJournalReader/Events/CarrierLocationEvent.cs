namespace EliteJournalReader.Events
{
    //{
    //  "timestamp": "2025-03-19T19:02:10Z",
    //  "event": "CarrierLocation",
    //  "CarrierID": 3705689344,
    //  "StarSystem": "HR 3635",
    //  "SystemAddress": 1694121347427,
    //  "BodyID": 1
    //}
    public class CarrierLocationEvent : JournalEvent<CarrierLocationEvent.CarrierLocationEventArgs>
    {
        public CarrierLocationEvent() : base("CarrierLocation") { }

        public class CarrierLocationEventArgs : JournalEventArgs
        {
            public long CarrierID { get; set; }
            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }
            public int BodyID { get; set; }
        }
    }
}
