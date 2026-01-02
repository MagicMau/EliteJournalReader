namespace EliteJournalReader.Events
{
    public class InvitedToSquadronEvent : JournalEvent<InvitedToSquadronEvent.InvitedToSquadronEventArgs>
    {
        public InvitedToSquadronEvent() : base("InvitedToSquadron") { }

        public class InvitedToSquadronEventArgs : JournalEventArgs
        {
            public string SquadronName { get; set; }
        }
    }
}
