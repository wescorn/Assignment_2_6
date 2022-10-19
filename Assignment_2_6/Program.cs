using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment_2_6
{
    class Program
    {
        const int N = 1_000_000_000;
        static int seed = 1234;
        static int index = -1;
        static readonly object theLock = new object();

        static void Main(string[] args)
        {
            int[] b = new int[N];

            Console.WriteLine($"Initializing array of size {N:n0}");
            ShowTimeInSeconds(() => InitializeArray(b));

            Console.Write("\nEnter number to search for: ");
            int number = int.Parse(Console.ReadLine());

            Console.WriteLine($"\nSequential Search for first occurence of {number}");
            ShowTimeInSeconds(() => FindindexOfFirstOccurence_Sequential(b, number));

            Console.WriteLine($"\nParallel Search for first occurence of {number}");
            ShowTimeInSeconds(() => FindindexOfFirstOccurence_Parallel(b, number));

            Console.WriteLine("\nIs first occurrence? " + IsFirstOccurrence(b, index, number));
        }

        private static void FindindexOfFirstOccurence_Parallel(int[] b, int number)
        {
            Parallel.ForEach(Partitioner.Create(0, b.Length), (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    if (number == b[i] && i < index)
                    {
                        lock (theLock)
                        {
                            index = i;
                            loopState.Break();
                        }
                    }
                }
            });
        }

        private static void FindindexOfFirstOccurence_Sequential(int[] b, int number)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (number == b[i])
                {
                    index = i;
                    break;
                }
            }
        }

        private static void ShowTimeInSeconds(Action p)
        {
            Stopwatch sw = Stopwatch.StartNew();
            p.Invoke();
            sw.Stop();
            Console.WriteLine("Time = {0:f5} seconds", sw.ElapsedMilliseconds / 1000d);
        }

        private static void InitializeArray(int[] b)
        {
            Parallel.ForEach(
                Partitioner.Create(0, b.Length),
                new ParallelOptions(),
                () => { return new Random(seed); },
                (range, loopState, rnd) =>
             {
                 for (int i = range.Item1; i < range.Item2; i++)
                 {
                     b[i] = rnd.Next(1, 100_000);
                 }
                 return rnd;
             },
                _ => { }
                );
        }

        private static bool IsFirstOccurrence(int[] b, int index, int number)
        {
            return (index == Array.IndexOf(b, number));
        }
    }
}
