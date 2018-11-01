using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //    When written: after using the “Surface Area Analysis” scanner
    //    Parameters:
    //•	Bodyname
    //•	BodyID
    //•	Discoverers: (array of names)
    //•	Mappers: (array of names)
    //•	ProbesUsed: (int)
    //•	EfficiencyTarget: (int)

    public class SAAScanCompleteEvent : JournalEvent<SAAScanCompleteEvent.SAAScanCompleteEventArgs>
    {
        public SAAScanCompleteEvent() : base("SAAScanComplete") { }

        public class SAAScanCompleteEventArgs : JournalEventArgs
        {
            public string BodyName { get; set; }
            public int BodyID { get; set; }
            public List<string> Discoverers { get; set; }
            public List<string> Mappers { get; set; }
            public int ProbesUsed { get; set; }
            public int EfficiencyTarget { get; set; }
        }
    }
}
