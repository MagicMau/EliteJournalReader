namespace EliteJournalReader.Events
{
    //When written: on a clean shutdown of the game
    //Parameters: none
    public class ShutdownEvent : JournalEvent<ShutdownEvent.ShutdownEventArgs>
    {
        public ShutdownEvent() : base("Shutdown") { }

        public class ShutdownEventArgs : JournalEventArgs
        {
        }
    }
}
