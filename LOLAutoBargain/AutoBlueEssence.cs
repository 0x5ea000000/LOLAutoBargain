﻿using JPClientStart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LOLAutoBargain
{
    class AutoBlueEssence
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

            CookieContainer cookies = new();
            HttpClientHandler handler = new();
            handler.CookieContainer = cookies;
            HttpClient client = new(handler);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("token", landingToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) LeagueOfLegendsClient/11.15.388.2387 (CEF 74) Safari/537.36");
            client.DefaultRequestHeaders.Add("referer", $"https://bargain.lol.garena.vn/?token={landingToken}");

            //Console.WriteLine("Getting profile ...");
            //var configResponse = client.GetAsync("https://bargain.lol.garena.vn/api/config");
            //var cfgmsg = await configResponse;
            //Console.WriteLine(await cfgmsg.Content.ReadAsStringAsync());
            //Console.WriteLine("---------------------------------------------------------");

            //var profileResponse = client.GetAsync("https://bargain.lol.garena.vn/api/profile");
            //var msg = await profileResponse;
            //Console.WriteLine(await msg.Content.ReadAsStringAsync());
            //Console.WriteLine("---------------------------------------------------------");

            //Uri uri = new Uri("https://bargain.lol.garena.vn");
            //IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
            //foreach (Cookie cookie in responseCookies)
            //{
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);
            //}

            Console.WriteLine("Convert Blue Escence 50 times ...");

            for (int i = 0; i < 50; i++)
            {
                var jsonObject = new StringContent($"{{\"type\":2,\"item_id\": 9}}", Encoding.UTF8, "application/json");
                Console.WriteLine(i);

                var enterResponse = client.PostAsync("https://bargain.lol.garena.vn/api/redeem", jsonObject);

                var entermsg = await enterResponse;

                var msgstr = await entermsg.Content.ReadAsStringAsync();

                Console.WriteLine(msgstr);
                Console.WriteLine("---------------------------------------------------------");

            }
            Console.WriteLine("MAXIMUM REACHED!");
        }
    }
}
