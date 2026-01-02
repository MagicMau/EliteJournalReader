namespace EliteJournalReader.Events
{
    //When written: Creating a new commander
    //Parameters:
    //�	Name: (new) commander name
    //�	Package: selected starter package
    public class NewCommanderEvent : JournalEvent<NewCommanderEvent.NewCommanderEventArgs>
    {
        public NewCommanderEvent() : base("NewCommander") { }

        public class NewCommanderEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public string Package { get; set; }
        }
    }
}
