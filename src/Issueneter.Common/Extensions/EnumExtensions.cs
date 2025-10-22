namespace Issueneter.Common.Extensions;

public static class EnumExtensions
{
    extension(Enum e)
    {
        public static bool TryParseSafe<T>(string value, out T result) where T : struct, Enum {
            if (Enum.TryParse(value, out result) && Enum.IsDefined(result))
            {
                return true;
            }

            return false;
        }
    }
}