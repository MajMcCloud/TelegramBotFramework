# TelegramBotBase.Extensions.Serializer.Database.MSSQL

[![NuGet version (TelegramBotBase)](https://img.shields.io/nuget/v/TelegramBotBase.Extensions.Serializer.Database.MSSQL.svg?style=flat-square)](https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Database.MSSQL/)
[![telegram chat](https://img.shields.io/badge/Support_Chat-Telegram-blue.svg?style=flat-square)](https://www.t.me/tgbotbase)


[![license](https://img.shields.io/github/license/MajMcCloud/telegrambotframework.svg?style=flat-square&maxAge=2592000&label=License)](https://raw.githubusercontent.com/MajMcCloud/TelegramBotFramework/master/LICENCE.md)
[![downloads](https://img.shields.io/nuget/dt/TelegramBotBase.Extensions.Serializer.Database.MSSQL.svg?style=flat-square&label=Package%20Downloads)](https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Database.MSSQL)

## How to use

```
using TelegramBotBase.Extensions.Serializer.Database.MSSQL;


var bb = BotBaseBuilder
            .Create()
            .WithAPIKey(APIKey)
            .DefaultMessageLoop()
            .WithStartForm<Start>()
            .NoProxy()
            .OnlyStart()
            .UseSQLDatabase("localhost", "telegram_bot")
            .UseEnglish()
            .Build();

bb.Start();

```