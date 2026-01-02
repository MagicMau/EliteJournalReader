namespace EliteJournalReader.Events
{
    //Written when accessing the outfitting menu
    //The full parts pricelist is written to a separate file Outfitting.json
    //Parameters:
    //�	MarketID
    //�	StationName
    //�	StarSystem

    //The separate file also contains
    //�	Items: array of objects
    //o   id
    //o   Name
    //o   BuyPrice
    public class OutfittingEvent : JournalEvent<OutfittingEvent.OutfittingEventArgs>
    {
        public OutfittingEvent() : base("Outfitting") { }

        public class OutfittingEventArgs : JournalEventArgs
        {
            public string StationName { get; set; }
            public long MarketID { get; set; }
            public string StarSystem { get; set; }
        }
    }
}
