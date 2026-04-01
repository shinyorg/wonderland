using Microsoft.Maui.Maps;

namespace ShinyWonderland;


public partial class MapRideTimesPage : ContentPage
{
    public MapRideTimesPage()
    {
        this.InitializeComponent();
    }

    
    protected override void OnBindingContextChanged()
    {
        if (this.BindingContext is MapRideTimesViewModel vm)
        {
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(vm.CenterOfPark.Latitude, vm.CenterOfPark.Longitude),
                Microsoft.Maui.Maps.Distance.FromMeters(vm.MapStartZoomDistanceMeters)
            );
            this.TheMap.MoveToRegion(mapSpan);
        }
        base.OnBindingContextChanged();
    }
}