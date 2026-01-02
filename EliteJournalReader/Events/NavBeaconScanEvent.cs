namespace EliteJournalReader.Events
{
    //When written: when scanning  a navigation beacon, before the scan data for all the bodies in the system is written into the journal
    //Parameters:
    //�	NumBodies
    public class NavBeaconScanEvent : JournalEvent<NavBeaconScanEvent.NavBeaconScanEventArgs>
    {
        public NavBeaconScanEvent() : base("NavBeaconScan") { }

        public class NavBeaconScanEventArgs : JournalEventArgs
        {
            public int NumBodies { get; set; }
            public long SystemAddress { get; set; }
        }
    }
}
