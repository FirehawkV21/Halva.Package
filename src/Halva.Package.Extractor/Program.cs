using System;

namespace Halva.Package.Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("= Halva Bootstrapper");
            Console.WriteLine("= Version D1.00 ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("= Developed by Studio ACE");
            Console.WriteLine("= Licesned under the MIT license.");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }
    }
}
