namespace EliteJournalReader.Events
{
    public class UseConsumableEvent : JournalEvent<UseConsumableEvent.UseConsumableEventArgs>
    {
        public UseConsumableEvent() : base("UseConsumable") { }

        public class UseConsumableEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public string Name_Localised { get; set; }
            public string Type { get; set; }
            public string Type_Localised { get; set; }
        }
    }
}