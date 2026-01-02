namespace EliteJournalReader.Events
{
    //When written: when the �self destruct� function is used
    //Parameters: none
    public class SelfDestructEvent : JournalEvent<SelfDestructEvent.SelfDestructEventArgs>
    {
        public SelfDestructEvent() : base("SelfDestruct") { }

        public class SelfDestructEventArgs : JournalEventArgs
        {
        }
    }
}
