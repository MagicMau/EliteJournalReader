namespace EliteJournalReader.Events
{
    public class JoinedSquadronEvent : JournalEvent<JoinedSquadronEvent.JoinedSquadronEventArgs>
    {
        public JoinedSquadronEvent() : base("JoinedSquadron") { }

        public class JoinedSquadronEventArgs : JournalEventArgs
        {
            public long SquadronID { get; set; }
            public string SquadronName { get; set; }
        }
    }
}
