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
        var vm = (MapRideTimesViewModel)this.BindingContext;
        if (vm != null)
        {
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(vm.CenterOfPark.Latitude, vm.CenterOfPark.Longitude),
                Microsoft.Maui.Maps.Distance.FromMeters(vm.MapStartZoomDistanceMeters)
            );
            this.TheMap.MoveToRegion(mapSpan);
        }
    }
}