namespace EliteJournalReader.Events
{
    //When written: when a docking request has timed out
    //Parameters:
    //�	StationName: name of station
    public class DockingTimeoutEvent : JournalEvent<DockingTimeoutEvent.DockingTimeoutEventArgs>
    {
        public DockingTimeoutEvent() : base("DockingTimeout") { }

        public class DockingTimeoutEventArgs : JournalEventArgs
        {
            public string StationName { get; set; }
            public string StationType { get; set; }
            public long MarketID { get; set; }
        }
    }
}
