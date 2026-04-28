namespace ShinyWonderland;

[ShellMap<AiLoadingPage>]
public partial class AiLoadingViewModel(INavigator navigator, IMediator mediator) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand]
    Task Cancel() => Task.CompletedTask;
    
    public void OnAppearing()
    {
    }

    public void OnDisappearing()
    {
    }
}


// public async Task RunAsync()
// {
//     cts = new CancellationTokenSource();
//     try
//     {
//         var service = IPlatformApplication.Current!.Services.GetRequiredService<AiChatService>();
//         await service.RunAsync(
//             status => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = status),
//             cts.Token
//         );
//     }
//     catch (OperationCanceledException)
//     {
//         // user dismissed — nothing to do
//     }
//     catch
//     {
//         try
//         {
//             var tts = IPlatformApplication.Current!.Services.GetRequiredService<ITextToSpeech>();
//             await tts.SpeakAsync("oh oh - something went wrong");
//         }
//         catch
//         {
//             // best effort
//         }
//     }
//     finally
//     {
//         cts?.Dispose();
//         cts = null;
//         await DismissAsync();
//     }
// }
//
// void OnCancelClicked(object? sender, EventArgs e)
//     => cts?.Cancel();
//
// protected override bool OnBackButtonPressed()
// {
//     cts?.Cancel();
//     return true;
// }
//
// async Task DismissAsync()
// {
//     if (Navigation.ModalStack.Contains(this))
//         await MainThread.InvokeOnMainThreadAsync(() => Navigation.PopModalAsync(true));
// }