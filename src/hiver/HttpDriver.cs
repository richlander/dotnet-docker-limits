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
        var requestTimer = new Stopwatch();
        var rateDelay = 400;
        var requestCount = 0;
        var requestCountUntilLog = 1;
        var requestCountMaxUntilLog = 4096;
        var loggingCount = 0;
        var loggingCountUpdateCount = 25;
        var logRequests = false;
        string result = string.Empty;

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
                requestTimer.Stop();
                await Task.Delay(rateDelay);
                requestTimer.Start();
                if (rateDelay < 1)
                {
                    progressivelyIncreaseRate = false;
                }
                else if (requestCount % 10 == 0)
                {
                    rateDelay = (int)(rateDelay * .5);
                }
            }
            
            if (requestCount % requestCountUntilLog == 0)
            {
                logRequests = true;
            }
            else
            {
                logRequests = false;
            }

            try
            {
                result = await client.GetStringAsync(url);
            }
            catch (Exception e) when (e is System.Net.Http.HttpRequestException || e is System.IO.IOException)
            {
                WriteLine($"Request failed at url: {url}");
                PrintExitMessage(driverTimer.Elapsed, requestCount);
                return;
            }

            if (logRequests)
            {
                var length = result.Length * 8;
                var duration = requestTimer.Elapsed;
                var perRequestTime = duration.TotalMilliseconds > 0 ? duration.TotalMilliseconds / 1000 / requestCountUntilLog : 0; 
                WriteLine($"request: {requestCount}; bytes: {length}; request count: {requestCountUntilLog}; time: {duration.Seconds}.{duration.Milliseconds}; time/request: {perRequestTime:N4}");
                if (showResponse)
                {
                    WriteLine($"{result}");
                }

                if (loggingCount++ >=loggingCountUpdateCount && 
                    requestCountUntilLog < requestCountMaxUntilLog)
                {
                    requestCountUntilLog *= 2;
                    loggingCount = 0;
                }
                requestTimer.Restart();
            }

            requestCount++;
        }
    }

    private static void PrintExitMessage(TimeSpan elapsedTime, int requests)
    {
        WriteLine($"total requests: {requests}; time: {elapsedTime.Seconds}.{elapsedTime.Milliseconds} seconds");
    }
    
}