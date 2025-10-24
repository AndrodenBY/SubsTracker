using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SubsTracker.BLL.Json;

public static class NewtonsoftJsonSettings
{
    public static readonly JsonSerializerSettings Default = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
}

