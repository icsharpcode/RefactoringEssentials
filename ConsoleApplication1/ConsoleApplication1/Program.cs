using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var totalSumZeroToTen = Enumerable.Range(0, 10).Sum();
            var TestOne = new int[] {0, 5, 1, 3, 2, 9, 7, 6, 4}.Sum();
            Console.WriteLine((Enumerable.Range(1, 3).Aggregate(1, (x, y) => x * y)));
            Console.ReadLine();

        }
    }
}
