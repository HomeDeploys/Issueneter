using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Issueneter.Common.Extensions;

public static class JsonExtensions
{
    private static readonly JsonSerializerSettings SnakeCaseSettings = new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public static T? DeserializeSnakeCase<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json, SnakeCaseSettings);
    }
    
    public static string SerializeSnakeCase<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, SnakeCaseSettings);
    }
}