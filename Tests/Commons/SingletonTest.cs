using Server.Commons;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Server.Tests.Commons;


public class SingletonTest
{


    private class SingletonTestClass(string name)
    {

        public string? Name { get; set; } = name;
    }

    public static void Run()
    {

        ConcurrentBag<string> ps = [];

        // 创建2个线程
        List<Task> workers = [

            Task.Run(()=>{

                var instance = Singleton<SingletonTestClass>.GetInstance([
                    "name1"
                ]);

                unsafe{

                    var tr = __makeref(instance);  // 创建引用
                    IntPtr p = **(IntPtr**)(&tr);
                    ps.Add(p.ToString("X"));
                }


            }),

             Task.Run(()=>{

                var instance = Singleton<SingletonTestClass>.GetInstance([
                    "name2"
                ]);

                unsafe{
                    
                    var tr = __makeref(instance);
                    IntPtr p = **(IntPtr**)(&tr);
                    ps.Add(p.ToString("X"));
                }


            })




        ];

        Task.WhenAll(workers).Wait();

        var r = ps.ToList();
        Debug.Assert(r[0] == r[1]);

    }


}