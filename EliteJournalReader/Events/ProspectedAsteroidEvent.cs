using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class ProspectedAsteroidEvent : JournalEvent<ProspectedAsteroidEvent.ProspectedAsteroidEventArgs>
    {
        public ProspectedAsteroidEvent() : base("ProspectedAsteroid") { }

        public class ProspectedAsteroidEventArgs : JournalEventArgs
        {
            public IEnumerable<ProspectedMaterial> Materials { get; set; }
            public string Content { get; set; }
            public string Content_Localised { get; set; }
            public string MotherlodeMaterial { get; set; }
            public string MotherlodeMaterial_Localised { get; set; }
            public double Remaining { get; set; }
        }
    }
}
