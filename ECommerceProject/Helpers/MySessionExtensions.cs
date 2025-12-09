using Microsoft.AspNetCore.Http;
using Newtonsoft.Json; // Cần cài NuGet Newtonsoft.Json

namespace ECommerceProject.Helpers
{
    public static class MySessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}