using System;

namespace EliteJournalReader.Events
{
    //When written: when checking the status of a community goal
    //This event contains the current status of all community goals the player is currently subscribed to
    //Parameters:
    //�	CurrentGoals: an array with an entry for each CG, containing:
    //o CGID: a unique ID number for this CG
    //o   Title: the description of the CG
    //o   SystemName
    //o   MarketName
    //o   Expiry: time and date
    //o   IsComplete: Boolean
    //o   CurrentTotal
    //o   PlayerContribution
    //o   NumContributors
    //o   PlayerPercentileBand

    //If the community goal is constructed with a fixed-size top rank (ie max reward for top 10 players)
    //o TopRankSize: (integer)
    //o PlayerInTopRank: (Boolean)

    //If the community goal has reached the first success tier:
    //o TierReached
    //o Bonus
    public class CommunityGoalEvent : JournalEvent<CommunityGoalEvent.CommunityGoalEventArgs>
    {
        public CommunityGoalEvent() : base("CommunityGoal") { }

        public class CommunityGoalEventArgs : JournalEventArgs
        {
            public class CurrentGoal
            {
                public long CGID { get; set; }
                public string Title { get; set; }
                public string SystemName { get; set; }
                public string MarketName { get; set; }
                public DateTime Expiry { get; set; }
                public bool IsComplete { get; set; }
                public long CurrentTotal { get; set; }
                public long PlayerContribution { get; set; }
                public long NumContributors { get; set; }
                public int? PlayerPercentileBand { get; set; }
                public int? TopRankSize { get; set; }
                public bool? PlayerInTopRank { get; set; }
                public string TierReached { get; set; }
                public long? Bonus { get; set; }
                public Tier TopTier { get; set; }
            }

            public class Tier
            {
                public string Name { get; set; }
                public string Bonus { get; set; }
            }
            
            public CurrentGoal[] CurrentGoals { get; set; }
        }
    }
}
