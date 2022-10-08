using EFCoreBot.Database;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace EFCoreBot;

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

        await Device.Send($"Your last message's text was: `{user.LastMessage}`");

        user.LastMessage = string.IsNullOrWhiteSpace(message.MessageText) ? "<unknown>" : message.MessageText;
        await _dbContext.SaveChangesAsync();
    }
}