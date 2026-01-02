namespace EliteJournalReader.Events
{
    //When written: when docking an SRV with the ship
    //Parameters: none
    public class DockSRVEvent : JournalEvent<DockSRVEvent.DockSRVEventArgs>
    {
        public DockSRVEvent() : base("DockSRV") { }

        public class DockSRVEventArgs : JournalEventArgs
        {
            public string SRVType { get; set; }
            public string SRVType_Localised { get; set; }
            public long ID { get; set; }
        }
    }
}
