using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperCard
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 10; i++)
            {
                StackReader reader = new StackReader("Background Art");
                Console.WriteLine(reader.ToString());
            }

            watch.Stop();
            Console.WriteLine("Took {0}ms", watch.ElapsedMilliseconds);

            Console.ReadKey();
        }
    }
}
