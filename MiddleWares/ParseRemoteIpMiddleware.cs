using System.Net;
using Server.Commons;

namespace Server.MiddleWares;

public class ParseRemoteIpMiddleware(RequestDelegate next)
{
    
    private static readonly int CallIndex = 1;
    
    private RequestDelegate Next { get; set; } = next;

    public async Task InvokeAsync(HttpContext context)
    {

        var headers = context.Request.Headers;  // 获取请求头部
        if (headers.TryGetValue("X-Forwarded-For", out var value))
        {
            // 设置请求头部
            context.Connection.RemoteIpAddress = IPAddress.Parse(value.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0]);

        }

        await Next(context);  // 传递到下一个管道

    }

}