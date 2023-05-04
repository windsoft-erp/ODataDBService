using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

namespace ODataDBTester.Helpers
{
    public class CustomUrlHelperFactory : IUrlHelperFactory
    {
        private readonly IUrlHelper _urlHelper;

        public CustomUrlHelperFactory(IUrlHelper urlHelper)
        {
            _urlHelper=urlHelper;
        }

        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            return _urlHelper;
        }
    }
}

