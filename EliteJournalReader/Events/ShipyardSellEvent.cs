namespace EliteJournalReader.Events
{
    //When Written: when selling a ship stored in the shipyard
    //Parameters:
    //�	ShipType: type of ship being sold
    //�	SellShipID
    //�	ShipPrice: sale price
    //�	System: (if ship is in another system) name of system
    public class ShipyardSellEvent : JournalEvent<ShipyardSellEvent.ShipyardSellEventArgs>
    {
        public ShipyardSellEvent() : base("ShipyardSell") { }

        public class ShipyardSellEventArgs : JournalEventArgs
        {
            public long MarketID { get; set; }
            public string ShipType { get; set; }
            public string ShipType_Localised { get; set; }
            public long SellShipID { get; set; }
            public int ShipPrice { get; set; }
            public string System { get; set; }
        }
    }
}
