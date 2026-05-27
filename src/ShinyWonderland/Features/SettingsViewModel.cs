using Shiny.Speech;

namespace ShinyWonderland.Features;


[ShellMap<SettingsPage>(registerRoute: false)]
public partial class SettingsViewModel : ObservableObject
{
    readonly AppSettings appSettings;
    readonly ITextToSpeechService tts;

    public SettingsViewModel(AppSettings appSettings, ITextToSpeechService tts)
    {
        this.appSettings = appSettings;
        this.tts = tts;

#if IOS
        this.IsHeyWonderlandEnabled = appSettings.IsHeyWonderlandEnabled;
#endif
        this.Ordering = appSettings.Ordering;
        this.ShowOpenOnly = appSettings.ShowOpenOnly;
        this.EnableTimeRideNotifications = appSettings.EnableTimeRideNotifications;
        this.EnableDrinkNotifications = appSettings.EnableDrinkNotifications;
        this.EnableMealNotifications = appSettings.EnableMealNotifications;
        this.ShowTimedOnly = appSettings.ShowTimedOnly;
        this.EnableGeofenceNotifications = appSettings.EnableGeofenceNotifications;
        this.SpeechRatePercent = appSettings.SpeechRatePercent;
        this.PitchPercent = appSettings.PitchPercent;
    }

    public string AppVersion => AppInfo.VersionString;

#if IOS
    [ObservableProperty] public partial bool IsHeyWonderlandEnabled { get; set; }
#endif
    [ObservableProperty] public partial RideOrder Ordering { get; set; }
    [ObservableProperty] public partial bool ShowOpenOnly { get; set; }
    [ObservableProperty] public partial bool EnableTimeRideNotifications { get; set; }
    [ObservableProperty] public partial bool EnableDrinkNotifications { get; set; }
    [ObservableProperty] public partial bool EnableMealNotifications { get; set; }
    [ObservableProperty] public partial bool ShowTimedOnly { get; set; }
    [ObservableProperty] public partial bool EnableGeofenceNotifications { get; set; }

    [ObservableProperty] public partial IReadOnlyList<VoiceInfo>? Voices { get; private set; }
    [ObservableProperty] public partial VoiceInfo? SelectedVoice { get; set; }
    [ObservableProperty] public partial int SpeechRatePercent { get; set; }
    [ObservableProperty] public partial int PitchPercent { get; set; }

    public async Task LoadVoices()
    {
        if (this.Voices != null)
            return;

        var voices = await this.tts.GetVoicesAsync();
        this.Voices = voices
            .OrderBy(v => v.Name)
            .ToList();

        if (!string.IsNullOrWhiteSpace(this.appSettings.VoiceId))
            this.SelectedVoice = this.Voices.FirstOrDefault(v => v.Id == this.appSettings.VoiceId);
    }

    [RelayCommand]
    async Task PlaySample()
    {
        await this.tts.SpeakAsync(
            "Welcome to Wonderland! This is how I will sound.",
            new TextToSpeechOptions
            {
                Voice = this.SelectedVoice,
                SpeechRate = this.SpeechRatePercent / 100f,
                Pitch = this.PitchPercent / 100f
            }
        );
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
#if IOS
            case nameof(IsHeyWonderlandEnabled):
                this.appSettings.IsHeyWonderlandEnabled = this.IsHeyWonderlandEnabled;
                break;
#endif
            case nameof(Ordering):
                this.appSettings.Ordering = this.Ordering;
                break;

            case nameof(ShowOpenOnly):
                this.appSettings.ShowOpenOnly = this.ShowOpenOnly;
                break;

            case nameof(ShowTimedOnly):
                this.appSettings.ShowTimedOnly = this.ShowTimedOnly;
                break;

            case nameof(EnableGeofenceNotifications):
                this.appSettings.EnableGeofenceNotifications = this.EnableGeofenceNotifications;
                break;

            case nameof(EnableMealNotifications):
                this.appSettings.EnableMealNotifications = this.EnableMealNotifications;
                break;

            case nameof(EnableDrinkNotifications):
                this.appSettings.EnableDrinkNotifications = this.EnableDrinkNotifications;
                break;

            case nameof(EnableTimeRideNotifications):
                this.appSettings.EnableTimeRideNotifications = this.EnableTimeRideNotifications;
                break;

            case nameof(SelectedVoice):
                this.appSettings.VoiceId = this.SelectedVoice?.Id;
                break;

            case nameof(SpeechRatePercent):
                this.appSettings.SpeechRatePercent = this.SpeechRatePercent;
                break;

            case nameof(PitchPercent):
                this.appSettings.PitchPercent = this.PitchPercent;
                break;
        }
        base.OnPropertyChanged(e);
    }
}
