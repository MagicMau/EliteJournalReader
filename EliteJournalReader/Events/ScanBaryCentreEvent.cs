namespace EliteJournalReader.Events
{
    public class ScanBaryCentreEvent : JournalEvent<ScanBaryCentreEvent.ScanBaryCentreEventArgs>
    {
        public ScanBaryCentreEvent() : base("ScanBaryCentre") { }

        public class ScanBaryCentreEventArgs : JournalEventArgs
        {
            public string StarSystem { get; set; }
            public long SystemAddress { get; set; }
            public int BodyID { get; set; }
            public double SemiMajorAxis { get; set; }
            public double Eccentricity { get; set; }
            public double OrbitalInclination { get; set; }
            public double Periapsis { get; set; }
            public double OrbitalPeriod { get; set; }
            public double AscendingNode { get; set; }
            public double MeanAnomaly { get; set; }
        }
    }
}
