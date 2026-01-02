namespace EliteJournalReader.Events
{
    public class SquadronDemotionEvent : JournalEvent<SquadronDemotionEvent.SquadronDemotionEventArgs>
    {
        public SquadronDemotionEvent() : base("SquadronDemotion") { }

        public class SquadronDemotionEventArgs : JournalEventArgs
        {
            public long SquadronID { get; set; }
            public string SquadronName { get; set; }
            public int OldRank { get; set; }
            public int NewRank { get; set; }
        }
    }
}
