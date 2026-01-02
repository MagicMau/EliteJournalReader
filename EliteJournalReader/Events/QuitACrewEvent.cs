namespace EliteJournalReader.Events
{
    //When written: When you leave another player ship's crew
    //Parameters:
    //�	Captain: Helm player's commander name
    public class QuitACrewEvent : JournalEvent<QuitACrewEvent.QuitACrewEventArgs>
    {
        public QuitACrewEvent() : base("QuitACrew") { }

        public class QuitACrewEventArgs : JournalEventArgs
        {
            public string Captain { get; set; }
        }
    }
}
