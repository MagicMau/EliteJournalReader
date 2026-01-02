namespace EliteJournalReader.Events
{
    //When Written: when signing up to a community goal
    //Parameters:
    //�	Name
    //�	System
    public class CommunityGoalJoinEvent : JournalEvent<CommunityGoalJoinEvent.CommunityGoalJoinEventArgs>
    {
        public CommunityGoalJoinEvent() : base("CommunityGoalJoin") { }

        public class CommunityGoalJoinEventArgs : JournalEventArgs
        {
            public long CGID { get; set; }
            public string Name { get; set; }
            public string System { get; set; }
        }
    }
}
