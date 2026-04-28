namespace ShinyWonderland.Features.AI.Pages;

public partial class AiLoadingPage : ContentPage
{
    public AiLoadingPage()
    {
        this.InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartPulseAnimations();
        StartDotAnimations();
    }

    void StartPulseAnimations()
    {
        // Outer ring breathes slowly
        var outerPulse = new Animation(v => OuterRing.Scale = v, 1.0, 1.15);
        var outerReturn = new Animation(v => OuterRing.Scale = v, 1.15, 1.0);
        var outerFull = new Animation();
        outerFull.Add(0, 0.5, outerPulse);
        outerFull.Add(0.5, 1.0, outerReturn);
        outerFull.Commit(this, "OuterPulse", length: 2500, repeat: () => true);

        // Middle ring breathes offset
        var midPulse = new Animation(v => MiddleRing.Scale = v, 1.0, 1.1);
        var midReturn = new Animation(v => MiddleRing.Scale = v, 1.1, 1.0);
        var midFull = new Animation();
        midFull.Add(0, 0.5, midPulse);
        midFull.Add(0.5, 1.0, midReturn);
        midFull.Commit(this, "MiddlePulse", length: 1800, repeat: () => true);

        // Inner orb gentle glow pulse
        var orbPulse = new Animation(v => InnerOrb.Opacity = v, 1.0, 0.7);
        var orbReturn = new Animation(v => InnerOrb.Opacity = v, 0.7, 1.0);
        var orbFull = new Animation();
        orbFull.Add(0, 0.5, orbPulse);
        orbFull.Add(0.5, 1.0, orbReturn);
        orbFull.Commit(this, "OrbGlow", length: 2000, repeat: () => true, easing: Easing.SinInOut);
    }

    async void StartDotAnimations()
    {
        AnimateDot(Dot1, "Dot1");
        await Task.Delay(200);
        AnimateDot(Dot2, "Dot2");
        await Task.Delay(200);
        AnimateDot(Dot3, "Dot3");
    }

    void AnimateDot(BoxView dot, string name)
    {
        var fadeIn = new Animation(v => dot.Opacity = v, 0.3, 1.0);
        var fadeOut = new Animation(v => dot.Opacity = v, 1.0, 0.3);
        var scaleUp = new Animation(v => dot.Scale = v, 1.0, 1.4);
        var scaleDown = new Animation(v => dot.Scale = v, 1.4, 1.0);

        var full = new Animation();
        full.Add(0, 0.4, fadeIn);
        full.Add(0, 0.4, scaleUp);
        full.Add(0.5, 0.9, fadeOut);
        full.Add(0.5, 0.9, scaleDown);
        full.Commit(this, name, length: 1200, repeat: () => true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.AbortAnimation("OuterPulse");
        this.AbortAnimation("MiddlePulse");
        this.AbortAnimation("OrbGlow");
        this.AbortAnimation("Dot1");
        this.AbortAnimation("Dot2");
        this.AbortAnimation("Dot3");
    }
}
