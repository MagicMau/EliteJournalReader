namespace EliteJournalReader.Events
{
    //When written: entering supercruise from normal space
    //Parameters:
    //�	Starsystem
    public class SupercruiseEntryEvent : JournalEvent<SupercruiseEntryEvent.SupercruiseEntryEventArgs>
    {
        public SupercruiseEntryEvent() : base("SupercruiseEntry") { }

        public class SupercruiseEntryEventArgs : JournalEventArgs
        {
            public long SystemAddress { get; set; }
            public string StarSystem { get; set; }
            public bool Taxi { get; set; }
            public bool Multicrew { get; set; }
        }
    }
}
