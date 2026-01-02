namespace EliteJournalReader.Events
{
    public class SquadronPromotionEvent : JournalEvent<SquadronPromotionEvent.SquadronPromotionEventArgs>
    {
        public SquadronPromotionEvent() : base("SquadronPromotion") { }

        public class SquadronPromotionEventArgs : JournalEventArgs
        {
            public long SquadronID { get; set; }
            public string SquadronName { get; set; }
            public int OldRank { get; set; }
            public int NewRank { get; set; }
        }
    }
}
