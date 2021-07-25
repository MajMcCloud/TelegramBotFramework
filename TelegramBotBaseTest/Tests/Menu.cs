using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBaseTest.Tests.Controls;

namespace TelegramBotBaseTest.Tests
{
    public class Menu : AutoCleanForm
    {
        public Menu()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
        }

        public override async Task Load(MessageResult message)
        {


            if (message.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group | message.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
            {
                var sf = new TelegramBotBaseTest.Tests.Groups.WelcomeUser();

                await this.NavigateTo(sf);
            }


            await Device.HideReplyKeyboard();
        }

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();


            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value)
            {
                case "text":

                    var sf = new SimpleForm();

                    await this.NavigateTo(sf);

                    break;

                case "buttons":

                    var bf = new ButtonTestForm();

                    await this.NavigateTo(bf);

                    break;

                case "progress":

                    var pf = new ProgressTest();

                    await this.NavigateTo(pf);

                    break;

                case "registration":

                    var reg = new Register.Start();

                    await this.NavigateTo(reg);

                    break;

                case "form1":

                    var form1 = new TestForm();

                    await this.NavigateTo(form1);

                    break;

                case "form2":

                    var form2 = new TestForm2();

                    await this.NavigateTo(form2);

                    break;

                case "data":

                    var data = new DataForm();

                    await this.NavigateTo(data);

                    break;

                case "calendar":

                    var calendar = new Controls.CalendarPickerForm();

                    await this.NavigateTo(calendar);

                    break;

                case "month":

                    var month = new Controls.MonthPickerForm();

                    await this.NavigateTo(month);

                    break;

                case "treeview":

                    var tree = new Controls.TreeViewForms();

                    await this.NavigateTo(tree);

                    break;

                case "togglebuttons":

                    var tb = new Controls.ToggleButtons();

                    await this.NavigateTo(tb);

                    break;

                case "multitogglebuttons":

                    var mtb = new Controls.MultiToggleButtons();

                    await this.NavigateTo(mtb);

                    break;

                case "buttongrid":

                    var bg = new Controls.ButtonGridForm();

                    await this.NavigateTo(bg);

                    break;

                case "buttongridfilter":

                    var bg2 = new Controls.ButtonGridPagingForm();

                    await this.NavigateTo(bg2);

                    break;

                case "buttongridtags":

                    var bg3 = new Controls.ButtonGridTagForm();

                    await this.NavigateTo(bg3);

                    break;

                case "multiview":

                    var mvf = new MultiViewForm();

                    await NavigateTo(mvf);


                    break;

                case "checkedbuttonlist":

                    var cbl = new CheckedButtonListForm();

                    await NavigateTo(cbl);


                    break;

                case "navigationcontroller":

                    var nc = new Navigation.Start();

                    await NavigateTo(nc);


                    break;

                default:

                    message.Handled = false;

                    break;
            }


        }

        public override async Task Render(MessageResult message)
        {

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("#1 Simple Text", new CallbackData("a", "text").Serialize()), new ButtonBase("#2 Button Test", new CallbackData("a", "buttons").Serialize()));
            btn.AddButtonRow(new ButtonBase("#3 Progress Bar", new CallbackData("a", "progress").Serialize()));
            btn.AddButtonRow(new ButtonBase("#4 Registration Example", new CallbackData("a", "registration").Serialize()));

            btn.AddButtonRow(new ButtonBase("#5 Form1 Command", new CallbackData("a", "form1").Serialize()));

            btn.AddButtonRow(new ButtonBase("#6 Form2 Command", new CallbackData("a", "form2").Serialize()));

            btn.AddButtonRow(new ButtonBase("#7 Data Handling", new CallbackData("a", "data").Serialize()));
            btn.AddButtonRow(new ButtonBase("#8 Calendar Picker", new CallbackData("a", "calendar").Serialize()));
            btn.AddButtonRow(new ButtonBase("#9 Month Picker", new CallbackData("a", "month").Serialize()));

            btn.AddButtonRow(new ButtonBase("#10 TreeView", new CallbackData("a", "treeview").Serialize()));

            btn.AddButtonRow(new ButtonBase("#11 ToggleButtons", new CallbackData("a", "togglebuttons").Serialize()));

            btn.AddButtonRow(new ButtonBase("#11.2 MultiToggleButtons", new CallbackData("a", "multitogglebuttons").Serialize()));

            btn.AddButtonRow(new ButtonBase("#12 ButtonGrid", new CallbackData("a", "buttongrid").Serialize()));

            btn.AddButtonRow(new ButtonBase("#13 ButtonGrid Paging & Filter", new CallbackData("a", "buttongridfilter").Serialize()));

            btn.AddButtonRow(new ButtonBase("#14 ButtonGrid Tags (Filter)", new CallbackData("a", "buttongridtags").Serialize()));

            btn.AddButtonRow(new ButtonBase("#15 MultiView", new CallbackData("a", "multiview").Serialize()));

            btn.AddButtonRow(new ButtonBase("#16 CheckedButtonList", new CallbackData("a", "checkedbuttonlist").Serialize()));

            btn.AddButtonRow(new ButtonBase("#17 NavigationController (Push/Pop)", new CallbackData("a", "navigationcontroller").Serialize()));

            await this.Device.Send("Choose your test:", btn);
        }


    }
}
