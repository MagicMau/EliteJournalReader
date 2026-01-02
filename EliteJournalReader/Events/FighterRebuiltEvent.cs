namespace EliteJournalReader.Events
{
    //When written: when a ship's fighter is rebuilt in the hangar
    //Parameters: none
    public class FighterRebuiltEvent : JournalEvent<FighterRebuiltEvent.FighterRebuiltEventArgs>
    {
        public FighterRebuiltEvent() : base("FighterRebuilt") { }

        public class FighterRebuiltEventArgs : JournalEventArgs
        {
            public string Loadout { get; set; }
            public long ID { get; set; }
        }
    }
}
