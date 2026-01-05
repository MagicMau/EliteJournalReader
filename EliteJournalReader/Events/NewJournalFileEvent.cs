namespace EliteJournalReader.Events
{
    public class NewJournalFileEvent : JournalEvent<NewJournalFileEvent.NewJournalFileEventArgs>
    {
        public NewJournalFileEvent() : base("MagicMau.NewJournalFileEvent") { }

        public class NewJournalFileEventArgs : JournalEventArgs
        {
            public string Filename { get; set; }
        }
    }
}
