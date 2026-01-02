namespace EliteJournalReader.Events
{
    public class SharedBookmarkToSquadronEvent : JournalEvent<SharedBookmarkToSquadronEvent.SharedBookmarkToSquadronEventArgs>
    {
        public SharedBookmarkToSquadronEvent() : base("SharedBookmarkToSquadron") { }

        public class SharedBookmarkToSquadronEventArgs : JournalEventArgs
        {
            public string SquadronName { get; set; }
        }
    }
}
