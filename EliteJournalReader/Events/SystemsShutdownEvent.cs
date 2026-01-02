namespace EliteJournalReader.Events
{
    //When written: on a clean shutdown of the game
    //Parameters: none
    public class SystemsShutdownEvent : JournalEvent<SystemsShutdownEvent.SystemsShutdownEventArgs>
    {
        public SystemsShutdownEvent() : base("SystemsShutdown") { }

        public class SystemsShutdownEventArgs : JournalEventArgs
        {
        }
    }
}
