namespace PWARAbilityTool.Client
{
    public class TokenHolder
    {
        private static TokenHolder tokenHolder;
        public string token { get; set; }

        private TokenHolder()
        { }

        public static TokenHolder GetInstance()
        {
            if (tokenHolder == null)
            {
                tokenHolder = new TokenHolder();
            }

            return tokenHolder;
        }
    }
}