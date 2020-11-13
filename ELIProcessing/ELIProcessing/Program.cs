using System;
using System.IO;

namespace ELIProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = ParallelProcessing.ParallelProcessingRun(args[0], args[1], 1000, out int imageWidth, out int imageHeight);
            Utilitis.WriteFile("result.ELI", imageWidth, imageHeight, result);
            Console.ReadKey();
        }
    }
}
