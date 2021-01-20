namespace blockchain.Enums
{
    public enum RoleType
    {
        VALIDATOR,
        DONOR,
        ORGANIZER
    }

    public static class RoleTypeExtionsions
    {
        public static string ToStringValue(this RoleType status)
        {
            switch(status)
            {
                case RoleType.VALIDATOR: 
                    return "Validator";

                case RoleType.DONOR: 
                    return "Donor";

                case RoleType.ORGANIZER: 
                    return "Organizer";
            }

            return string.Empty;
        }
    }
}
