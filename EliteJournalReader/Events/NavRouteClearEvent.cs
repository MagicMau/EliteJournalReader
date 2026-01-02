namespace EliteJournalReader.Events
{
    public class NavRouteClearEvent : JournalEvent<NavRouteClearEvent.NavRouteClearEventArgs>
    {
        public NavRouteClearEvent() : base("NavRouteClear") { }

        public class NavRouteClearEventArgs : JournalEventArgs
        {
        }
    }
}