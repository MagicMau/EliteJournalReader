using System.Collections.Generic;

namespace EliteJournalReader.Events
{
    public class ShipLockerMaterialsEvent : JournalEvent<ShipLockerMaterialsEvent.ShipLockerMaterialsEventArgs>
    {
        public ShipLockerMaterialsEvent() : base("ShipLockerMaterials") { }

        public class ShipLockerMaterialsEventArgs : JournalEventArgs
        {
            public List<Item> Items { get; set; }
            public List<Item> Components { get; set; }
            public List<Item> Consumables { get; set; }
            public List<Item> Data { get; set; }
        }
    }
}