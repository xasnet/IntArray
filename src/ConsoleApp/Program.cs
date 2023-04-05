// See https://aka.ms/new-console-template for more information
using IntArray.Configuration;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", false);

        IConfigurationRoot configuration = builder.Build();
        var intArrayConfig = configuration.Get<IntArrayConfig>()!;

        int[] intArray = IntArrayGenerator(intArrayConfig.ElementsCount);


        Console.WriteLine($"Массив целых чисел. Количество элементов = {intArrayConfig.ElementsCount}");

        if (intArrayConfig.ElementsCount <= 100)
        {
            for (int i = 0; i < intArrayConfig.ElementsCount; i++)
            {
                Console.WriteLine("Element[{0}] = {1}", i, intArray[i]);
            }
        }

        Console.WriteLine();


        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        int sum = IntArraySum(intArray);
        stopwatch.Stop();
        Console.WriteLine($"Последовательная обработка массива, сек: {stopwatch.Elapsed.TotalSeconds}");
        Console.WriteLine("IntArraySum = {0}", sum);
        Console.WriteLine();

        stopwatch.Restart();
        int batchsize = (int)Math.Ceiling((decimal)intArrayConfig.ElementsCount / intArrayConfig.ThreadsCount);
        sum = IntArraySumThread(intArray, intArrayConfig.ThreadsCount, batchsize);
        stopwatch.Stop();
        Console.WriteLine($"Обработка массива потоками, сек: {stopwatch.Elapsed.TotalSeconds}");
        Console.WriteLine("IntArraySumThread = {0}", sum);
        Console.WriteLine();

        stopwatch.Restart();
        sum = IntArraySumParallel(intArray);
        stopwatch.Stop();
        Console.WriteLine($"Обработка массива Parallel LINQ, сек: {stopwatch.Elapsed.TotalSeconds}");
        Console.WriteLine("IntArraySumParallel = {0}", sum);

        Console.ReadKey();

    }

    private static int IntArraySumParallel(int[] intArray)
    {
        return intArray.AsParallel().Aggregate((sum, value) => sum + value);
    }

    private static int IntArraySumThread(int[] intArray, int threadsCount, int batchsize)
    {
        CountdownEvent countDown = new CountdownEvent(1);

        int sum = 0;
        int[] threadSums = new int[threadsCount];

        for (int i = 0; i < threadsCount; i++)
        {
            int tmp = i;
            countDown.AddCount();

            var batchElements = intArray.Skip(i * batchsize).Take(batchsize).ToArray();

            new Thread(() =>
            {
                threadSums[tmp] = batchElements.Sum(x => x);
                countDown.Signal();
            }).Start();
        }

        countDown.Signal();
        countDown.Wait();

        sum = threadSums.Sum(x => x);

        return sum;
    }

    private static int IntArraySum(int[] intArray)
    {
        int sum = 0;

        for (int i = 0; i < intArray.Length; i++)
        {
            sum += intArray[i];
        }

        return sum;
    }

    private static int[] IntArrayGenerator(int countElements)
    {
        int[] intArray = new int[countElements];

        for (int i = 0; i < countElements; i++)
        {
            intArray[i] = Random.Shared.Next(0, 9);
        }

        return intArray;
    }
}