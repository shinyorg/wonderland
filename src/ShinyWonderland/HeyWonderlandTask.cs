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
    public void Initialize(IServiceProvider services)
    {
        appsettings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsHeyWonderlandEnabled))
            {
                if (appsettings.IsHeyWonderlandEnabled)
                    _ = aiService.StartWakeWord("Hey Wonderland");
                else
                    aiService.StopWakeWord();
            }
        };

        if (appsettings.IsHeyWonderlandEnabled)
            _ = aiService.StartWakeWord("Hey Wonderland");
    }
}
#endif
