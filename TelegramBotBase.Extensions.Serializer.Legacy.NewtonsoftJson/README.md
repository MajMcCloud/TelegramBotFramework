# TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson

[![NuGet version (TelegramBotBase)](https://img.shields.io/nuget/v/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson.svg?style=flat-square)](https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/)
[![Telegram chat](https://img.shields.io/badge/Support_Chat-Telegram-blue.svg?style=flat-square)](https://www.t.me/tgbotbase)

[![License](https://img.shields.io/github/license/MajMcCloud/telegrambotframework.svg?style=flat-square&maxAge=2592000&label=License)](https://raw.githubusercontent.com/MajMcCloud/TelegramBotFramework/master/LICENCE.md)
[![Package Downloads](https://img.shields.io/nuget/dt/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson.svg?style=flat-square&label=Package%20Downloads)](https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson)


### Legacy version to recover with old dependencies using Newtonsoft.Json for session serialization


## How to use

```csharp
using TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson;


var bot = BotBaseBuilder
            .Create()
            .WithAPIKey(APIKey)
            .DefaultMessageLoop()
            .WithStartForm<Start>()
            .NoProxy()
            .OnlyStart()
            .UseNewtonsoftJson()
            .UseEnglish()
            .Build();

bot.Start();
```
