using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Datasources
{
    public class List : FormBase
    {
        ButtonGrid __buttons = null;

        public List()
        {
            this.Init += List_Init;
        }

        private async Task List_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {

            __buttons = new ButtonGrid();

            __buttons.EnablePaging = true;
            __buttons.EnableSearch = false;
            __buttons.ButtonClicked += __buttons_ButtonClicked;
            __buttons.KeyboardType = TelegramBotBase.Enums.eKeyboardType.ReplyKeyboard;
            __buttons.DeleteReplyMessage = true;

            __buttons.HeadLayoutButtonRow = new ButtonRow(new ButtonBase("Back", "back"));

            var cds = new CustomDataSource();
            __buttons.DataSource = cds;

            AddControl(__buttons);
        }

        private async Task __buttons_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            switch(e.Button.Value)
            {
                case "back":

                    var mn = new Menu();
                    await NavigateTo(mn);

                    break;
            }
        }


    }
}
