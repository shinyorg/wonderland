#if IOS
using Shiny.Speech;

namespace ShinyWonderland;

[Singleton]
public class HeyWonderTask(
    AppSettings appsettings,
    ISpeechToTextService speechToTextService
) : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        appsettings.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsHeyWonderlandEnabled))
            {

            }
        };
        _ = this.HeyWonderland();
    }

    async Task HeyWonderland()
    {
        // TODO: cancellation token to stop listening when disabled
        // TODO: should also continue to loop listen after taking a command and handing it to AI
        // TODO: if the user presses the AI button, we should stop this listener somehow or just trigger the "hey wonderland".... ?
        if (appsettings.IsHeyWonderlandEnabled)
        {
            await speechToTextService.ListenWithWakeWord("Hey Wonderland");
        }
    }
}
#endif