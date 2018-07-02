using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace aspnetapi_driver
{
    class Program
    {
        private static string _url = string.Empty;
        private static Dictionary<string,string> _args = new Dictionary<string, string>();
        private static readonly string PROGRESSIVELY_INCREASE_RATE = "--progressivelyincreaserate";
        private static readonly string SHOW_RESPONSE = "--showresponse";
        private static readonly string REQUESTS = "--requests";

        static async Task Main(string[] args)
        {
            // Command-line args
            // aspnetapi-driver url [--requests int] [--progressivelyIncreaseRate bool] [--showResponse bool]
            //var url = "http://localhost:8000/api/values";
            var requests = 0;
            var progressivelyIncreaseRate = false;
            var showResponse = false;

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("No Url provided -- provide one.");
                return;
            }

            GetCommandLineArgs(args);

            if (_args.ContainsKey(REQUESTS))
            {
                requests = int.Parse(_args[REQUESTS]);
            }

            if (_args.ContainsKey(SHOW_RESPONSE))
            {
                showResponse = _args[SHOW_RESPONSE] == "true" ? true : false;
            }

            if (_args.ContainsKey(PROGRESSIVELY_INCREASE_RATE))
            {
                progressivelyIncreaseRate = _args[PROGRESSIVELY_INCREASE_RATE] == "true" ? true : false;
            }

            await HttpDriver.Go(_url,requests, progressivelyIncreaseRate, showResponse);
        }

        static void GetCommandLineArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _url = args[0];

            if (args.Length == 1)
            {
                return;
            }
            
            for(var i = 1; i < args.Length;i++)
            {
                var key = args[i].ToLowerInvariant();
                var value = string.Empty;

                if (args.Length > i && key.StartsWith("-"))
                {
                    value = args[i+1].ToLowerInvariant();
                }
                else
                {
                    value = key;
                }
                _args.Add(key,value);
                i++;
            }
        }
    }
}
