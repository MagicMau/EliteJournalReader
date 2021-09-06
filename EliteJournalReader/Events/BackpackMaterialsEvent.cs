using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class BackpackMaterialsEvent : JournalEvent<BackpackMaterialsEvent.BackpackMaterialsEventArgs>
    {
        public BackpackMaterialsEvent() : base("BackpackMaterials") { }

        public class BackpackMaterialsEventArgs : JournalEventArgs
        {
            public List<Material> Items { get; set; }

            public List<Material> Components { get; set; }

            public List<Material> Consumables { get; set; }

            public List<Material> Data { get; set; }
        }
    }
}