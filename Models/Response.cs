

using Newtonsoft.Json;

namespace Server.Models;

public class Response<T>
{

    [JsonProperty("message")]
    /// <summary>
    /// message
    /// </summary>
    public string? Message { get; set; }

    [JsonProperty("data")]
    /// <summary>
    /// data field
    /// </summary>
    public T? Data { get; set; }

    [JsonProperty("code")]
    public int Code { get; set; } = 200;

}


