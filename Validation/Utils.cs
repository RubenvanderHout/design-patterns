namespace Validation
{
    static class Helpers
    {
        public static T GetOrThrow<T>(Dictionary<string, T> dict, string key, string kind)
        {
            return dict.TryGetValue(key, out var value)
                ? value
                : throw new InvalidOperationException($"Developer error: {kind} '{key}' not found.");
        }
    }
}