namespace EliteJournalReader.Events
{
    //When written: when a ship-launched fighter is destroyed
    //Parameters: none
    public class FighterDestroyedEvent : JournalEvent<FighterDestroyedEvent.FighterDestroyedEventArgs>
    {
        public FighterDestroyedEvent() : base("FighterDestroyed") { }

        public class FighterDestroyedEventArgs : JournalEventArgs
        {
            public long ID { get; set; }
        }
    }
}
