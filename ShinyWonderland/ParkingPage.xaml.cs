using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace ShinyWonderland;


public partial class ParkingPage : ContentPage
{
    IDisposable sub;
    
    public ParkingPage()
    {
        this.InitializeComponent();
    }


    protected override void OnBindingContextChanged()
    {
        var vm = (ParkingViewModel)this.BindingContext;
        if (vm.ParkLocation != null)
            this.SetPin(vm.ParkLocation);
        
        this.sub = vm
            .WhenAnyProperty()
            .Where(x => x.PropertyName == nameof(ParkingViewModel.ParkLocation))
            .Subscribe(_ =>
            {
                if (vm.ParkLocation == null)
                    this.ParkingMap.Pins.Clear();
                else
                    this.SetPin(vm.ParkLocation);
            });

        var mapSpan = MapSpan.FromCenterAndRadius(
            new Location(vm.CenterOfPark.Latitude, vm.CenterOfPark.Longitude),
            Microsoft.Maui.Maps.Distance.FromMeters(vm.MapStartZoomDistanceMeters)
        );
        this.ParkingMap.MoveToRegion(mapSpan);
        base.OnBindingContextChanged();
    }

    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.sub?.Dispose();
    }


    void SetPin(Position position)
    {
        this.ParkingMap.Pins.Add(new Pin
        {
            Label = "YOU PARKED HERE",
            Type = PinType.SavedPin,
            Location = new Location(position.Latitude, position.Longitude)
        });
    }
}