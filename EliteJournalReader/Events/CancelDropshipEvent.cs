namespace EliteJournalReader.Events
{
    public class CancelDropshipEvent : JournalEvent<CancelDropshipEvent.CancelDropshipEventArgs>
    {
        public CancelDropshipEvent() : base("CancelDropship") { }

        public class CancelDropshipEventArgs : JournalEventArgs
        {
            public int Refund { get; set; }
        }
    }
}