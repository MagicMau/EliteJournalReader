namespace EliteJournalReader.Events
{
    public class IsLiveEvent : JournalEvent<IsLiveEvent.IsLiveEventArgs>
    {
        public IsLiveEvent() : base("MagicMau.IsLiveEvent") { }

        public class IsLiveEventArgs : JournalEventArgs
        {
        }
    }
}
