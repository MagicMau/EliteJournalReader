namespace EliteJournalReader.Events
{
    public class AppliedToSquadronEvent : JournalEvent<AppliedToSquadronEvent.AppliedToSquadronEventArgs>
    {
        public AppliedToSquadronEvent() : base("AppliedToSquadron") { }

        public class AppliedToSquadronEventArgs : JournalEventArgs
        {
            public string SquadronName { get; set; }
        }
    }
}
