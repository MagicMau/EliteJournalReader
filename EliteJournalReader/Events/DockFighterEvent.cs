namespace EliteJournalReader.Events
{
    //When written: when docking a fighter back with the mothership
    //Parameters: none
    public class DockFighterEvent : JournalEvent<DockFighterEvent.DockFighterEventArgs>
    {
        public DockFighterEvent() : base("DockFighter") { }

        public class DockFighterEventArgs : JournalEventArgs
        {
            public long ID { get; set; }
        }
    }
}
