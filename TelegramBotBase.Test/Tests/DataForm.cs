using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests
{
    public class DataForm : AutoCleanForm
    {


        public override async Task SentData(DataResult data)
        {
            String tmp = "";
            InputOnlineFile file;

            switch (data.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Contact:

                    tmp += "Firstname: " + data.Contact.FirstName + "\r\n";
                    tmp += "Lastname: " + data.Contact.LastName + "\r\n";
                    tmp += "Phonenumber: " + data.Contact.PhoneNumber + "\r\n";
                    tmp += "UserId: " + data.Contact.UserId + "\r\n";

                    await this.Device.Send("Your contact: \r\n" + tmp, replyTo: data.MessageId);

                    break;

                case Telegram.Bot.Types.Enums.MessageType.Document:

                    file = new InputOnlineFile(data.Document.FileId);

                    await this.Device.SendDocument(file, "Your uploaded document");


                    break;

                case Telegram.Bot.Types.Enums.MessageType.Video:

                    file = new InputOnlineFile(data.Document.FileId);

                    await this.Device.SendDocument(file, "Your uploaded video");

                    break;

                case Telegram.Bot.Types.Enums.MessageType.Audio:

                    file = new InputOnlineFile(data.Document.FileId);

                    await this.Device.SendDocument(file, "Your uploaded audio");

                    break;

                case Telegram.Bot.Types.Enums.MessageType.Location:

                    tmp += "Lat: " + data.Location.Latitude + "\r\n";
                    tmp += "Lng: " + data.Location.Longitude + "\r\n";

                    await this.Device.Send("Your location: \r\n" + tmp, replyTo: data.MessageId);

                    break;

                case Telegram.Bot.Types.Enums.MessageType.Photo:

                    InputOnlineFile photo = new InputOnlineFile(data.Photos.Last().FileId);

                    await this.Device.Send("Your image: ", replyTo: data.MessageId);
                    await this.Client.TelegramClient.SendPhotoAsync(this.Device.DeviceId, photo);

                    break;

                default:

                    await this.Device.Send("Unknown response");

                    break;
            }

        }

        public override async Task Action(MessageResult message)
        {
            await message.ConfirmAction();

            if (message.Handled)
                return;

            switch (message.RawData)
            {
                case "contact":

                    await this.Device.RequestContact();

                    break;

                case "location":

                    await this.Device.RequestLocation();

                    break;

                case "back":

                    message.Handled = true;

                    var start = new Menu();

                    await this.NavigateTo(start);

                    break;
            }


        }

        public override async Task Render(MessageResult message)
        {
            ButtonForm bf = new ButtonForm();

            bf.AddButtonRow(new ButtonBase("Request User contact", "contact"));

            bf.AddButtonRow(new ButtonBase("Request User location", "location"));

            bf.AddButtonRow(new ButtonBase("Back", "back"));

            InlineKeyboardMarkup ikv = bf;

            await this.Device.Send("Please upload a contact, photo, video, audio, document or location.", bf);



        }

    }
}
