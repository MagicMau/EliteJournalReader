namespace EliteJournalReader.Events
{
    //When written: When you join another player ship's crew
    //Parameters:
    //�	Captain: Helm player's commander name
    public class JoinACrewEvent : JournalEvent<JoinACrewEvent.JoinACrewEventArgs>
    {
        public JoinACrewEvent() : base("JoinACrew") { }

        public class JoinACrewEventArgs : JournalEventArgs
        {
            public string Captain { get; set; }
        }
    }
}
