using Newtonsoft.Json;

namespace EliteJournalReader.Events
{
    //When written: when receiving information about a change in a friend's status
    //Parameters:
    //�	Status: one of the following: Requested, Declined, Added, Lost, Offline, Online
    //�	Name: the friend's commander name
    public class FriendsEvent : JournalEvent<FriendsEvent.FriendsEventArgs>
    {
        public FriendsEvent() : base("Friends") { }

        public class FriendsEventArgs : JournalEventArgs
        {
            [JsonConverter(typeof(ExtendedStringEnumConverter<FriendStatus>))]
            public FriendStatus Status { get; set; }

            public string Name { get; set; }
        }
    }
}
