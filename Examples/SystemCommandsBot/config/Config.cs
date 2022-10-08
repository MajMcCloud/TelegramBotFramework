using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SystemCommandsBot.commands;

namespace SystemCommandsBot.config
{
    public class Config
    {
        public string Password { get; set; }

        public string ApiKey { get; set; }

        public List<Commando> Commandos { get; set; }


        public Config()
        {
            Commandos = new List<Commando>();
        }

        public void LoadDefaultValues()
        {
            ApiKey = "";
            Commandos.Add(new Commando { Id = 0, Enabled = true, Title = "Test Befehl", ShellCmd = "explorer.exe", Action = "start", MaxInstances = 2 });
        }


        public static Config Load()
        {
            try
            {
                return Load(AppContext.BaseDirectory + "config\\default.cfg");

                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }


        public static Config Load(string path)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                return cfg;
            }
            catch (DirectoryNotFoundException)
            {
                var di = new DirectoryInfo(path);

                if (!Directory.Exists(di.Parent.FullName))
                {
                    Directory.CreateDirectory(di.Parent.FullName);
                }

                var cfg = new Config();
                cfg.LoadDefaultValues();
                cfg.Save(path);
                return cfg;
            }
            catch (FileNotFoundException)
            {
                var cfg = new Config();
                cfg.LoadDefaultValues();
                cfg.Save(path);
                return cfg;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public void Save(string path)
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(this));
            }
            catch
            {

            }
        }
    }
}
