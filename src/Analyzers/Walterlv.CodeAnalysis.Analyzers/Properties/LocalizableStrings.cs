using Microsoft.CodeAnalysis;

namespace Walterlv.CodeAnalysis.Properties
{
    internal static class LocalizableStrings
    {
        public static LocalizableString Get(string key) => new LocalizableResourceString(key, Resources.ResourceManager, typeof(Resources));
    }
}
