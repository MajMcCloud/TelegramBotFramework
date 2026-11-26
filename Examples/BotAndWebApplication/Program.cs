using BotAndWebApplication.BotStuff;
using BotAndWebApplication.BotStuff.Tasks;
using TelegramBotBase.Builder;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        var botBuilder = BotBaseBuilder.Create(builder.Configuration["API_KEY"] ??
                                               throw new InvalidOperationException("API_KEY is not set."))
                                       .WithStartForm<StartForm>()
                                       .DefaultCommands()
                                       .UseEnglish();

        var webhookUrl = builder.Configuration["Telegram:WebhookUrl"];
        var webhookSecret = builder.Configuration["Telegram:WebhookSecret"];
        var useWebhook = webhookUrl != null || webhookSecret != null;
        if (useWebhook)
        {
            botBuilder.WithWebhook(webhookUrl ?? throw new InvalidOperationException("Telegram:WebhookUrl is not set."),
                                   webhookSecret ?? throw new InvalidOperationException("Telegram:WebhookSecret is not set."));
            builder.Services.AddSingleton<TelegramWebhook>();
        }
        else
        {
            botBuilder.UseThreadPool();
        }

        builder.Services.AddSingleton(botBuilder.Build());
        builder.Services.AddHostedService<BotBackgroundTask>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        if (useWebhook)
        {
            app.MapPost(TelegramWebhook.Path, (HttpRequest request, TelegramWebhook webhook) => webhook.HandleAsync(request));
        }

        app.Run();
    }
}
