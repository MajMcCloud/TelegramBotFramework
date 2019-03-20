
# .Net Telegram Bot Framework - Context based addon

[![NuGet version (TelegramBotBase)](https://img.shields.io/nuget/v/TelegramBotBase.svg?style=flat-square)](https://www.nuget.org/packages/TelegramBotBase/)
[![telegram chat](https://img.shields.io/badge/Support_Chat-Telegram-blue.svg?style=flat-square)](https://t.me/tgbotbase)


[![license](https://img.shields.io/github/license/MajMcCloud/telegrambotframework.svg?style=flat-square&maxAge=2592000&label=License)](https://raw.githubusercontent.com/MajMcCloud/TelegramBotFramework/master/LICENCE.md)
[![downloads](https://img.shields.io/nuget/dt/TelegramBotBase.svg?style=flat-square&label=Package%20Downloads)](https://www.nuget.org/packages/TelegramBotBase)

Test the Testproject: [@TGBaseBot](https://t.me/TGBaseBot)

---

## Index
- [Introduction](#introduction)
- [How to Start](#how-to-start)
- [Message Handling](#message-handling)
    * [Example #0 - System Calls](#add-some-system-calls-example-0---system-calls)

    * [Example #1 - Simple text messages](#lets-start-with-text-messages-example-1---simple-test)

    * [Example #2 - Button test](#now-some-buttons-example-2---button-test)

    * [Example #3 - Progress Bar control](#now-some-controls-example-3---progress-bar-test)

    * [Example #4 - Registration Formular](#registration-example-example-4---registration-form-test)

- [Special Forms](#forms)

	* [AlertDialog](#alert-dialog)
	* [AutoCleanForm](#autocleanform)
	* [PromptDialog](#prompt-dialog)


---

## Introduction

Hey guys,

here we are. After some time and thoughts i give my TelegramBot framework  to public.
It is based on C#.

It is a module which is based on the original [TelegramBotLibrary](https://github.com/TelegramBots/Telegram.Bot) you will find in nuget.

It gives you features which will look/feel like WinForms or have a good way to create apps with actions and forms.

---

## How to start:

Within your empty App your need to put some initial lines including your APIKey to get things started. 
The "BotBase" Class will manage a lot of things for you, like system calls, action events and so on. 
"StartForm" is your first Formular which every user will get internally redirected to, like a start page, you could redirect from there later in code, so users won't recognize it. 
It needs to be a subclass of "FormBase" you will find in Namespace TelegramBotBase.Form.


```

//Prepare the System
BotBase<StartForm> bb = new BotBase<StartForm>("{YOUR API KEY}");

//Add Systemcommands if you like, you could catch them later
bb.SystemCalls.Add("/start");

//Start your Bot
bb.Start();

```

Every Form has some events which will get raised at specific times. On every form you are able to get notes about the "Remote Device" like ChatId and other stuff your carrying. From there you build up you apps:

```
public class StartForm : FormBase
{


        public override async Task PreLoad(MessageResult message)
        {

        }

        public override async Task Init(params object[] param)
        {
            
        }

        public override async Task Opened()
        {

        }

        public override async Task Closed()
        {

        }

        public override async Task Load(MessageResult message)
        {
            await this.Device.Send("Hello world!");
        }



        public override async Task Action(MessageResult message)
        {


        }


        public override async Task Render(MessageResult message)
        {

        }

}

```

For instance send a message after loading a specific form:

```
await this.Device.Send("Hello world!");
```

Or you want to goto a different form?
Go ahead, create it, initialize it and navigate to it:

```
var tf = new TestForm();

await this.NavigateTo(tf);
```

## Message Handling

All examples are within the test project, so just try it out on your own.

### Add some system calls (Example #0 - System Calls)

Inside of the BotFather you are able to add "Commands" to your TelegramBot. The user will see them, depending on the application as options he could choose.
I'm calling them Systemcalls. Before start (and later for sure) you could add them to your BotBase. Every time a message comes in they will get checked if they are one of them.
If so, a special event Handler will get raised where you are easier able to manage the action behind.

Below we have 4 options.

/start - opens the Startformular

/form1 - navigates in this context to form1

/form2 - navigates in this context to form2

/params - demonstrates the use of parameters per command (i.e. /params 1 2 3 test ...)



```
BotBase<Start> bb = new BotBase<Start>("{YOUR API KEY}");

bb.SystemCalls.Add("/start");
bb.SystemCalls.Add("/form1");
bb.SystemCalls.Add("/form2");

bb.SystemCalls.Add("/params");

bb.SystemCall += async (s, en) =>
{
	switch (en.Command)
	{
	    case "/form1":

		var form1 = new TestForm();

		await en.Device.ActiveForm.NavigateTo(form1);

		break;

	    case "/form2":

		var form2 = new TestForm2();

		await en.Device.ActiveForm.NavigateTo(form2);

		break;

	    case "/params":

		String m = en.Parameters.DefaultIfEmpty("").Aggregate((a, b) => a + " and " + b);

		await en.Device.Send("Your parameters are " + m, replyTo: en.Device.LastMessage);

		break;
	}

};

bb.Start();

```


On every input the user is sending back to the bot the Action event gets raised. So here we could manage to send something back to him. For sure we could also manage different button inputs:

### Lets start with text messages (Example #1 - Simple Test)



```
public class SimpleForm : FormBase
{

	public override async Task Opened()
	{
	    await this.Device.Send("Hello world!");
	}


	public override async Task Load(MessageResult message)
	{
	    //message.MessageText will work also, cause it is a string you could manage a lot different scenerios here

	    var messageId = message.MessageId;

	    switch (message.Command)
	    {
		case "hello":
		case "hi":

		    //Send him a simple message
		    await this.Device.Send("Hello you there !");
		    break;

		case "maybe":

		    //Send him a simple message and reply to the one of himself
		    await this.Device.Send("Maybe what?", replyTo: messageId);

		    break;

		case "bye":
		case "ciao":

		    //Send him a simple message
		    await this.Device.Send("Ok, take care !");
		    break;
	    }
	}
}

```

### Now some buttons (Example #2 - Button Test)

I using a different base class (AutoCleanForm) I created for a better "feeling" inside the bot which will delete "old" messages from this form. You have some settings within this class to manage when messages should be getting deleted.


```
public class ButtonTestForm : AutoCleanForm
{

        public override async Task Opened()
        {
            await this.Device.Send("Hello world! (Click 'back' to get back to Start)");
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
                case "button1":

                    await this.Device.Send("Button 1 pressed");

                    break;

                case "button2":

                    await this.Device.Send("Button 2 pressed");

                    break;

                case "button3":

                    await this.Device.Send("Button 3 pressed");

                    break;

                case "button4":

                    await this.Device.Send("Button 4 pressed");

                    break;

                case "back":

                    var st = new Start();

                    await this.NavigateTo(st);

                    break;

                default:

                    message.Handled = false;

                    break;
            }


        }


        public override async Task Render(MessageResult message)
        {

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("Button 1", new CallbackData("a", "button1").Serialize()), new ButtonBase("Button 2", new CallbackData("a", "button2").Serialize()));

            btn.AddButtonRow(new ButtonBase("Button 3", new CallbackData("a", "button3").Serialize()), new ButtonBase("Button 4", new CallbackData("a", "button4").Serialize()));

            btn.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "back").Serialize()));

            await this.Device.Send("Click a button", btn);


        }
}

```

### Now some controls (Example #3 - Progress Bar Test)

Sometimes it makes sense to show the end user a type of progressbar or status. For this i tried to make a simple control, which is useful for some situations.
Maybe, if i  got more ideas, i will add other "controls" in the future.

```

public class ProgressTest : AutoCleanForm
{

public ProgressTest()
{
    this.DeleteMode = eDeleteMode.OnLeavingForm;
}

public override async Task Opened()
{
    await this.Device.Send("Welcome to ProgressTest");
}

public override async Task Action(MessageResult message)
{
    var call = message.GetData<CallbackData>();

    await message.ConfirmAction();


    if (call == null)
	return;

    TelegramBotBase.Controls.ProgressBar Bar = null;

    switch (call.Value)
    {
	case "standard":

	    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.standard);
	    Bar.Device = this.Device;

	    break;

	case "squares":

	    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.squares);
	    Bar.Device = this.Device;

	    break;

	case "circles":

	    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.circles);
	    Bar.Device = this.Device;

	    break;

	case "lines":

	    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.lines);
	    Bar.Device = this.Device;

	    break;

	case "squaredlines":

	    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.squaredLines);
	    Bar.Device = this.Device;

	    break;

	case "start":

	    var sf = new Start();

	    await sf.Init();

	    await this.NavigateTo(sf);

	    return;

	default:

	    return;

    }


    //Render Progress bar and show some "example" progress
    await Bar.Render();

    this.Controls.Add(Bar);

    for (int i = 0; i <= 100; i++)
    {
	Bar.Value++;
	await Bar.Render();

	Thread.Sleep(250);
    }


}


public override async Task Render(MessageResult message)
{
    ButtonForm btn = new ButtonForm();
    btn.AddButtonRow(new ButtonBase("Standard", new CallbackData("a", "standard").Serialize()), new ButtonBase("Squares", new CallbackData("a", "squares").Serialize()));

    btn.AddButtonRow(new ButtonBase("Circles", new CallbackData("a", "circles").Serialize()), new ButtonBase("Lines", new CallbackData("a", "lines").Serialize()));

    btn.AddButtonRow(new ButtonBase("Squared Line", new CallbackData("a", "squaredlines").Serialize()));

    btn.AddButtonRow(new ButtonBase("Back to start", new CallbackData("a", "start").Serialize()));

    await this.Device.Send("Choose your progress bar:", btn);
}

public override async Task Closed()
{
    foreach (var b in this.Controls)
    {
	await b.Cleanup();
    }

    await this.Device.Send("Ciao from ProgressTest");
}



}


```

### Registration Example (Example #4 - Registration Form Test)

I read it a lot in different Telegram groups that some developers are searching for easy solutions to create context based apps. For this is my project an ideal solution here.
To give you an example about the possiblities, i added into the Test project an example for a registration form.

```

public class PerForm : AutoCleanForm
{
public String EMail { get; set; }

public String Firstname { get; set; }

public String Lastname { get; set; }

public async override Task Load(MessageResult message)
{
    if (message.MessageText.Trim() == "")
	return;

    if (this.Firstname == null)
    {
	this.Firstname = message.MessageText;
	return;
    }

    if (this.Lastname == null)
    {
	this.Lastname = message.MessageText;
	return;
    }

    if (this.EMail == null)
    {
	this.EMail = message.MessageText;
	return;
    }

}

public async override Task Action(MessageResult message)
{
    var call = message.GetData<CallbackData>();

    await message.ConfirmAction();

    if (call == null)
	return;

    switch (call.Value)
    {
	case "back":

	    var start = new Start();

	    await this.NavigateTo(start);

	    break;

    }


}

public async override Task Render(MessageResult message)
{
    if (this.Firstname == null)
    {
	await this.Device.Send("Please sent your firstname:");
	return;
    }

    if (this.Lastname == null)
    {
	await this.Device.Send("Please sent your lastname:");
	return;
    }

    if (this.EMail == null)
    {
	await this.Device.Send("Please sent your email address:");
	return;
    }


    String s = "";

    s += "Firstname: " + this.Firstname + "\r\n";
    s += "Lastname: " + this.Lastname + "\r\n";
    s += "E-Mail: " + this.EMail + "\r\n";

    ButtonForm bf = new ButtonForm();
    bf.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "back").Serialize()));

    await this.Device.Send("Your details:\r\n" + s, bf);
}


}

```

There is also a second example, where every of these 3 inputs gets requested by a different formular (class). Just for imagination of the possiblites.
Cause its to much Text, i didnt have added it here. You will find it under [TelegramBaseTest/Tests/Register/PerStep.cs](TelegramBaseTest/Tests/Register/PerStep.cs)
Beginn there and navigate your way through these Steps in the subfolder.


---


## Forms

There are some default types of forms to make the interaction with users easier.
For now we have the following:

- [AlertDialog](#alert-dialog)
	Just a simple dialog with one Button.

- [AutoCleanForm](#autocleanform)
	A form which needs to be derived from. It will be delete all in the context sent messages to the user after every new message or on leaving the formular and navigates somewhere else.
	Makes sense to create a "feeling" of a clean environment for the user. For instance if you have a multilevel menu. This will remove the previously shown menu, and renders the new sub/top level.

- [PromptDialog](#prompt-dialog)
	A simple dialog which is able to show multiple buttons and a Text message. The user could select one option and will get redirected to a different form, depending on the click.

### Alert Dialog

```

var fto = new TestForm2();

AlertDialog ad = new AlertDialog("This is a message", "Ok", fto);

await this.NavigateTo(ad);

```


### AutoCleanForm



### Prompt Dialog

#### Without Eventhandler (pre-init of form necessary)

```

PromptDialog pd = new PromptDialog("Please confirm", new ButtonBase("Ok", "ok"), new ButtonBase("Cancel", "cancel"));

var tf = new TestForm2();

pd.ButtonForms.Add("ok", tf);
pd.ButtonForms.Add("cancel", tf);

await this.NavigateTo(pd);

```


#### With Eventhandler (no pre-init of form necessary)

```

PromptDialog pd = new PromptDialog("Please confirm", new ButtonBase("Ok", "ok"), new ButtonBase("Cancel", "cancel"));

//You could mix here for sure.
pd.ButtonForms.Add("ok", null);
pd.ButtonForms.Add("cancel", null);

pd.ButtonClicked += async (s, en) =>
{
    var tf = new TestForm2();

    //Remember only to navigate from the current running form. (here it is the prompt dialog, cause we have left the above already)
    await pd.NavigateTo(tf);
};

await this.NavigateTo(pd);

```

---

I will add more notes to it soon, so stay tuned.

Warm regards

Florian Dahn

