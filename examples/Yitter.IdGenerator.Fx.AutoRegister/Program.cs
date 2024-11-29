using System;

namespace Yitter.IdGenerator.Fx.AutoRegister
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("New Id:" + IdGeneratorUtil.NextId());
            Console.ReadKey();
        }
    }
}