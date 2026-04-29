namespace ShinyWonderland.Features.AI.Pages;

public partial class AiLoadingPage : ContentPage
{
    AiPhase currentPhase = AiPhase.Idle;
    bool isAnimating;
    BoxView[] barsArr = null!;
    BoxView[] thinkDotsArr = null!;
    Border[] ripplesArr = null!;

    public AiLoadingPage()
    {
        this.InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        isAnimating = true;

        barsArr = [Bar1, Bar2, Bar3, Bar4, Bar5, Bar6, Bar7];
        var particlesArr = new[] { Particle1, Particle2, Particle3, Particle4, Particle5, Particle6 };
        thinkDotsArr = [ThinkDot1, ThinkDot2, ThinkDot3];
        ripplesArr = [Ripple1, Ripple2, Ripple3];

        if (BindingContext is AiLoadingViewModel vm)
            vm.PhaseChanged += OnPhaseChanged;

        StartAmbientAnimations(particlesArr);
        StartOrbBreathing();
    }

    void OnPhaseChanged(AiPhase phase)
        => TransitionToPhase(phase);

    void TransitionToPhase(AiPhase newPhase)
    {
        if (!isAnimating) return;
        var oldPhase = currentPhase;
        currentPhase = newPhase;

        StopPhaseAnimations(oldPhase);

        switch (newPhase)
        {
            case AiPhase.Prompting:
                StatusLabel.Text = "Getting ready...";
                SubtitleLabel.Text = "Preparing your assistant";
                PhaseIcon.Text = "\u2728";
                WaveformBars.FadeToAsync(0, 200);
                ThinkingDots.FadeToAsync(0, 200);
                OrbitalRing.FadeToAsync(0, 200);
                StartPromptingAnimation();
                break;

            case AiPhase.Listening:
                StatusLabel.Text = "Listening...";
                SubtitleLabel.Text = "Speak now";
                PhaseIcon.Text = "\U0001F399";
                ThinkingDots.FadeToAsync(0, 200);
                OrbitalRing.FadeToAsync(0, 200);
                WaveformBars.FadeToAsync(1, 300);
                StartWaveformAnimation();
                StartListeningOrbPulse();
                break;

            case AiPhase.Thinking:
                StatusLabel.Text = "Thinking...";
                SubtitleLabel.Text = "Processing your request";
                PhaseIcon.Text = "\U0001F9E0";
                WaveformBars.FadeToAsync(0, 200);
                OrbitalRing.FadeToAsync(1, 400);
                ThinkingDots.FadeToAsync(1, 300);
                StartOrbitalAnimation();
                StartThinkingDotsAnimation();
                StartThinkingOrbPulse();
                break;

            case AiPhase.Speaking:
                StatusLabel.Text = "Speaking...";
                SubtitleLabel.Text = "Listen to the response";
                PhaseIcon.Text = "\U0001F50A";
                WaveformBars.FadeToAsync(0, 200);
                OrbitalRing.FadeToAsync(0, 300);
                ThinkingDots.FadeToAsync(0, 200);
                StartRippleAnimation();
                StartSpeakingOrbPulse();
                break;
        }
    }

    void StopPhaseAnimations(AiPhase phase)
    {
        switch (phase)
        {
            case AiPhase.Prompting:
                this.AbortAnimation("PromptPulse");
                break;

            case AiPhase.Listening:
                for (var i = 0; i < barsArr.Length; i++)
                    this.AbortAnimation($"Bar{i}");
                this.AbortAnimation("ListenOrb");
                break;

            case AiPhase.Thinking:
                this.AbortAnimation("OrbitalSpin");
                for (var i = 0; i < thinkDotsArr.Length; i++)
                    this.AbortAnimation($"ThinkDot{i}");
                this.AbortAnimation("ThinkOrb");
                break;

            case AiPhase.Speaking:
                for (var i = 0; i < ripplesArr.Length; i++)
                    this.AbortAnimation($"Ripple{i}");
                this.AbortAnimation("SpeakOrb");
                break;
        }
    }

    // --- Ambient (always running) ---

    void StartAmbientAnimations(BoxView[] particlesArr)
    {
        var random = new Random();
        for (var i = 0; i < particlesArr.Length; i++)
        {
            var p = particlesArr[i];
            var dx = random.Next(-30, 30);
            var dy = random.Next(-40, 40);
            var duration = (uint)(3000 + random.Next(2000));
            StartParticleDrift(p, $"Particle{i}", dx, dy, duration);
        }
    }

    void StartParticleDrift(BoxView particle, string name, double dx, double dy, uint duration)
    {
        var startX = particle.TranslationX;
        var startY = particle.TranslationY;

        var moveOut = new Animation(v =>
        {
            particle.TranslationX = startX + dx * v;
            particle.TranslationY = startY + dy * v;
            particle.Opacity = 0.05 + 0.15 * v;
        }, 0, 1);
        var moveBack = new Animation(v =>
        {
            particle.TranslationX = startX + dx * (1 - v);
            particle.TranslationY = startY + dy * (1 - v);
            particle.Opacity = 0.2 - 0.15 * v;
        }, 0, 1);

        var full = new Animation();
        full.Add(0, 0.5, moveOut);
        full.Add(0.5, 1.0, moveBack);
        full.Commit(this, name, length: duration, repeat: () => isAnimating, easing: Easing.SinInOut);
    }

    void StartOrbBreathing()
    {
        var pulse = new Animation(v => InnerOrb.Scale = v, 1.0, 1.06);
        var ret = new Animation(v => InnerOrb.Scale = v, 1.06, 1.0);
        var full = new Animation();
        full.Add(0, 0.5, pulse);
        full.Add(0.5, 1.0, ret);
        full.Commit(this, "OrbBreathe", length: 3000, repeat: () => isAnimating, easing: Easing.SinInOut);

        var outerUp = new Animation(v => OuterRing.Scale = v, 1.0, 1.08);
        var outerDown = new Animation(v => OuterRing.Scale = v, 1.08, 1.0);
        var outerFull = new Animation();
        outerFull.Add(0, 0.5, outerUp);
        outerFull.Add(0.5, 1.0, outerDown);
        outerFull.Commit(this, "OuterBreathe", length: 4000, repeat: () => isAnimating, easing: Easing.SinInOut);

        var midUp = new Animation(v => MiddleRing.Scale = v, 1.0, 1.05);
        var midDown = new Animation(v => MiddleRing.Scale = v, 1.05, 1.0);
        var midFull = new Animation();
        midFull.Add(0, 0.5, midUp);
        midFull.Add(0.5, 1.0, midDown);
        midFull.Commit(this, "MiddleBreathe", length: 2800, repeat: () => isAnimating, easing: Easing.SinInOut);
    }

    // --- Prompting phase ---

    void StartPromptingAnimation()
    {
        var fade = new Animation(v => InnerOrb.Opacity = v, 1.0, 0.7);
        var fadeBack = new Animation(v => InnerOrb.Opacity = v, 0.7, 1.0);
        var full = new Animation();
        full.Add(0, 0.5, fade);
        full.Add(0.5, 1.0, fadeBack);
        full.Commit(this, "PromptPulse", length: 1500, repeat: () => isAnimating && currentPhase == AiPhase.Prompting,
            easing: Easing.SinInOut);
    }

    // --- Listening phase ---

    void StartWaveformAnimation()
    {
        var random = new Random(42);
        double[] maxHeights = [24, 32, 20, 36, 28, 22, 30];

        for (var i = 0; i < barsArr.Length; i++)
        {
            var bar = barsArr[i];
            var maxH = maxHeights[i];
            var speed = (uint)(400 + random.Next(300));
            var idx = i;

            var grow = new Animation(v => bar.HeightRequest = 8 + (maxH - 8) * v, 0, 1);
            var shrink = new Animation(v => bar.HeightRequest = maxH - (maxH - 8) * v, 0, 1);
            var full = new Animation();
            full.Add(0, 0.5, grow);
            full.Add(0.5, 1.0, shrink);
            full.Commit(this, $"Bar{idx}", length: speed,
                repeat: () => isAnimating && currentPhase == AiPhase.Listening,
                easing: Easing.SinInOut);
        }
    }

    void StartListeningOrbPulse()
    {
        var scale = new Animation(v => InnerOrb.Scale = v, 1.0, 1.12);
        var scaleBack = new Animation(v => InnerOrb.Scale = v, 1.12, 1.0);
        var full = new Animation();
        full.Add(0, 0.5, scale);
        full.Add(0.5, 1.0, scaleBack);
        full.Commit(this, "ListenOrb", length: 800,
            repeat: () => isAnimating && currentPhase == AiPhase.Listening,
            easing: Easing.SinInOut);
    }

    // --- Thinking phase ---

    void StartOrbitalAnimation()
    {
        var spin = new Animation(v => OrbitalRing.Rotation = 360 * v, 0, 1);
        spin.Commit(this, "OrbitalSpin", length: 2000,
            repeat: () => isAnimating && currentPhase == AiPhase.Thinking,
            easing: Easing.Linear);
    }

    void StartThinkingDotsAnimation()
    {
        for (var i = 0; i < thinkDotsArr.Length; i++)
        {
            var dot = thinkDotsArr[i];
            var idx = i;

            var fadeIn = new Animation(v => dot.Opacity = v, 0.3, 1.0);
            var fadeOut = new Animation(v => dot.Opacity = v, 1.0, 0.3);
            var scaleUp = new Animation(v => dot.Scale = v, 0.8, 1.3);
            var scaleDown = new Animation(v => dot.Scale = v, 1.3, 0.8);

            var full = new Animation();
            full.Add(0, 0.4, fadeIn);
            full.Add(0, 0.4, scaleUp);
            full.Add(0.5, 0.9, fadeOut);
            full.Add(0.5, 0.9, scaleDown);
            full.Commit(this, $"ThinkDot{idx}", length: 1200,
                repeat: () => isAnimating && currentPhase == AiPhase.Thinking);

            if (i > 0)
                dot.Scale = 0.8;
        }
    }

    void StartThinkingOrbPulse()
    {
        var scale = new Animation(v => InnerOrb.Scale = v, 1.0, 1.08);
        var scaleBack = new Animation(v => InnerOrb.Scale = v, 1.08, 1.0);
        var full = new Animation();
        full.Add(0, 0.5, scale);
        full.Add(0.5, 1.0, scaleBack);
        full.Commit(this, "ThinkOrb", length: 1600,
            repeat: () => isAnimating && currentPhase == AiPhase.Thinking,
            easing: Easing.SinInOut);
    }

    // --- Speaking phase ---

    void StartRippleAnimation()
    {
        for (var i = 0; i < ripplesArr.Length; i++)
        {
            var ripple = ripplesArr[i];
            var idx = i;
            var delayFraction = i * 0.33;

            var expand = new Animation(v =>
            {
                ripple.Scale = 1.0 + 1.5 * v;
                ripple.Opacity = 0.6 * (1.0 - v);
            }, 0, 1);

            var reset = new Animation(v =>
            {
                ripple.Scale = 1.0;
                ripple.Opacity = 0;
            }, 0, 1);

            var full = new Animation();
            var start = delayFraction * 0.3;
            full.Add(start, start + 0.7, expand);
            full.Add(start + 0.7, Math.Min(start + 0.75, 1.0), reset);
            full.Commit(this, $"Ripple{idx}", length: 2400,
                repeat: () => isAnimating && currentPhase == AiPhase.Speaking,
                easing: Easing.CubicOut);
        }
    }

    void StartSpeakingOrbPulse()
    {
        var scale = new Animation(v => InnerOrb.Scale = v, 1.0, 1.15);
        var scaleBack = new Animation(v => InnerOrb.Scale = v, 1.15, 1.0);
        var glow = new Animation(v => InnerOrb.Opacity = v, 1.0, 0.85);
        var glowBack = new Animation(v => InnerOrb.Opacity = v, 0.85, 1.0);

        var full = new Animation();
        full.Add(0, 0.5, scale);
        full.Add(0, 0.5, glow);
        full.Add(0.5, 1.0, scaleBack);
        full.Add(0.5, 1.0, glowBack);
        full.Commit(this, "SpeakOrb", length: 1000,
            repeat: () => isAnimating && currentPhase == AiPhase.Speaking,
            easing: Easing.SinInOut);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        isAnimating = false;

        if (BindingContext is AiLoadingViewModel vm)
            vm.PhaseChanged -= OnPhaseChanged;

        this.AbortAnimation("OrbBreathe");
        this.AbortAnimation("OuterBreathe");
        this.AbortAnimation("MiddleBreathe");
        this.AbortAnimation("PromptPulse");
        this.AbortAnimation("ListenOrb");
        this.AbortAnimation("ThinkOrb");
        this.AbortAnimation("SpeakOrb");
        this.AbortAnimation("OrbitalSpin");

        for (var i = 0; i < 7; i++)
            this.AbortAnimation($"Bar{i}");
        for (var i = 0; i < 6; i++)
            this.AbortAnimation($"Particle{i}");
        for (var i = 0; i < 3; i++)
        {
            this.AbortAnimation($"ThinkDot{i}");
            this.AbortAnimation($"Ripple{i}");
        }
    }
}
