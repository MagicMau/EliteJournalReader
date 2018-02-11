using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //•	Name: commander name
    public class MaterialsEvent : JournalEvent<MaterialsEvent.MaterialsEventArgs>
    {
        public MaterialsEvent() : base("Materials") { }

        public class MaterialsEventArgs : JournalEventArgs
        {
            public List<Material> Raw { get; set; }
            public List<Material> Manufactured { get; set; }
            public List<Material> Encoded { get; set; }
        }
    }
}
