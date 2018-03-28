# EliteJournalReader

The game Elite Dangerous generates a journal file while playing.
Tools can use this data for their own purposes. 

This library aims to provide an easy way to consume these journal events
in a .NET environment.

# Usage

Instantiate a JournalWatcher object. Call `StartWatching()` to start
monitoring the Elite Dangerous Save Games folder (where the journal files are created)
and register for events by calling:

	JournalWatcher.GetEvent<TEventType>().Fired += MyEventHandler;

All events are automatically registered by a static method, you can find them
in the namespace EliteJournalReader.Events.

When you're done, call `StopWatching()` to stop monitoring file changes.

# More information
The full documentation on all generated events can be found at:
http://hosting.zaonce.net/community/journal/v16/Journal_Manual_v18.pdf
(or the same as a 
[Word document](http://hosting.zaonce.net/community/journal/v18/Journal_Manual_v16.doc))

# Disclaimer
This site was created using assets and imagery from Elite: Dangerous, 
with the permission of Frontier Developments plc for non-commercial purposes. 
It is not endorsed by nor reflects the views or opinions of Frontier Developments 
and no employee of Frontier Developments was involved in the making of it.
