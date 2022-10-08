using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace SystemCommandsBot.forms;

public class CmdForm : AutoCleanForm
{
    public DateTime ExpiresAt { get; set; }

    public int? MessageId { get; set; }

    public override Task Load(MessageResult message)
    {
        return Task.CompletedTask;
    }

    public override async Task Action(MessageResult message)
    {
        var btn = message.RawData;

        var id = -1;

        if (!int.TryParse(btn, out id))
        {
            return;
        }

        var cmd = Program.BotConfig.Commandos.Where(a => a.Enabled && a.Id == id).FirstOrDefault();
        if (cmd == null)
        {
            await Device.Send("Cmd nicht verfügbar.");
            return;
        }

        message.Handled = true;

        switch (cmd.Action)
        {
            case "start":

                var fi = new FileInfo(cmd.ShellCmd);

                if (cmd.MaxInstances != null && cmd.MaxInstances >= 0)
                {
                    if (Process.GetProcessesByName(cmd.ProcName).Count() >= cmd.MaxInstances)
                    {
                        await Device.Send("Anwendung läuft bereits.");
                        await message.ConfirmAction("Anwendung läuft bereits.");

                        return;
                    }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = cmd.ShellCmd,
                    WorkingDirectory = fi.DirectoryName,
                    UseShellExecute = cmd.UseShell
                };

                Process.Start(psi);

                await Device.Send(fi.Name + " wurde gestarted.");

                await message.ConfirmAction(fi.Name + " wurde gestarted.");

                break;

            case "kill":

                var fi2 = new FileInfo(cmd.ShellCmd);

                var pros = fi2.Name.Replace(fi2.Extension, "");

                var proc = Process.GetProcessesByName(pros).ToList();

                foreach (var p in proc)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch
                    {
                    }
                }

                await Device.Send(fi2.Name + " wurde beendet.");

                await message.ConfirmAction(fi2.Name + " wurde beendet.");

                break;

            case "restart":

                var fi3 = new FileInfo(cmd.ShellCmd);

                var pros2 = fi3.Name.Replace(fi3.Extension, "");

                var proc2 = Process.GetProcessesByName(pros2).ToList();

                foreach (var p in proc2)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch
                    {
                    }
                }

                var fi4 = new FileInfo(cmd.ShellCmd);

                var psi2 = new ProcessStartInfo
                {
                    FileName = cmd.ShellCmd,
                    WorkingDirectory = fi4.DirectoryName
                };
                psi2.FileName = cmd.ShellCmd;
                psi2.UseShellExecute = cmd.UseShell;

                Process.Start(psi2);

                await Device.Send(fi3.Name + " wurde neugestarted.");
                await message.ConfirmAction(fi3.Name + " wurde neugestarted.");


                break;

            default:

                await message.ConfirmAction();

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        if (MessageId == null)
        {
            var buttons = Program.BotConfig.Commandos.Where(a => a.Enabled)
                                 .Select(a => new ButtonBase(a.Title, a.Id.ToString()));

            var bf = new ButtonForm();
            bf.AddSplitted(buttons, 1);
            await Device.Send("Deine Optionen", bf);
        }
    }
}