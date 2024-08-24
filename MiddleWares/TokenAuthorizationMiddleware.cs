using Microsoft.AspNetCore.Http.Extensions;
using Server.Connectors;
using Server.Models;
using Newtonsoft.Json;

namespace Server.MiddleWares;

public class TokenAuthorizationMiddleware(RequestDelegate next)
{

    private RequestDelegate Next { get; set; } = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {

        // check white list
        List<string> whiteListUrls = JsonConvert.DeserializeObject<List<string>>(configuration["whiteList"]);
        var r = from url in whiteListUrls where context.Request.GetEncodedUrl().Contains(url) select url;
        if (r.Any())
            await Next(context);
        else
        {

            try
            {


                using MysqlConnector connectors = new(configuration, out string msg);
                if (!string.IsNullOrEmpty(msg))
                {

                    Response<string> response = new()
                    {
                        Code = 500,
                        Message = msg
                    };
                    MemoryStream stream = new();
                    StreamWriter writer = new(stream);
                    await writer.WriteAsync(JsonConvert.SerializeObject(response));
                    await writer.FlushAsync();
                    context.Response.StatusCode = 500;
                    context.Response.Body = stream;
                    return;


                }

                string token = context.Request.Headers["Authorization"];
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    return;
                }

                var result = await connectors.Query(["@Token"], "select 1 as isExist from userCfg where token=@Token", new List<string>() { token });
                if (!string.IsNullOrEmpty(result.Message))
                {

                    Response<string> response = new()
                    {
                        Code = 500,
                        Message = result.Message
                    };
                    MemoryStream stream = new();
                    StreamWriter writer = new(stream);
                    await writer.WriteAsync(JsonConvert.SerializeObject(response));
                    await writer.FlushAsync();
                    context.Response.StatusCode = 500;
                    context.Response.Body = stream;
                    return;

                }

                if (result.Data.Rows.Count == 0)
                {

                    context.Response.StatusCode = 401;
                    return;

                }

                await Next(context);

            }
            catch (Exception ex)
            {

                Response<string> response = new()
                {
                    Code = 500,
                    Message = ex.Message
                };
                MemoryStream stream = new();
                StreamWriter writer = new(stream);
                await writer.WriteAsync(JsonConvert.SerializeObject(response));
                await writer.FlushAsync();
                context.Response.StatusCode = 500;
                context.Response.Body = stream;
                return;


            }
        }



    }

}
