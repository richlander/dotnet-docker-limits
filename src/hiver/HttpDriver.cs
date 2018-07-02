using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using static System.Console;

public class HttpDriver
{

    public static async Task Go(string url, int requests, bool progressivelyIncreaseRate, bool showResponse)
    {
        var client = new HttpClient();
        var driverTimer = new Stopwatch();
        var iterationTimer = new Stopwatch();
        var rateDelay = 400;
        var requestCount = 0;
        var requestCountToLog = 1;
        var requestCountMaxToLogging = 100000;
        var loggingCount = 0;
        var loggingCountUpdateCount = 25;
        var logRequests = false;
        string result = string.Empty;
        var tasks = new List<Task<string>>();

        CancelKeyPress +=  (object sender, ConsoleCancelEventArgs args) =>
        {
            PrintExitMessage(driverTimer.Elapsed, requestCount-1);
        };

        WriteLine($"Starting HTTP Driver for URL: {url}");

        driverTimer.Start();

        while (true)
        {
            if (requests >0 && requestCount >= requests )
            {
                return;
            }

            if (progressivelyIncreaseRate)
            {
                await Task.Delay(rateDelay);
                if (rateDelay < 1)
                {
                    progressivelyIncreaseRate = false;
                }
                else if (requestCount % 10 == 0)
                {
                    rateDelay = (int)(rateDelay * .5);
                }
            }
            
            if (requestCount % requestCountToLog == 0)
            {
                logRequests = true;
            }
            else
            {
                logRequests = false;
            }

            var task = client.GetStringAsync(url);
            tasks.Add(task);

            if (logRequests)
            {
                try 
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception e) when (e is System.Net.Http.HttpRequestException || e is System.IO.IOException)
                {
                    WriteLine($"Request failed at url: {url}");
                    PrintExitMessage(driverTimer.Elapsed,requestCount);
                    return;
                }
                result = await tasks[0];
                var length = result.Length * 8;
                WriteLine($"request # {requestCount}; requests: {requestCountToLog}; time: {iterationTimer.Elapsed.Seconds}.{iterationTimer.Elapsed.Milliseconds} seconds; Bytes: {length};");
                if (showResponse)
                {
                    WriteLine($"{result}");
                }
                tasks.Clear();
                iterationTimer.Restart();

                if ( loggingCount++ >=loggingCountUpdateCount && 
                    requestCountToLog < requestCountMaxToLogging)
                {
                    requestCountToLog *= 2;
                    loggingCount = 0;
                }
            }

            requestCount++;
        }
    }

    private static void PrintExitMessage(TimeSpan elapsedTime, int requests)
    {
        WriteLine($"total requests: {requests}; time: {elapsedTime.Seconds}.{elapsedTime.Milliseconds} seconds");
    }
    
}