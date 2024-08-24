using StackExchange.Redis;
using Server.Models;

namespace Server.Connectors;


public class RedisConnector : IDisposable
{

    public RedisConnector(IConfiguration configuration, out string msg, int db = 1)
    {

        Configuration = configuration;
        GetConnection(out msg, db);

    }

    private readonly IConfiguration Configuration;


    private ConnectionMultiplexer? Mux;  // redis连接池

    private IDatabase? Connection;   // redis数据库连接


    /// <summary>
    /// 获取redis连接
    /// </summary>
    private void GetConnection(out string msg, int db)
    {

        msg = string.Empty;

        try
        {

            string? redisIp = Configuration["redis:ip"]?.ToString();
            if (redisIp is null)
            {

                msg = "redisIP为空";
                return;

            }
            var options = ConfigurationOptions.Parse(redisIp);

            options.User = Configuration["redis:user"];
            options.Password = Configuration["redis:passWord"];
            options.AllowAdmin = string.IsNullOrEmpty(Configuration["redis:allowAdmin"]) || Convert.ToBoolean(Configuration["redis:allowAdmin"]);
            Mux = ConnectionMultiplexer.Connect(options);
            Connection = Mux.GetDatabase(db);

        }
        catch (Exception ex)
        {
            msg = ex.Message;
        }


    }

    /// <summary>
    /// 从redis获取指定的keys
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task<Response<IDictionary<string, string>>> Get(List<string> keys)
    {

        Response<IDictionary<string, string>> response = new();

        try
        {

            var r = await Connection?.StringGetAsync([.. keys]);

            if (r == null)
            {

                response.Message = "获取redis数据失败";
                response.Code = 500;
                return response;

            }

            Dictionary<string, string> results = [];
            for (int i = 0; i < r.Length; i++)
            {
                results[keys[i]] = r[i].HasValue ? r[i].ToString() : null;
            }

            response.Data = results;


        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
            response.Code = 500;
        }

        return response;



    }

    /// <summary>
    /// insert key value to redis
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expired"></param>
    /// <returns></returns>
    public async Task<Response<bool>> Set(string key, string value, TimeSpan? expired = null)
    {

        Response<bool> response = new();

        try
        {

            var r = await Connection.StringSetAsync(key, value, expired);
            response.Message = r ? string.Empty : "redis数据插入失败";
            response.Code = r ? 200 : 500;

        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
            response.Code = 500;
        }

        return response;


    }

    public async Task<Response<bool>> Set(IDictionary<string, string> keyValues)
    {

        Response<bool> response = new();

        try
        {

            List<KeyValuePair<RedisKey, RedisValue>> ks = [];

            foreach (var kv in keyValues)
            {
                ks.Add(new(kv.Key, kv.Value));
            }

            var r = await Connection.StringSetAsync([.. ks]);
            response.Message = r ? string.Empty : "redis数据插入失败";
            response.Code = r ? 200 : 500;

        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
            response.Code = 500;
        }

        return response;

    }


    public void Dispose()
    {

        Mux?.Dispose();

        Connection = null;

        Mux = null;

        GC.SuppressFinalize(this);


    }




}