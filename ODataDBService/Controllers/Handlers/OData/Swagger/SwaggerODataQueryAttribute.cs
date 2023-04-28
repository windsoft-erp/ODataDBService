namespace ODataDBService.Controllers.Handlers.OData.Swagger
{
    public class SwaggerODataQueryAttribute : Attribute
    {
        public Dictionary<string, string> QueryExamples { get; private set; }

        public SwaggerODataQueryAttribute(params string[] queryExamples)
        {
            QueryExamples=queryExamples
                .Select(example =>
                {
                    var keyValue = example.Split(new[] { '=' }, 2);
                    return new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
                })
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
