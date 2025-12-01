namespace Bannerlord.LordLife.DebugTools
{
    /// <summary>
    /// Manages debug mode settings for LordLife mod
    /// </summary>
    public static class DebugSettings
    {
        /// <summary>
        /// Enable or disable debug mode
        /// Set to true to enable debug features (e.g., K key to add 10000 gold)
        /// </summary>
        public static bool IsDebugEnabled { get; set; } = false;
    }
}
