# Action Manager
## Handling unhandled calls

The Action Manager is a class that helps you to handle unhandled calls. This is useful when you have a command that is not recognized or when you want to handle a specific message that is not a command.



### How to use it

To use the Action Manager, you need to create an instance of it and then add the actions you want to handle. The actions are added using the `AddAction` method, which receives a `Func<Update, Task>` as a parameter. This function will be called when the action is triggered.
```csharp

var eam = ExternalActionManager.Configure(a =>
        {
            //Handling all callback data values where method is "test" and value is a int32
            //You need to use a serialized CallbackData object for this
            a.AddInt32Action("test", async (value, cd, ur, mr) =>
            {
                await mr.Device.Send($"This is a test action. Value is {value}");
            });

            //Handling all callback data values where method is "test2" and value is a guid
            //You need to use a serialized CallbackData object for this
            a.AddGuidAction("test2", async (value, cd, ur, mr) =>
            {
                await mr.Device.Send($"This is a test action. Value is {value}");
            });

            //Handle all inline values starting with "test3"
            //No need to use a serialized CallbackData object for this
            a.AddStartsWithAction("test3", HandleStartsWith);
        });
```

After adding the actions, you need to call the `HandleUpdate` method, passing the update you want to handle. The method will return a boolean indicating if the update was handled or not.
```csharp

//Add it into your bot startup routine

var bot = BotBaseBuilder
                 .Create()
                 .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ??
                             throw new Exception("API_KEY is not set"))
                 .DefaultMessageLoop(eam)
                 .WithStartForm<Start>()
                 .NoProxy()
                 .CustomCommands(a =>
                 {
                     a.Start("Starts the bot");
                 })
                 .NoSerialization()
                 .UseEnglish()
                 .UseThreadPool()
                 .Build();


                 await bot.Start();

```

			

### How it works

The Action Manager will check all the actions you added in the order they were added. If an action returns true, the update is considered handled, and the next actions are not checked. If no action returns true, the update is considered unhandled, and the bot will handle it as usual.

### Adding actions

You can add actions using the following methods:

- `AddAction(Func<Update, Task>)`: Adds an action that will be triggered for any update.
- `AddInt32Action(string, Func<int, CallbackData, User, MessageRequest, Task>)`: Adds an action that will be triggered when the callback data method fits the string and the value is an integer.
- `AddGuidAction(string, Func<Guid, CallbackData, User, MessageRequest, Task>)`: Adds an action that will be triggered when the callback data method fits the string and the value is a GUID.
- `AddStartsWithAction(string, Func<string, CallbackData, User, MessageRequest, Task>)`: Adds an action that will be triggered when the value starts with a specific string.


### Send messages
This will send a message to the user with 3 different buttons, regarding the setup from above.
```csharp
//Send message via Telegram Bot API directly
long deviceId = 1;

var bf = new ButtonForm();

//Create a serialized CallbackData object
var cd_int32 = new CallbackData("test", "123").Serialize();
bf.AddButtonRow(new ButtonBase("1. Test", cd_int32));

//Create a serialized CallbackData object
var cd_guid = new CallbackData("test2", Guid.NewGuid().ToString()).Serialize();
bf.AddButtonRow(new ButtonBase("2. Test", cd_guid));

//Add the raw string value
bf.AddButtonRow(new ButtonBase("3. Test", "test3"));

await bot.Client.TelegramClient.SendTextMessageAsync(deviceId, "This is an example message", replyMarkup: bf);
```