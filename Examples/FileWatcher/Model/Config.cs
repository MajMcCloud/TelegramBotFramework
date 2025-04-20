using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileWatcher.Model
{
    public class Config
    {

        public String APIKey { get; set; } = "";

        public String DirectoryToWatch { get; set; } = "";

        public List<long> DeviceIds { get; set; } = new List<long>();

        public string Filter { get; set; } = "*.*";

        public List<string> FilesToExclude { get; set; } = new List<string>() { "anything.txt" , "others.txt" };



        public static Config Load()
        {
            Config config = new Config();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            try
            {
                if (!File.Exists(path))
                {
                    config.Save();
                }

                var content = File.ReadAllText(path);

                config = JsonSerializer.Deserialize<Config>(content);
            }
            catch
            {

            }


            return config;
        }

        public void Save()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            Save(path);
        }

        public void Save(String path)
        {

            try
            {
                if (File.Exists(path))
                    File.Delete(path);


                var content = System.Text.Json.JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });

                File.WriteAllText(path, content);
            }
            catch
            {

            }

        }

    }
}
