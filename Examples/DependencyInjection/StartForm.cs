using DependencyInjection.Database;
using DependencyInjection.Forms;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.DependencyInjection;

namespace DependencyInjection;

public class StartForm : FormBase
{
    private readonly BotDbContext _dbContext;

    public StartForm(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task Load(MessageResult message)
    {
        var user = await _dbContext.Users.FindAsync(Device.DeviceId);
        if (user is null)
        {
            user = new User
            {
                Id = Device.DeviceId,
                LastMessage = "<unknown>"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        if (message.IsAction)
            return;


        user.LastMessage = string.IsNullOrWhiteSpace(message.MessageText) ? "<unknown>" : message.MessageText;
        await _dbContext.SaveChangesAsync();
    }

    public override async Task Action(MessageResult message)
    {
        await message.ConfirmAction("Ok");

        switch(message.RawData)
        {

            case "open":

                await this.NavigateTo(typeof(ConfirmationForm));

                var new_form = await this.NavigateTo<ConfirmationForm>();

                if (new_form == null)
                {
                    await Device.Send("Cant open ConfirmationForm");
                }

                break;

        }


    }

    public override async Task Render(MessageResult message)
    {
        var user = await _dbContext.Users.FindAsync(Device.DeviceId);
        if (user == null)
            return;

        var bf = new ButtonForm();

        bf.AddButtonRow("Open confirmation", "open");

        await Device.Send($"Your last message's text was: `{user.LastMessage}`", bf);
    }

}
