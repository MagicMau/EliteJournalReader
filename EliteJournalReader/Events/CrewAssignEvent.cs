namespace EliteJournalReader.Events
{
    //When Written: when changing the task assignment of a member of crew
    //Parameters:
    //�	Name
    //�	Role
    public class CrewAssignEvent : JournalEvent<CrewAssignEvent.CrewAssignEventArgs>
    {
        public CrewAssignEvent() : base("CrewAssign") { }

        public class CrewAssignEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public long CrewID { get; set; }
            public string Role { get; set; }
        }
    }
}
