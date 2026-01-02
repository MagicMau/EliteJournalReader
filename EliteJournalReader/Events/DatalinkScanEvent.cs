namespace EliteJournalReader.Events
{
    //When written: when scanning a data link
    //Parameters:
    //�	Message: message from data link
    public class DatalinkScanEvent : JournalEvent<DatalinkScanEvent.DatalinkScanEventArgs>
    {
        public DatalinkScanEvent() : base("DatalinkScan") { }

        public class DatalinkScanEventArgs : JournalEventArgs
        {
            public string Message { get; set; }
        }
    }
}
