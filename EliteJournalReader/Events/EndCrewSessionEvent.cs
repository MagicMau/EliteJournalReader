namespace EliteJournalReader.Events
{
    //When written: When another player leaves your ship's crew
    //Parameters:
    //�	Crew: player's commander name
    public class EndCrewSessionEvent : JournalEvent<EndCrewSessionEvent.EndCrewSessionEventArgs>
    {
        public EndCrewSessionEvent() : base("EndCrewSession") { }

        public class EndCrewSessionEventArgs : JournalEventArgs
        {
            public bool OnCrime { get; set; }
        }
    }
}
