namespace EliteJournalReader.Events
{
    public class DisbandedSquadronEvent : JournalEvent<DisbandedSquadronEvent.DisbandedSquadronEventArgs>
    {
        public DisbandedSquadronEvent() : base("DisbandedSquadron") { }

        public class DisbandedSquadronEventArgs : JournalEventArgs
        {
            public string SquadronName { get; set; }
        }
    }
}
