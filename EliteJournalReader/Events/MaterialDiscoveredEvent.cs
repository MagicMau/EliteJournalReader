namespace EliteJournalReader.Events
{
    public class MaterialDiscoveredEvent : JournalEvent<MaterialDiscoveredEvent.MaterialDiscoveredEventArgs>
    {
        public MaterialDiscoveredEvent() : base("MaterialDiscovered") { }

        public class MaterialDiscoveredEventArgs : JournalEventArgs
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public int DiscoveryNumber { get; set; }
        }
    }
}
