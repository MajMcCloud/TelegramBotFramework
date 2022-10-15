using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.DataSources;

public class List : FormBase
{
    private ButtonGrid _buttons;

    public List()
    {
        Init += List_Init;
    }

    private Task List_Init(object sender, InitEventArgs e)
    {
        _buttons = new ButtonGrid
        {
            EnablePaging = true,
            EnableSearch = false
        };

        _buttons.ButtonClicked += __buttons_ButtonClicked;
        _buttons.KeyboardType = EKeyboardType.ReplyKeyboard;
        _buttons.DeleteReplyMessage = true;

        _buttons.HeadLayoutButtonRow = new ButtonRow(new ButtonBase("Back", "back"));

        var cds = new CustomDataSource();
        _buttons.DataSource = cds;

        AddControl(_buttons);
        return Task.CompletedTask;
    }

    private async Task __buttons_ButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        switch (e.Button.Value)
        {
            case "back":

                var mn = new Menu();
                await NavigateTo(mn);

                break;
        }
    }
}
