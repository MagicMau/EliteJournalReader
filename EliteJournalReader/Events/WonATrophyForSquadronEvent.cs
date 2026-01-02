namespace EliteJournalReader.Events
{
    public class WonATrophyForSquadronEvent : JournalEvent<WonATrophyForSquadronEvent.WonATrophyForSquadronEventArgs>
    {
        public WonATrophyForSquadronEvent() : base("WonATrophyForSquadron") { }

        public class WonATrophyForSquadronEventArgs : JournalEventArgs
        {
            public string SquadronName { get; set; }
        }
    }
}
