using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN")
               ?? throw new Exception("BOT_TOKEN environment variable is not set");
               
var bot = new TelegramBotClient(botToken);

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

bot.StartReceiving(
    updateHandler: HandleUpdateAsync,
    errorHandler: HandleErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await bot.GetMe();
Console.WriteLine($"Bot started: @{me.Username}");
Console.WriteLine("Press Enter to stop...");
Console.ReadLine();

cts.Cancel(); // stop the bot cleanly

// ─── Update Handler ────────────────────────────────────────────────────────────

async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    // User sends /start → show the menu
    if (update.Type == UpdateType.Message
        && update.Message?.Text == "/start")
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Button 1", "btn1"),
                InlineKeyboardButton.WithCallbackData("Button 2", "btn2"),
                InlineKeyboardButton.WithCallbackData("Button 3", "btn3"),
            }
        });

        await bot.SendMessage(
            chatId: update.Message.Chat.Id,
            text: "Choose an option:",
            replyMarkup: keyboard,
            cancellationToken: ct
        );

        return;
    }

    // User taps a button → handle the callback
    if (update.Type == UpdateType.CallbackQuery
        && update.CallbackQuery is { } query)
    {
        // Always answer the callback to remove the loading spinner
        await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);

        var responseText = query.Data switch
        {
            "btn1" => "text1",
            "btn2" => "text2",
            "btn3" => "text3",
            _      => "Unknown button"
        };

        await bot.SendMessage(
            chatId: query.Message!.Chat.Id,
            text: responseText,
            cancellationToken: ct
        );
    }
}

Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
{
    Console.WriteLine($"Error: {ex.Message}");
    return Task.CompletedTask;
}