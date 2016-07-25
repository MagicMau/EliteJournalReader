using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace EliteJournalReader
{
    public class JournalEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }

        public JournalEventArgs()
        {
        }

        public virtual void Initialize(JObject evt)
        {
            Timestamp = DateTime.Parse(evt.StringValue("timestamp"),
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
    }
}