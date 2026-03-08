# Overview

EliteJournalReader is a .NET library for consuming Elite Dangerous journal events. The game generates journal files (and a `Status.json` file) while playing; this library monitors those files in real time and surfaces each event as a typed .NET event that consuming applications can subscribe to.

The library is published as a NuGet package and is used by EliteG19s (in `../EliteG19s-stt`) as its sole source of journal data.

# Project Structure

| Project | Responsibility |
|---------|---------------|
| **EliteJournalReader** | Core library — `JournalWatcher`, `StatusWatcher`, all event types, base classes |
| **EliteJournalReader.Tests** | MSTest unit tests |
| **EliteJournalFeedTester** | Console demo application showing library usage |

# Building

```powershell
# Build the entire solution (Debug):
dotnet build EliteJournalReader.sln 2>&1 | Tee-Object -Variable buildOutput | Out-Host; $buildOutput

# Build with a specific configuration:
dotnet build EliteJournalReader.sln -c "Debug local" 2>&1 | Tee-Object -Variable buildOutput | Out-Host; $buildOutput

# Release build (generates NuGet package, runs T4 version auto-increment):
dotnet build EliteJournalReader.sln -c Release 2>&1 | Tee-Object -Variable buildOutput | Out-Host; $buildOutput
```

Available configurations: `Debug`, `Debug local`, `Release`.  
Available platforms: `AnyCPU`, `x64`.

> **Note:** T4 template processing (`VersionAutoIncrementer.tt`) only runs on Release builds to avoid unnecessary file changes during development.

# Testing

The testing framework uses MSTest. Use `FakeJournalWatcher` and `FakeStatusWatcher` (in the Tests project) to inject synthetic JSON without touching the file system.

```powershell
# Run all tests:
dotnet test EliteJournalReader.Tests

# Run specific tests:
dotnet test EliteJournalReader.Tests --filter "Test_DockedEvent"
```

When testing, only run the relevant test cases — never the full test suite unnecessarily.

# Warnings

We want the library to be without warnings. When warnings occur, try not to use suppression directives unless there is no other option.

# Reflection

Unlike the consuming EliteG19s application, EliteJournalReader **intentionally uses reflection** as a core design feature. The static constructor of `JournalWatcher` uses reflection to automatically discover and register all `JournalEvent` subclasses at startup. This is an established pattern in this codebase and should not be changed.

# Try-catch behavior

Only use try/catch where an exception is genuinely expected. The catch clause should log a human-readable message with `Trace.TraceWarning` or `Trace.TraceError`, and include technical details (e.g. stack trace) with `Trace.TraceInformation`.

# Debug statements

Wrap any debug-only diagnostic `Trace.TraceInformation` calls in `#if DEBUG / #endif` blocks.

# Key Files

| File | Purpose |
|------|---------|
| `EliteJournalReader/JournalWatcher.cs` | Core file watcher; auto-discovers all event types via reflection at startup |
| `EliteJournalReader/StatusWatcher.cs` | Monitors `Status*.json` for real-time player status |
| `EliteJournalReader/JournalEvent.cs` | Abstract base classes `JournalEvent` and `JournalEvent<TArgs>` |
| `EliteJournalReader/JournalEventArgs.cs` | Base event args; provides `PostProcess`, `Clone`, `Timestamp`, `OriginalEvent` |
| `EliteJournalReader/Events/` | One file per event type (200+ events) |
| `EliteJournalReader.Tests/FakeJournalWatcher.cs` | Test double that fires events from raw JSON strings |
| `EliteJournalReader.Tests/FakeStatusWatcher.cs` | Test double for status events |

# Adding a New Event

1. Create `EliteJournalReader/Events/MyNewEvent.cs` in the `EliteJournalReader.Events` namespace.
2. Declare the event class and its nested `EventArgs` class following the established pattern:

```csharp
namespace EliteJournalReader.Events
{
    public class MyNewEvent : JournalEvent<MyNewEvent.MyNewEventArgs>
    {
        public MyNewEvent() : base("MyNew") { }

        public class MyNewEventArgs : JournalEventArgs
        {
            public string SomeProperty { get; set; }
            public int? OptionalValue { get; set; }
        }
    }
}
```

3. The event is automatically discovered and registered by `JournalWatcher`'s static constructor — no further registration is needed.
4. Add a test in `EliteJournalReader.Tests/TestJournalEvents.cs` using `FakeJournalWatcher.FireFakeEventAndReturn(json)`.

### Key conventions

- Event class name: `{EventName}Event` (matches the journal `"event"` field value)
- Constructor argument: the journal event name string, e.g. `base("FSDJump")`
- Nested args class: `{EventName}EventArgs : JournalEventArgs`
- Optional properties use nullable types (`int?`, `bool?`, `string` already nullable)
- Localised variants: add a `{Property}_Localised` string property alongside the original
- Override `Clone()` when the args contain reference-type fields that need deep-copying
- Override `PostProcess(JObject evt, JournalWatcher watcher)` for custom deserialization logic

# Event Architecture

```
JournalWatcher (FileSystemWatcher)
  └── on file change → parses JSON lines
        └── looks up event name in journalEventsByName dict
              └── JournalEvent<TArgs>.FireEvent()
                    ├── Deserializes JSON → TArgs (Newtonsoft.Json)
                    ├── Sets Timestamp and OriginalEvent
                    ├── Calls TArgs.PostProcess()
                    └── Raises Fired event → subscribers
```

Consumers subscribe with:

```csharp
var watcher = new JournalWatcher(path);
watcher.GetEvent<FSDJumpEvent>().Fired += (sender, e) => { /* e is FSDJumpEventArgs */ };
await watcher.StartWatching();
```
