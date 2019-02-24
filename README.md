
[![NuGet version (TelegramBotBase)](https://img.shields.io/nuget/v/TelegramBotBase.svg?style=flat-square)](https://www.nuget.org/packages/TelegramBotBase/)

Hey guys,

here we are. After some time and thoughts i give my TelegramBot framework  to public.
It is based on C#.

It is a module which is based on the original TelegramBotLibrary you will find in nuget.

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

await tf.Init();

await this.NavigateTo(tf);
```

## Message Handling

All examples are within the test project, so just try it out on your own.

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

                    await st.Init();

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

                    await Bar.Render();

                    this.Controls.Add(Bar);

                    for (int i = 0; i <= 100; i++)
                    {
                        Bar.Value++;
                        await Bar.Render();

                        Thread.Sleep(250);
                    }

                    break;

                case "squares":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.squares);
                    Bar.Device = this.Device;

                    await Bar.Render();

                    this.Controls.Add(Bar);

                    for (int i = 0; i <= 100; i++)
                    {
                        Bar.Value++;
                        await Bar.Render();

                        Thread.Sleep(250);
                    }

                    break;

                case "circles":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.circles);
                    Bar.Device = this.Device;

                    await Bar.Render();

                    this.Controls.Add(Bar);

                    for (int i = 0; i <= 100; i++)
                    {
                        Bar.Value++;
                        await Bar.Render();

                        Thread.Sleep(250);
                    }

                    break;

                case "lines":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.lines);
                    Bar.Device = this.Device;

                    await Bar.Render();

                    this.Controls.Add(Bar);

                    for (int i = 0; i <= 100; i++)
                    {
                        Bar.Value++;
                        await Bar.Render();

                        Thread.Sleep(250);
                    }

                    break;

                case "squaredlines":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.squaredLines);
                    Bar.Device = this.Device;

                    await Bar.Render();

                    this.Controls.Add(Bar);

                    for (int i = 0; i <= 100; i++)
                    {
                        Bar.Value++;
                        await Bar.Render();

                        Thread.Sleep(250);
                    }


                    break;

                case "start":

                    var sf = new Start();

                    await sf.Init();

                    await this.NavigateTo(sf);

                    break;

                default:



                    break;
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
            foreach(var b in this.Controls)
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


---

I will add more notes to it soon, so stay tuned.

Warm regards

Florian Dahn

