using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    /// <summary>
    /// When plotting a multi-star route, the file “Route.json” is written in the same directory as the journal, with a list of stars along that route
    /// Example:
    /// { "timestamp":"2020-04-27T08:02:52Z", "event":"Route", "Route":[ 
    /// { "StarSystem":1733120004818, "StarPos":[-19.75000,41.78125,-3.18750], "StarClass":"K" }, 
    /// { "StarSystem":5068732704169, "StarPos":[-15.25000,39.53125,-2.25000], "StarClass":"M" }
    ///  ] }
    /// Note this may be changed for final 3.7 release to use tag name “SystemAddress” instead of “StarSystem” for better consistency
    /// </summary>
    public class RouteEvent : JournalEvent<RouteEvent.RouteEventArgs>
    {
        public RouteEvent() : base("Route") { }

        public class RouteEventArgs : JournalEventArgs
        {
            public List<RouteElement> Route { get; set; }
        }
    }
}