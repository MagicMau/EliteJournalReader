using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EliteJournalReader
{
    public abstract class JournalEvent
    {
        private readonly string[] _eventNames;
        public string[] EventNames { get { return _eventNames; } }

        public string OriginalEvent { get; protected set; }

        protected JournalEvent(params string[] eventNames)
        {
            _eventNames = eventNames;
        }

        internal abstract void FireEvent(object sender, JObject evt);
    }

    public abstract class JournalEvent<TJournalEventArgs> : JournalEvent
        where TJournalEventArgs : JournalEventArgs, new()
    {
        public event EventHandler<TJournalEventArgs> Fired;

        protected JournalEvent(params string[] eventNames) : base(eventNames)
        {
        }

        public void AddHandler(EventHandler<TJournalEventArgs> eventHandler)
        {
            Fired += eventHandler;
        }

        public void RemoveHandler(EventHandler<TJournalEventArgs> eventHandler)
        {
            Fired -= eventHandler;
        }

        internal override void FireEvent(object sender, JObject evt)
        {
            var eventArgs = new TJournalEventArgs();
            eventArgs.Initialize(evt);
            Fired?.Invoke(sender, eventArgs);
        }
    }
}