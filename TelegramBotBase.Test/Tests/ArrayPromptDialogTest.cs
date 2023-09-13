using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests
{
    public class ArrayPromptDialogTest : ArrayPromptDialog
    {

        public ArrayPromptDialogTest() : base("Choose an option", new[] { new ButtonBase("Option 1", "option1"), new ButtonBase("Option 2", "option2") })
        {

            this.ButtonClicked += ArrayPromptDialogTest_ButtonClicked;
        }

        private void ArrayPromptDialogTest_ButtonClicked(object sender, Args.ButtonClickedEventArgs e)
        {

            Console.WriteLine(e.Button.Text + " has been clicked");


        }
    }
}
