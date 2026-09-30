using System.Reactive.Linq;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace ShinyWonderland.Features.Parking.Pages;


public partial class ParkingPage : ContentPage
{
    IDisposable? sub;
    
    public ParkingPage()
    {
        this.InitializeComponent();
    }


    protected override void OnBindingContextChanged()
    {
        if (this.BindingContext is ParkingViewModel vm)
        {
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(vm.CenterOfPark.Latitude, vm.CenterOfPark.Longitude),
                Microsoft.Maui.Maps.Distance.FromMeters(vm.MapStartZoomDistanceMeters)
            );
            this.ParkingMap.MoveToRegion(mapSpan);
        }

        base.OnBindingContextChanged();
    }


    // subscribed per appearance - disposing on disappear used to leave the pin frozen after switching tabs
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (this.BindingContext is not ParkingViewModel vm)
            return;

        this.SetPin(vm.ParkLocation);
        this.sub = vm
            .WhenAnyProperty()
            .Where(x => x.PropertyName == nameof(ParkingViewModel.ParkLocation))
            .Subscribe(_ => this.Dispatcher.Dispatch(() => this.SetPin(vm.ParkLocation)));
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.sub?.Dispose();
        this.sub = null;
    }


    void SetPin(Position? position)
    {
        this.ParkingMap.Pins.Clear();
        if (position == null)
            return;

        this.ParkingMap.Pins.Add(new Pin
        {
            Label = "YOU PARKED HERE",
            Type = PinType.SavedPin,
            Location = new Location(position.Latitude, position.Longitude)
        });
    }
}