using System;
using System.IO;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        var path = Path.Combine("e:", "hungryHub", ".nuget", "socketioclient", "4.0.0", "lib", "net8.0", "SocketIOClient.dll");
        var asm = Assembly.LoadFrom(path);
        var ctxType = asm.GetType("SocketIOClient.IEventContext");
        Console.WriteLine(ctxType.FullName);
        foreach (var method in ctxType.GetMethods().OrderBy(m => m.Name))
        {
            Console.WriteLine(method.ToString());
        }
    }
}
