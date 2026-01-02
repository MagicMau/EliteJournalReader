namespace EliteJournalReader.Events
{
    //When written: deploying the SRV from a ship onto planet surface
    //Parameters:
    //�	Loadout
    public class LaunchSRVEvent : JournalEvent<LaunchSRVEvent.LaunchSRVEventArgs>
    {
        public LaunchSRVEvent() : base("LaunchSRV") { }

        public class LaunchSRVEventArgs : JournalEventArgs
        {
            public string SRVType { get; set; }
            public string SRVType_Localised { get; set; }
            public string Loadout { get; set; }
            public long ID { get; set; }
            public bool PlayerControlled { get; set; }
        }
    }
}
