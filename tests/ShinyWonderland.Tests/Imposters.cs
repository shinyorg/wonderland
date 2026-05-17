using Microsoft.Maui.Media;
using Shiny.DocumentDb;
using Shiny.Speech;

[assembly: GenerateImposter(typeof(INotificationManager))]
[assembly: GenerateImposter(typeof(IGpsManager))]
[assembly: GenerateImposter(typeof(IDialogs))]
[assembly: GenerateImposter(typeof(IMediaPicker))]
[assembly: GenerateImposter(typeof(ILogger<>))]
[assembly: GenerateImposter(typeof(ITextToSpeechService))]
