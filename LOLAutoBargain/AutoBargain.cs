using JPClientStart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LOLAutoBargain
{
    class AutoBargain
    {
        public static string Path
        {
            get
            {
                return Regex.Match(commandLine, pathPattern).Value;
            }
        }

        public static string Args
        {
            get
            {
                return commandLine.Substring(Path.Length + 1);
            }
        }

        private static string landingTokenPattern = @"--landing-token=(\w*) ";

        private static string pathPattern = @""".*RiotClientServices\.exe""";

        private static string commandLine;

        private static string landingToken;

        private static bool gotClientcommandLine = false;

        public static async Task Run()
        {
            Console.WriteLine("Finding RiotClientServices ...");

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName == "RiotClientServices")
                    {
                        commandLine = process.GetCommandLine();
                        gotClientcommandLine = true;
                        Console.WriteLine("RiotClientServices found");
                        break;
                    }
                }
                catch (Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
                {
                    // Intentionally empty - no security access to the process.
                }
                catch (InvalidOperationException)
                {
                    // Intentionally empty - the process exited before getting details.
                }
            }
            if (!gotClientcommandLine)
            {
                Console.WriteLine("Client process not found! Press any key to go back...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("---------------------------------------------------------");

            Console.WriteLine("Getting landing-token");
            landingToken = Regex.Match(commandLine, landingTokenPattern).Groups[1].Value;
            Console.WriteLine($"Getting landing-token successfully: {landingToken}");

            Console.WriteLine("---------------------------------------------------------");

            Console.WriteLine("Reading code file...");
            string filePath = "Commentator.txt";
            string[] lines = System.IO.File.ReadAllLines(filePath);
            List<string> codes = new List<string>();
            foreach (string line in lines)
            {
                var code = Regex.Match(line, @"LOL\w{10}").Value;
                if (code.Length != 0)
                {
                    codes.Add(code);
                    //Console.WriteLine(code);
                }
            }
            Console.WriteLine($"Read code file successfully: {codes.Count}");

            Console.WriteLine("---------------------------------------------------------");

            CookieContainer cookies = new();
            HttpClientHandler handler = new();
            handler.CookieContainer = cookies;
            HttpClient client = new(handler);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("token", landingToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) LeagueOfLegendsClient/11.15.388.2387 (CEF 74) Safari/537.36");
            client.DefaultRequestHeaders.Add("referer", $"https://bargain.lol.garena.vn/?token={landingToken}");
            client.DefaultRequestHeaders.Add("origin", $"https://bargain.lol.garena.vn");
            client.DefaultRequestHeaders.Add("accept", $"application/json, text/plain, */*");


            Console.WriteLine("Getting profile ...");
            var configResponse = client.GetAsync("https://bargain.lol.garena.vn/api/config");
            var cfgmsg = await configResponse;
            Console.WriteLine(await cfgmsg.Content.ReadAsStringAsync());
            Console.WriteLine("---------------------------------------------------------");

            var profileResponse = client.GetAsync("https://bargain.lol.garena.vn/api/profile");
            var msg = await profileResponse;
            Console.WriteLine(await msg.Content.ReadAsStringAsync());
            Console.WriteLine("---------------------------------------------------------");

            Uri uri = new Uri("https://bargain.lol.garena.vn");
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies)
            {
                Console.WriteLine(cookie.Name + ": " + cookie.Value);
            }

            Console.WriteLine("Spamming codes ...");

            for (int i = 0; i < codes.Count; i++)
            {
                var jsonObject = new StringContent($"{{\"code\":\"{codes[i]}\",\"confirm\": false}}", Encoding.UTF8, "application/json");
                var tempEnterResponse = client.PostAsync("https://bargain.lol.garena.vn/api/enter", jsonObject);
                var tempEntermsg = await tempEnterResponse;
                var tempMsgstr = await tempEntermsg.Content.ReadAsStringAsync();

                var jsonObject2 = new StringContent($"{{\"code\":\"{codes[i]}\",\"confirm\": true}}", Encoding.UTF8, "application/json");
                Console.WriteLine(codes[i]);
                var enterResponse = client.PostAsync("https://bargain.lol.garena.vn/api/enter", jsonObject2);
                var entermsg = await enterResponse;
                var msgstr = await entermsg.Content.ReadAsStringAsync();

                Console.WriteLine(msgstr);
                if (msgstr == "{\"error\": \"ERROR__ENTER_CODE_AMOUNT_OUT_OF_QUOTA\"}")
                {
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine("MAXIMUM REACHED!");
                    break;
                }
                Console.WriteLine("---------------------------------------------------------");
            }

        }
    }
}
