namespace EliteJournalReader.Events
{
    public class UpgradeSuitEvent : JournalEvent<UpgradeSuitEvent.UpgradeSuitEventArgs>
    {
        public UpgradeSuitEvent() : base("UpgradeSuit") { }

        public class UpgradeSuitEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public string Name_Localised { get; set; }
            public long SuitID { get; set; }
            public int Class { get; set; }
            public int Cost { get; set; }
        }
    }
}