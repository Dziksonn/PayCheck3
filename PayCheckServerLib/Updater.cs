﻿using Newtonsoft.Json;
using System.Security.Cryptography;

namespace PayCheckServerLib
{
    public class Updater
    {
        // that or https://raw.githubusercontent.com/MoolahModding/PayCheck3Files/main
        static readonly string FilesUrl = @"https://github.com/MoolahModding/PayCheck3Files/raw/main/";

        public static async void CheckForJsonUpdates(bool UIHandleUpdate = false)
        {
            Dictionary<string, string> LocalFiles = new();
            foreach (var file in Directory.GetFiles("./Files"))
            {
                string hash = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(file))).Replace("-", "").ToLower();
                LocalFiles.Add(file.Replace("./Files\\", ""), hash);
            }

            Dictionary<string, string> Files = new();
            if (FilesUrl.StartsWith("http"))
            {
                try
                {
                    HttpClient client = new();
                    var FilesData = await client.GetStringAsync(FilesUrl + "Hashes.json");
                    Files = JsonConvert.DeserializeObject<Dictionary<string, string>>(FilesData)!;
                }
                catch
                {
                    Debugger.PrintWarn("Unable to fetch hash list for updates", "Updater");
                    return;
                }
            }
            else
            {
                Files = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Join(FilesUrl, "Hashes.json")))!;
            }

            foreach (var KeyPair in Files)
            {
                try
                {
                    if (LocalFiles[KeyPair.Key] != Files[KeyPair.Key])
                    {
                        Debugger.PrintInfo(KeyPair.Key + " is out of date", "Updater");
                        if (UIHandleUpdate)
                        {
                            //send stuff to ui to update
                            UpdateWithUI?.Invoke(null, KeyPair.Key);
                        }
                        else
                        {
                            Console.WriteLine("You want to update?\n Y/y = Yes , N/n = No");

                            var inp = Console.ReadLine();

                            if (inp == null || string.IsNullOrEmpty(inp))
                            {
                                Console.WriteLine("Your input was wrong, skipping");
                                continue;
                            }

                            inp = inp.ToLower();
                            if (inp == "y")
                            {
                                Console.WriteLine("Updating started!");
                                HttpClient client = new();
                                var FilesData = await client.GetStringAsync(FilesUrl + KeyPair.Key);
                                File.WriteAllText(KeyPair.Key, FilesData);
                            }
                            else
                            {
                                Console.WriteLine("Not want to update, Skipping");
                                continue;
                            }


                        }
                    }
                }
                catch
                {
                    Debugger.PrintWarn("Unable to fetch get file to update", "Updater");
                }
            }
        }

        public static event EventHandler<string> UpdateWithUI;


    }
}
