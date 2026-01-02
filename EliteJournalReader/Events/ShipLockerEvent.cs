namespace EliteJournalReader.Events
{
    public class ShipLockerEvent : JournalEvent<ShipLockerEvent.ShipLockerEventArgs>
    {
        public ShipLockerEvent() : base("ShipLocker") { }

        public class ShipLockerEventArgs : JournalEventArgs
        {
            public Item[] Items { get; set; }
            public ScanItemComponent[] Components { get; set; }
            public Consumable[] Consumables { get; set; }
            public Data[] Data { get; set; }
        }
    }
}