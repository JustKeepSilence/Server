
namespace Server;

public static class Program
{

    public static void Main(string[] args)
    {

        var Config = new ConfigurationBuilder().AddJsonFile("appsettings.json",
        true, true).Build();

        string user = Config["redis:user"];


        Runner.Start(args).Wait();
    }

}
