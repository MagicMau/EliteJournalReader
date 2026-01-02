namespace EliteJournalReader.Events
{
    //When written: When selling a stored ship to raise funds when on insurance/rebuy screen
    //Parameters:
    //�	ShipType
    //�	System
    //�	SellShipId
    //�	ShipPrice
    public class SellShipOnRebuyEvent : JournalEvent<SellShipOnRebuyEvent.SellShipOnRebuyEventArgs>
    {
        public SellShipOnRebuyEvent() : base("SellShipOnRebuy") { }

        public class SellShipOnRebuyEventArgs : JournalEventArgs
        {
            public string ShipType { get; set; }
            public string ShipType_Localised { get; set; }
            public long SellShipID { get; set; }
            public int ShipPrice { get; set; }
            public string System { get; set; }
        }
    }
}
