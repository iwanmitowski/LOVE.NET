using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoveNet.BuildingBlocks.EventBus;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
