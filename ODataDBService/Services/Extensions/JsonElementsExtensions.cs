using System.Text.Json;

namespace ODataDBService.Services.Extensions
{
    public static class JsonElementExtensions
    {
        public static object? ToObject(this JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
                JsonValueKind.Number when element.TryGetDouble(out var doubleValue) => doubleValue,
                JsonValueKind.Number when element.TryGetDecimal(out var decimalValue) => decimalValue,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.GetRawText(),
                JsonValueKind.Object => element.GetRawText(),
                _ => throw new ArgumentOutOfRangeException($"Unknown JsonElement ValueKind: {element.ValueKind}")
            };
        }
    }
}
