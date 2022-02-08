using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Controls
{
    public class ButtonGridTagForm : AutoCleanForm
    {

        TaggedButtonGrid m_Buttons = null;

        public ButtonGridTagForm()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;

            this.Init += ButtonGridTagForm_Init;
        }

        private async Task ButtonGridTagForm_Init(object sender, InitEventArgs e)
        {
            m_Buttons = new TaggedButtonGrid();

            m_Buttons.KeyboardType = TelegramBotBase.Enums.eKeyboardType.ReplyKeyboard;

            m_Buttons.EnablePaging = true;

            m_Buttons.HeadLayoutButtonRow = new List<ButtonBase>() { new ButtonBase("Back", "back") };
            

            var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            ButtonForm bf = new ButtonForm();

            foreach (var c in countries)
            {
                bf.AddButtonRow(new TagButtonBase(c.EnglishName, c.EnglishName, c.Parent.EnglishName));
            }

            m_Buttons.Tags = countries.Select(a => a.Parent.EnglishName).Distinct().OrderBy(a => a).ToList();
            m_Buttons.SelectedTags = countries.Select(a => a.Parent.EnglishName).Distinct().OrderBy(a => a).ToList();

            m_Buttons.EnableCheckAllTools = true;

            m_Buttons.DataSource = new TelegramBotBase.Datasources.ButtonFormDataSource(bf);

            m_Buttons.ButtonClicked += Bg_ButtonClicked;

            this.AddControl(m_Buttons);
        }

        private async Task Bg_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            if (e.Button == null)
                return;

            switch (e.Button.Value)
            {

                case "back":
                    var start = new Menu();
                    await this.NavigateTo(start);
                    return;

            }


            await this.Device.Send($"Button clicked with Text: {e.Button.Text} and Value {e.Button.Value}");
        }
    }
}
