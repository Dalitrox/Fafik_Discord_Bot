using Newtonsoft.Json;

namespace Fafik.Models
{
    public static class Serialize
    {
        public static string ToJson(this Events self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
