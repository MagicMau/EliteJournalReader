namespace EliteJournalReader.Events
{
    //When written: leaving supercruise for normal space
    //Parameters:
    //�	Starsystem
    //�	Body
    public class SupercruiseExitEvent : JournalEvent<SupercruiseExitEvent.SupercruiseExitEventArgs>
    {
        public SupercruiseExitEvent() : base("SupercruiseExit") { }

        public class SupercruiseExitEventArgs : JournalEventArgs
        {
            public long SystemAddress { get; set; }
            public string StarSystem { get; set; }
            public string Body { get; set; }
            public int BodyID { get; set; }
            public string BodyType { get; set; }

            public bool Taxi { get; set; }

            public bool Multicrew { get; set; }
        }
    }
}
