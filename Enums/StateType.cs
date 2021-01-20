namespace blockchain.Enums
{
    public enum StateType
    {
        FOLLLOWER,
        CANDIDATE,
        LEADER
    }

    public static class StateTypeExtionsions
    {
        public static string ToStringValue(this StateType status)
        {
            switch(status)
            {
                case StateType.FOLLLOWER: 
                    return "Follower";

                case StateType.CANDIDATE: 
                    return "Candidate";

                case StateType.LEADER: 
                    return "Leader";
            }

            return string.Empty;
        }
    }
}
