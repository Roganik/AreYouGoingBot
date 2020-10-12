namespace AreYouGoingBot
{
    public static class RegexPatterns
    {
        public const string SetLimitPattern = @"\->\d+"; // e.g. ->5

        public const string AddRangeByPlusesPattern = @"\++"; // e.g. +++

        public const string AddRangeByNumberPattern =@"\+\d+"; // e.g. +3
    }
}