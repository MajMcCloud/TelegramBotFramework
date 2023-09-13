using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Example.Tests.Controls;
using TelegramBotBase.Example.Tests.DataSources;
using TelegramBotBase.Example.Tests.Groups;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

public class Menu : AutoCleanForm
{
    public Menu()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
    }

    public override async Task Load(MessageResult message)
    {
        if ((message.Message.Chat.Type == ChatType.Group) | (message.Message.Chat.Type == ChatType.Supergroup))
        {
            var sf = new WelcomeUser();

            await NavigateTo(sf);
        }


        await Device.HideReplyKeyboard();
    }

    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();

        await message.ConfirmAction();


        if (call == null)
        {
            return;
        }

        message.Handled = true;

        switch (call.Value)
        {
            case "text":

                var sf = new SimpleForm();

                await NavigateTo(sf);

                break;

            case "buttons":

                var bf = new ButtonTestForm();

                await NavigateTo(bf);

                break;

            case "progress":

                var pf = new ProgressTest();

                await NavigateTo(pf);

                break;

            case "registration":

                var reg = new Register.Start();

                await NavigateTo(reg);

                break;

            case "form1":

                var form1 = new TestForm();

                await NavigateTo(form1);

                break;

            case "form2":

                var form2 = new TestForm2();

                await NavigateTo(form2);

                break;

            case "data":

                var data = new DataForm();

                await NavigateTo(data);

                break;

            case "calendar":

                var calendar = new CalendarPickerForm();

                await NavigateTo(calendar);

                break;

            case "month":

                var month = new MonthPickerForm();

                await NavigateTo(month);

                break;

            case "treeview":

                var tree = new TreeViewForms();

                await NavigateTo(tree);

                break;

            case "togglebuttons":

                var tb = new ToggleButtonForm();

                await NavigateTo(tb);

                break;

            case "multitogglebuttons":

                var mtb = new MultiToggleButtonForm();

                await NavigateTo(mtb);

                break;

            case "buttongrid":

                var bg = new ButtonGridForm();

                await NavigateTo(bg);

                break;

            case "buttongridfilter":

                var bg2 = new ButtonGridPagingForm();

                await NavigateTo(bg2);

                break;

            case "buttongridtags":

                var bg3 = new ButtonGridTagForm();

                await NavigateTo(bg3);

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

            case "dynamicbuttongrid":

                var dg = new List();

                await NavigateTo(dg);

                break;

            case "notifications":

                var not = new Notifications.Start();
                await NavigateTo(not);

                break;

            case "label":

                var lf = new LabelForm();

                await NavigateTo(lf);

                break;

            case "arraypromptdialog":

                var apt = new ArrayPromptDialogTest();

                await NavigateTo(apt);

                break;

            default:

                message.Handled = false;

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        var btn = new ButtonForm();

        btn.AddButtonRow(new ButtonBase("#1 Simple Text", new CallbackData("a", "text").Serialize()),
                         new ButtonBase("#2 Button Test", new CallbackData("a", "buttons").Serialize()));
        btn.AddButtonRow(new ButtonBase("#3 Progress Bar", new CallbackData("a", "progress").Serialize()));
        btn.AddButtonRow(new ButtonBase("#4 Registration Example", new CallbackData("a", "registration").Serialize()));

        btn.AddButtonRow(new ButtonBase("#5 Form1 Command", new CallbackData("a", "form1").Serialize()));

        btn.AddButtonRow(new ButtonBase("#6 Form2 Command", new CallbackData("a", "form2").Serialize()));

        btn.AddButtonRow(new ButtonBase("#7 Data Handling", new CallbackData("a", "data").Serialize()));
        btn.AddButtonRow(new ButtonBase("#8 Calendar Picker", new CallbackData("a", "calendar").Serialize()));
        btn.AddButtonRow(new ButtonBase("#9 Month Picker", new CallbackData("a", "month").Serialize()));

        btn.AddButtonRow(new ButtonBase("#10 TreeView", new CallbackData("a", "treeview").Serialize()));

        btn.AddButtonRow(new ButtonBase("#11 ToggleButtons", new CallbackData("a", "togglebuttons").Serialize()));

        btn.AddButtonRow(new ButtonBase("#11.2 MultiToggleButtons",
                                        new CallbackData("a", "multitogglebuttons").Serialize()));

        btn.AddButtonRow(new ButtonBase("#12 ButtonGrid", new CallbackData("a", "buttongrid").Serialize()));

        btn.AddButtonRow(new ButtonBase("#13 ButtonGrid Paging & Filter",
                                        new CallbackData("a", "buttongridfilter").Serialize()));

        btn.AddButtonRow(new ButtonBase("#14 ButtonGrid Tags (Filter)",
                                        new CallbackData("a", "buttongridtags").Serialize()));

        btn.AddButtonRow(new ButtonBase("#15 MultiView", new CallbackData("a", "multiview").Serialize()));

        btn.AddButtonRow(
            new ButtonBase("#16 CheckedButtonList", new CallbackData("a", "checkedbuttonlist").Serialize()));

        btn.AddButtonRow(new ButtonBase("#17 NavigationController (Push/Pop)",
                                        new CallbackData("a", "navigationcontroller").Serialize()));

        btn.AddButtonRow(new ButtonBase("#18 Dynamic ButtonGrid (DataSources)",
                                        new CallbackData("a", "dynamicbuttongrid").Serialize()));

        btn.AddButtonRow(new ButtonBase("#19 Notifications", new CallbackData("a", "notifications").Serialize()));


        btn.AddButtonRow(new ButtonBase("#20 Label", new CallbackData("a", "label").Serialize()));

        btn.AddButtonRow(new ButtonBase("#21 ArrayPromptDialogTest", new CallbackData("a", "arraypromptdialog").Serialize()));

        await Device.Send("Choose your test:", btn);
    }
}
