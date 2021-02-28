using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Controls
{
    public class CheckedButtonListForm : AutoCleanForm
    {

        CheckedButtonList m_Buttons = null;

        public CheckedButtonListForm()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;

            this.Init += CheckedButtonListForm_Init;
        }

        private async Task CheckedButtonListForm_Init(object sender, InitEventArgs e)
        {
            m_Buttons = new CheckedButtonList();

            m_Buttons.KeyboardType = TelegramBotBase.Enums.eKeyboardType.InlineKeyBoard;
            m_Buttons.EnablePaging = true;

            m_Buttons.HeadLayoutButtonRow = new List<ButtonBase>() { new ButtonBase("Back", "back"), new ButtonBase("Switch Keyboard", "switch") };

            m_Buttons.SubHeadLayoutButtonRow = new List<ButtonBase>() { new ButtonBase("No checked items", "$") };

            ButtonForm bf = new ButtonForm();

            for(int i = 0;i < 30;i++)
            {
                bf.AddButtonRow($"{i}. Item", i.ToString());
            }

            m_Buttons.ButtonsForm = bf;

            m_Buttons.ButtonClicked += Bg_ButtonClicked;
            m_Buttons.CheckedChanged += M_Buttons_CheckedChanged;

            this.AddControl(m_Buttons);
        }

        private async Task M_Buttons_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            m_Buttons.SubHeadLayoutButtonRow = new List<ButtonBase>() { new ButtonBase($"{m_Buttons.CheckedItems.Count} checked items", "$") };
        }

        private async Task Bg_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            if (e.Button == null)
                return;

            if (e.Button.Value == "back")
            {
                var start = new Menu();
                await this.NavigateTo(start);
            }
            else if (e.Button.Value == "switch")
            {
                switch (m_Buttons.KeyboardType)
                {
                    case TelegramBotBase.Enums.eKeyboardType.ReplyKeyboard:
                        m_Buttons.KeyboardType = TelegramBotBase.Enums.eKeyboardType.InlineKeyBoard;
                        break;
                    case TelegramBotBase.Enums.eKeyboardType.InlineKeyBoard:
                        m_Buttons.KeyboardType = TelegramBotBase.Enums.eKeyboardType.ReplyKeyboard;
                        break;
                }


            }
            else
            {

                await this.Device.Send($"Button clicked with Text: {e.Button.Text} and Value {e.Button.Value}");
            }


        }
    }
}
