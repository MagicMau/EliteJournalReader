using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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