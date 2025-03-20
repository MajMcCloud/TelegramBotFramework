﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemCommandsBot;

public class Config
{
    public Config()
    {
        Commands = new List<Command>();
    }

    public string Password { get; set; }

    public string ApiKey { get; set; }

    public List<Command> Commands { get; set; }

    public void LoadDefaultValues()
    {
        ApiKey = "";
        Commands.Add(new Command
        {
            Id = 0, Enabled = true, Title = "Test Befehl", ShellCmd = "explorer.exe", Action = "start",
            MaxInstances = 2
        });
    }


    public static Config Load()
    {
        try
        {
            return Load(AppContext.BaseDirectory + "config\\default.cfg");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }


    public static Config Load(string path)
    {
        try
        {
            var cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText(path));
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }

    public void Save(string path)
    {
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(this));
        }
        catch
        {
        }
    }
}
