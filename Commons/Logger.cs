using Microsoft.AspNetCore.Http.Extensions;

namespace Server.Commons;

public static class Logger
{

    private static readonly object Log4NetSyncObject = new();

    /// <summary>
    /// Write Log to file
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    /// <param name="time">Time to execute request</param>
    /// <param name="message">Error message</param>
    /// <param name="body"></param>
    /// <param name="level">Log Level</param>
    public static void WriteLog<T>(this ILogger<T> logger, HttpContext context,double time, string message, string? body, int level)
    {

        lock (Log4NetSyncObject)
        {
            try
            {



                string logMessage = string.Empty;
                if (context == null)
                {
                    logMessage = $"[url]:null,[remoteIp]:null,[method]:null,[Content-Type]: null,[body]:null, [time]:{time},[message]:{message} {body}";
                    logger.WriteLog(logMessage, level);
                    return;

                }
                else
                {

                    string url = context.Request.GetEncodedUrl();  // request url
                    if (url.Contains("getRdkxResult")) return;
                    string remoteIp = context.Connection.RemoteIpAddress.ToString();  // client ip
                    string method = context.Request.Method;  // request method
                    string bodyMessage = string.IsNullOrEmpty(body) ? "" : body;
                    string contentType = context.Request.Headers["Content-Type"].ToString() == null ? "" : context.Request.Headers["Content-Type"].ToString();
                    logMessage = $"[url]:{url},[remoteIp]:{remoteIp},[method]:{method},[Content-Type]: {contentType},[body]:{bodyMessage}, [time]:{time},[message]:{message}";
                }
                switch (level)
                {

                    case 0:
                        // info
                        logger.LogInformation(logMessage);
                        break;
                    case 1:
                        // info
                        logger.LogWarning(logMessage);
                        break;
                    default:
                        // info
                        logger.LogError(logMessage);
                        break;

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{DateTime.Now}:fail to write log to file!, error message is {ex.Message}");

            }
        }


    }

    private static void WriteLog<T>(this ILogger<T> logger, string message, int level)
    {

        var now = DateTime.Now;
        string name = now.ToString("yyyyMMdd");
        // 2024-06-26 17:09:34,300 [10] INFO  Server.Sever.Controllers.ReportController [(null)] - [url]:http://192.178.31.97:8088/report/getBoilerResult,[remoteIp]:::ffff:192.178.31.85,[method]:GET,[Content-Type]: ,[body]:, [time]:550.3248,[message]:
        var contents = now.ToString("yyyy-MM-dd HH:mm:ss");
        contents += level switch
        {
            0 => " INFO ",
            1 => " WARN ",
            _ => " ERROR ",
        };

        contents += logger.ToString();
        contents += $" {message}" + "\n";
        using FileStream fs = new($"/var/log/server/{name}-.log", FileMode.Append, FileAccess.Write);
        var d = System.Text.Encoding.UTF8.GetBytes(contents);
        fs.Position = fs.Length;
        fs.Write(d, 0, d.Length);
        fs.Flush();




    }

}