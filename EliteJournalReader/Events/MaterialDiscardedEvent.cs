namespace EliteJournalReader.Events
{
    public class MaterialDiscardedEvent : JournalEvent<MaterialDiscardedEvent.MaterialDiscardedEventArgs>
    {
        public MaterialDiscardedEvent() : base("MaterialDiscarded") { }

        public class MaterialDiscardedEventArgs : JournalEventArgs
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }
    }
}
