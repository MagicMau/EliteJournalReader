using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class ColonisationContributionEvent : JournalEvent<ColonisationContributionEvent.ColonisationContributionEventArgs>
    {
        public ColonisationContributionEvent() : base("ColonisationContribution") { }

        public class ColonisationContributionEventArgs : JournalEventArgs
        {
            public long MarketID { get; set; }
            public ColonizationContribution[] Contributions { get; set; }

        }
    }

    public class ColonizationContribution
    {
        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public int Amount { get; set; }
    }
}