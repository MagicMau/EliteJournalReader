using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: Player has escaped interdiction
    //Parameters: None
    public class EscapeInterdictionEvent : JournalEvent<EscapeInterdictionEvent.EscapeInterdictionEventArgs>
    {
        public EscapeInterdictionEvent() : base("EscapeInterdiction") { }

        public class EscapeInterdictionEventArgs : JournalEventArgs
        {
        }
    }
}
