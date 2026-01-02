namespace EliteJournalReader.Events
{
    //When written: when the player's SRV is destroyed
    //Parameters: none
    public class SRVDestroyedEvent : JournalEvent<SRVDestroyedEvent.SRVDestroyedEventArgs>
    {
        public SRVDestroyedEvent() : base("SRVDestroyed") { }

        public class SRVDestroyedEventArgs : JournalEventArgs
        {
            public long ID { get; set; }
        }
    }
}
