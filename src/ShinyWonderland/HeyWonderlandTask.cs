#if IOS
using Shiny.AiConversation;

namespace ShinyWonderland;

[Singleton]
public class HeyWonderTask(
    AppSettings appsettings,
    IAiConversationService aiService,
    ILogger<HeyWonderTask> logger
) : IMauiInitializeService
{
    public async void Initialize(IServiceProvider services)
    {
        appsettings.PropertyChanged += async (_, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsHeyWonderlandEnabled))
            {
                if (appsettings.IsHeyWonderlandEnabled)
                    await aiService.StartWakeWord("Hey Wonderland");
                else
                    aiService.StopWakeWord();
            }
        };

        if (appsettings.IsHeyWonderlandEnabled)
            await aiService.StartWakeWord("Hey Wonderland");
    }
}
#endif
