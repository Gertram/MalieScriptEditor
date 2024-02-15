using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace ConfigLib
{
    public class StringListConfigurationOptionType : IConfigurationOptionType<List<string>>
    {
        public List<string> FromString(string value)
        {
            return new JavaScriptSerializer().Deserialize<List<string>>(value);
        }

        public string ToString(List<string> value)
        {
            string json = new JavaScriptSerializer().Serialize(value);
            return json;
        }
    }
}
