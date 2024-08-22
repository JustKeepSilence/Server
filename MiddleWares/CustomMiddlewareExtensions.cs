using System.Reflection;

namespace Server.MiddleWares;

public static class CustomMiddlewareExtensions
{

    public static IApplicationBuilder UseCustomMiddlware(this IApplicationBuilder builder)
    {
        var ns = from x in Assembly.GetExecutingAssembly().GetTypes()
                 where x.IsClass && x.Namespace == "Server.MiddleWares" && x.FullName is not null
        && x.FullName != "Server.MiddleWares.CustomMiddlewareExtensions"
        && !x.FullName.Contains("InvokeAsync")
        && x.Attributes == (TypeAttributes.Public | TypeAttributes.BeforeFieldInit) select x;
        foreach (var n in ns)
        {
            var method = (from m in typeof(UseMiddlewareExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                          where m.Name == "UseMiddleware" &&
            m.IsGenericMethod
                          select m).ToList()[0].MakeGenericMethod([
                n
            ]);
            builder = (IApplicationBuilder)method.Invoke(null, [builder, Array.Empty<object>()]);

        }

        return builder;

    }


}