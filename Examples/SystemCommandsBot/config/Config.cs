using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SystemCommandsBot.config
{
    public class Config
    {
        public String Password { get; set; }

        public String ApiKey { get; set; }

        public List<commands.Commando> Commandos { get; set; }


        public Config()
        {
            this.Commandos = new List<commands.Commando>();
        }

        public void loadDefaultValues()
        {
            this.ApiKey = "";
            this.Commandos.Add(new commands.Commando() { ID = 0, Enabled = true, Title = "Test Befehl", ShellCmd = "explorer.exe", Action = "start", MaxInstances = 2 });
        }


        public static Config load()
        {
            try
            {
                return load(AppContext.BaseDirectory + "config\\default.cfg");

                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }


        public static Config load(String path)
        {
            try
            {
                var cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) as Config;
                return cfg;
            }
            catch (DirectoryNotFoundException ex)
            {
                DirectoryInfo di = new DirectoryInfo(path);

                if (!Directory.Exists(di.Parent.FullName))
                {
                    Directory.CreateDirectory(di.Parent.FullName);
                }

                var cfg = new Config();
                cfg.loadDefaultValues();
                cfg.save(path);
                return cfg;
            }
            catch (FileNotFoundException ex)
            {
                var cfg = new Config();
                cfg.loadDefaultValues();
                cfg.save(path);
                return cfg;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public void save(String path)
        {
            try
            {
                File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(this));
            }
            catch
            {

            }
        }
    }
}
