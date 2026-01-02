namespace EliteJournalReader.Events
{
    //When Written: when selling a module in outfitting
    //Parameters:
    //�	Slot
    //�	SellItem
    //�	SellPrice
    //�	Ship
    public class ModuleSellRemoteEvent : JournalEvent<ModuleSellRemoteEvent.ModuleSellRemoteEventArgs>
    {
        public ModuleSellRemoteEvent() : base("ModuleSellRemote") { }

        public class ModuleSellRemoteEventArgs : JournalEventArgs
        {
            public string StorageSlot { get; set; }
            public string SellItem { get; set; }
            public string SellItem_Localised { get; set; }
            public int SellPrice { get; set; }
            public string Ship { get; set; }
            public long ShipID { get; set; }
            public string ServerId { get; set; }
        }
    }
}
