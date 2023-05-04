using Microsoft.AspNetCore.Mvc;

namespace ODataDBTester.Helpers;

public static class CreatedResultExtensions
{
    public static Dictionary<string, object> GetDapperRowAsDictionary(this CreatedResult createdResult)
    {
        var dapperRow = createdResult.Value?.GetType().GetProperty("DapperRow")?.GetValue(createdResult.Value);
        if (dapperRow == null)
        {
            return null;
        }

        return ((IEnumerable<KeyValuePair<string, object>>)dapperRow).ToDictionary(x => x.Key, x => x.Value);
    }
}