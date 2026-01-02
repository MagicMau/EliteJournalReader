namespace EliteJournalReader.Events
{
    //When written: liftoff from a landing pad in a station, outpost or settlement
    //Parameters:
    //�	StationName: name of station
    public class UndockedEvent : JournalEvent<UndockedEvent.UndockedEventArgs>
    {
        public UndockedEvent() : base("Undocked") { }

        public class UndockedEventArgs : JournalEventArgs
        {
            public string StationName { get; set; }
            public string StationType { get; set; }
            public long MarketID { get; set; }
            public bool Taxi { get; set; }
            public bool Multicrew { get; set; }
        }
    }
}
