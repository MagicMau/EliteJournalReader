namespace EliteJournalReader.Events
{
    //When Written: when dismissing a member of crew
    //Parameters:
    //�	Name
    public class CrewFireEvent : JournalEvent<CrewFireEvent.CrewFireEventArgs>
    {
        public CrewFireEvent() : base("CrewFire") { }

        public class CrewFireEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public long CrewID { get; set; }
        }
    }
}
