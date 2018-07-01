using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using static System.Console;

public class HttpDriver
{

    public static async Task Go(string url, int iterations, bool progressivelyIncreaseRate, bool showResponse)
    {
        var client = new HttpClient();
        var driverTimer = new Stopwatch();
        var iterationTimer = new Stopwatch();
        var rateDelay = 400;
        var count = 0;
        var iterationDisplayCount = 10;
        var iterationDisplayCountMax = 1000000;
        var iterationsAtCurrentDisplayCount = 0;
        var iterationsAtCurrentDisplayCountReset = 100;
        var displayIteration = false;
        string result = string.Empty;
        var tasks = new List<Task<string>>();

        CancelKeyPress +=  (object sender, ConsoleCancelEventArgs args) =>
        {
            PrintExitMessage(driverTimer.Elapsed, count-1);
        };

        WriteLine($"Starting HTTP Driver for URL: {url}");

        driverTimer.Start();

        while (true)
        {
            if (iterations >0 && count >= iterations )
            {
                return;
            }

            if (progressivelyIncreaseRate ||
                (iterations >0 && iterations <= 20) ||
                count % iterationDisplayCount == 0)
            {
                displayIteration = true;
                iterationTimer.Restart();
            }
            else
            {
                displayIteration = false;
            }

            if (progressivelyIncreaseRate)
            {
                await Task.Delay(rateDelay);
                if (rateDelay < 1)
                {
                    progressivelyIncreaseRate = false;
                    displayIteration = false;
                }
                else if (count % 10 == 0)
                {
                    rateDelay = (int)(rateDelay * .5);
                }
            }

            var task = client.GetStringAsync(url);
            tasks.Add(task);

            if (displayIteration)
            {
                try 
                {
                    await Task.WhenAll(tasks);
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    WriteLine($"Request failed at url: {url}");
                    PrintExitMessage(driverTimer.Elapsed,count);
                    return;
                }
                result = await tasks[0];
                var length = result.Length * 8;
                WriteLine($"request # {count}; requests: {iterationDisplayCount}; time: {iterationTimer.Elapsed.Seconds}.{iterationTimer.Elapsed.Milliseconds} seconds; Bytes: {length};");
                if (showResponse)
                {
                    WriteLine($"{result}");
                }
                tasks.Clear();

                if (!progressivelyIncreaseRate && 
                    iterationsAtCurrentDisplayCount++ >=iterationsAtCurrentDisplayCountReset && 
                    iterationDisplayCount < iterationDisplayCountMax)
                {
                    iterationDisplayCount *= 2;
                    iterationsAtCurrentDisplayCount = 0;
                }
            }

            count++;
        }
    }

    private static void PrintExitMessage(TimeSpan elapsedTime, int requests)
    {
        WriteLine($"total requests: {requests}; time: {elapsedTime.Seconds}.{elapsedTime.Milliseconds} seconds");
    }
    
}