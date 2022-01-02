using Newtonsoft.Json;

namespace Aareon.Business
{
    public static class Extensions
    {
        public static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented, 
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
        }
    }
}