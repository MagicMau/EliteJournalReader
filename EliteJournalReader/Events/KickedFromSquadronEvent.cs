namespace EliteJournalReader.Events
{
    public class KickedFromSquadronEvent : JournalEvent<KickedFromSquadronEvent.KickedFromSquadronEventArgs>
    {
        public KickedFromSquadronEvent() : base("KickedFromSquadron") { }

        public class KickedFromSquadronEventArgs : JournalEventArgs
        {
            public string SquadronName { get; set; }
        }
    }
}
