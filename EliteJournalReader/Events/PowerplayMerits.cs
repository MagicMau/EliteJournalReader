namespace EliteJournalReader.Events
{
    //When written: at startup, if player has pledged to a power
    //Parameters:
    //�	Power: name
    //�	Rank
    //�	Merits
    //�	Votes
    //�	TimePledged(time in seconds)

    public class PowerplayMeritsEvent : JournalEvent<PowerplayMeritsEvent.PowerplayMeritsEventArgs>
    {
        public PowerplayMeritsEvent() : base("PowerplayMerits") { }

        public class PowerplayMeritsEventArgs : JournalEventArgs
        {
            public string Power { get; set; }
            public int MeritsGained { get; set; }
            public long TotalMerits { get; set; }
        }
    }
}
