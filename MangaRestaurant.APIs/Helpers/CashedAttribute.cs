using MangaRestaurant.Core.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace MangaRestaurant.APIs.Helpers
{
    public class CashedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _expireTimeInSecond;

        public CashedAttribute(int ExpireTimeInSecond)
        {
            _expireTimeInSecond = ExpireTimeInSecond;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var casheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            var cachedResponse = await cacheService.GetCashedResponse(casheKey);
            if (!string.IsNullOrWhiteSpace(cachedResponse))
            {
                var contentResult = new ContentResult()
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }
            var excutedEndPointContext = await next.Invoke();
            if (excutedEndPointContext.Result is OkObjectResult result)
            {
                await cacheService.CasheResponseAsync(casheKey, result.Value, TimeSpan.FromSeconds(_expireTimeInSecond));
            }
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            // Sort = Name
            // Page Index = 1
            // Page Size = 5
            keyBuilder.Append(request.Path);// api/Products  
            foreach (var (key, value) in request.Query.OrderBy(X=>X.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
