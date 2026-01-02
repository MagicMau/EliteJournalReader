namespace EliteJournalReader.Events
{
    public class DisembarkEvent : JournalEvent<DisembarkEvent.DisembarkEventArgs>
    {
        public DisembarkEvent() : base("Disembark") { }

        public class DisembarkEventArgs : JournalEventArgs
        {
            public bool SRV { get; set; }
            public bool Taxi { get; set; }
            public bool Multicrew { get; set; }
            public long ID { get; set; }
            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }
            public string Body { get; set; }
            public int BodyID { get; set; }
            public bool OnStation { get; set; }
            public bool OnPlanet { get; set; }
            public string StationName { get; set; }
            public string StationType { get; set; }
            public long MarketID { get; set; }
        }
    }
}