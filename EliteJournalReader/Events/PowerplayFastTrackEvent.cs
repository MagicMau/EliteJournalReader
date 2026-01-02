namespace EliteJournalReader.Events
{
    //When written: when paying to fast-track allocation of commodities
    //Parameters:
    //�	Power
    //�	Cost
    public class PowerplayFastTrackEvent : JournalEvent<PowerplayFastTrackEvent.PowerplayFastTrackEventArgs>
    {
        public PowerplayFastTrackEvent() : base("PowerplayFastTrack") { }

        public class PowerplayFastTrackEventArgs : JournalEventArgs
        {
            public string Power { get; set; }
            public int Cost { get; set; }
        }
    }
}
