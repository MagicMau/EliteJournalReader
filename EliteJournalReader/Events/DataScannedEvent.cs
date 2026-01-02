namespace EliteJournalReader.Events
{
    //When written: when scanning a data link
    //Parameters:
    //�	Message: message from data link
    public class DataScannedEvent : JournalEvent<DataScannedEvent.DataScannedEventArgs>
    {
        public DataScannedEvent() : base("DataScanned") { }

        public class DataScannedEventArgs : JournalEventArgs
        {
            public string Type { get; set; }
        }
    }
}
