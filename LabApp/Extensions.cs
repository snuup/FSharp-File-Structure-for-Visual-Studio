namespace Snuup
{
    static class Extensions
    {
        public static bool IsSet(this string s) { return !string.IsNullOrWhiteSpace(s); }
    }
}