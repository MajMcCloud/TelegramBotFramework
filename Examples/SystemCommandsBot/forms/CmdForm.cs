using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace SystemCommandsBot.forms
{
    public class CmdForm : TelegramBotBase.Form.AutoCleanForm
    {
        public DateTime ExpiresAt { get; set; }

        public int? MessageId { get; set; }

        public override async Task Load(MessageResult message)
        {

        }

        public override async Task Action(MessageResult message)
        {
            var btn = message.RawData;

            int id = -1;

            if (!int.TryParse(btn, out id))
            {

                return;
            }

            var cmd = Program.BotConfig.Commandos.Where(a => a.Enabled && a.ID == id).FirstOrDefault();
            if (cmd == null)
            {
                await this.Device.Send("Cmd nicht verfügbar.");
                return;
            }

            message.Handled = true;

            switch (cmd.Action)
            {
                case "start":

                    FileInfo fi = new FileInfo(cmd.ShellCmd);

                    if (cmd.MaxInstances != null && cmd.MaxInstances >= 0)
                    {

                        if (Process.GetProcessesByName(cmd.ProcName).Count() >= cmd.MaxInstances)
                        {
                            await this.Device.Send("Anwendung läuft bereits.");
                            await message.ConfirmAction("Anwendung läuft bereits.");

                            return;
                        }

                    }

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = cmd.ShellCmd;
                    psi.WorkingDirectory = fi.DirectoryName;
                    psi.UseShellExecute = cmd.UseShell;

                    Process.Start(psi);

                    await this.Device.Send(fi.Name + " wurde gestarted.");

                    await message.ConfirmAction(fi.Name + " wurde gestarted.");

                    break;

                case "kill":

                    FileInfo fi2 = new FileInfo(cmd.ShellCmd);

                    String pros = fi2.Name.Replace(fi2.Extension, "");

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

                    await this.Device.Send(fi2.Name + " wurde beendet.");

                    await message.ConfirmAction(fi2.Name + " wurde beendet.");

                    break;

                case "restart":

                    FileInfo fi3 = new FileInfo(cmd.ShellCmd);

                    String pros2 = fi3.Name.Replace(fi3.Extension, "");

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

                    FileInfo fi4 = new FileInfo(cmd.ShellCmd);

                    ProcessStartInfo psi2 = new ProcessStartInfo();
                    psi2.FileName = cmd.ShellCmd;
                    psi2.WorkingDirectory = fi4.DirectoryName;
                    psi2.FileName = cmd.ShellCmd;
                    psi2.UseShellExecute = cmd.UseShell;

                    Process.Start(psi2);

                    await this.Device.Send(fi3.Name + " wurde neugestarted.");
                    await message.ConfirmAction(fi3.Name + " wurde neugestarted.");


                    break;

                default:

                    await message.ConfirmAction();

                    break;

            }




        }

        public override async Task Render(MessageResult message)
        {
            if (this.MessageId == null)
            {
                var buttons = Program.BotConfig.Commandos.Where(a => a.Enabled).Select(a => new ButtonBase(a.Title, a.ID.ToString()));

                ButtonForm bf = new ButtonForm();
                bf.AddSplitted(buttons, 1);
                await this.Device.Send("Deine Optionen", bf);

                return;
            }


        }




    }
}
