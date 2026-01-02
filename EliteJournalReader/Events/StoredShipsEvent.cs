namespace EliteJournalReader.Events
{
    //    When written: when visiting shipyard
    //
    //    Parameters:
    //�	MarketID
    //�	StationName
    //�	StarSystem
    //�	ShipsHere: (array of objects)
    //o ShipID
    //o ShipType
    //o Name(if named)
    //o Value
    //o Hot
    //�	ShipsRemote: (array of objects)
    //o ShipID
    //o ShipType
    //o Name(if named)
    //o Value
    //o Hot
    //
    //If the ship is in transit:
    //o InTransit: true
    //
    //If the ship is not in transit:
    //o StarSystem
    //o ShipMarketID
    //o TransferPrice
    //o TransferType

    public class StoredShipsEvent : JournalEvent<StoredShipsEvent.StoredShipsEventArgs>
    {
        public StoredShipsEvent() : base("StoredShips") { }

        public class StoredShipsEventArgs : JournalEventArgs
        {
            public class StoredShip
            {
                public long ShipID { get; set; }
                public string ShipType { get; set; }
                public string ShipType_Localised { get; set; }
                public string Name { get; set; }
                public int Value { get; set; }
                public bool Hot { get; set; }
                public bool InTransit { get; set; }
                public string StarSystem { get; set; }
                public long ShipMarketID { get; set; }
                public int TransferPrice { get; set; }
                public string TransferType { get; set; }
            }

            public long MarketID { get; set; }
            public string StationName { get; set; }
            public string StarSystem { get; set; }
            public StoredShip[] ShipsHere { get; set; }
            public StoredShip[] ShipsRemote { get; set; }
        }
    }
}
