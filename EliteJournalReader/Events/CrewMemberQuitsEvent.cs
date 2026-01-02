namespace EliteJournalReader.Events
{
    //When written: When another player leaves your ship's crew
    //Parameters:
    //�	Crew: player's commander name
    public class CrewMemberQuitsEvent : JournalEvent<CrewMemberQuitsEvent.CrewMemberQuitsEventArgs>
    {
        public CrewMemberQuitsEvent() : base("CrewMemberQuits") { }

        public class CrewMemberQuitsEventArgs : JournalEventArgs
        {
            public string Crew { get; set; }
            public long CrewID { get; set; }
        }
    }
}
