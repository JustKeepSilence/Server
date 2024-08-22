using Server.MiddleWares;

namespace Server;

public class Runner
{

    public async static Task Start(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCors();
        builder.Services.AddCors(option =>
        {

            option.AddPolicy(name: "cors", policy =>
            {

                policy.WithOrigins("http://192.168.31.97:9527").SetIsOriginAllowed((host) => true).AllowAnyMethod().AllowAnyHeader().Build();
            });

        });

        var app = builder.Build();

        app.UseRouting();
        app.UseCors("cors");
        app.UseCustomMiddlware();
        app.MapControllers();
        await app.RunAsync();

    }


}
