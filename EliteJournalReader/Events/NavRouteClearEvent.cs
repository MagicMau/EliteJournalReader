using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class NavRouteClearEvent : JournalEvent<NavRouteClearEvent.NavRouteClearEventArgs>
    {
        public NavRouteClearEvent() : base("NavRouteClear") { }

        public class NavRouteClearEventArgs : JournalEventArgs
        {
        }
    }
}