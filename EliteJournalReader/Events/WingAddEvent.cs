using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: another player has joined the wing
    //Parameters:
    //•	Name
    public class WingAddEvent : JournalEvent<WingAddEvent.WingAddEventArgs>
    {
        public WingAddEvent() : base("WingAdd") { }

        public class WingAddEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");
            }

            public string Name { get; set; }
        }
    }
}
