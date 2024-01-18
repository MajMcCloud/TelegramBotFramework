﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace DemoBot.Forms
{
    public class HiddenOpenForm : AutoCleanForm
    {

        public Guid guid {  get; set; }

        public HiddenOpenForm()
        {

            DeleteMode = TelegramBotBase.Enums.EDeleteMode.OnLeavingForm;
            DeleteSide = TelegramBotBase.Enums.EDeleteSide.Both;
        }

        public override async Task Action(MessageResult message)
        {
            if (message.RawData != "start")
            {
                return;
            }

            await message.ConfirmAction("Lets go");

            message.Handled = true;

            var st = new StartForm();

            await NavigateTo(st);
        }

        public override async Task Render(MessageResult message)
        {

            var bf = new ButtonForm();

            bf.AddButtonRow("Goto Start", "start");

            await Device.Send($"Welcome to Hidden open form\n\nYour guid is: {guid}", bf);


        }

    }
}
