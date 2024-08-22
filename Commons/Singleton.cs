namespace Server.Commons;

/// <summary>
/// generic Singleton Class, get instance of T
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T>
{

    private Singleton()
    {


    }

    private static T instance = default;

    private static readonly object syncRoot = new();

    /// <summary>
    /// Get Instance of T using given Constructor Parameter
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static T GetInstance(object?[]? parameters)
    {

        if (instance == null)
        {

            lock (syncRoot)
            {

                instance ??= (T)Activator.CreateInstance(typeof(T), parameters);

            }


        }

        return instance;

    }

}