using Shiny.Hosting;
using ShinyWonderland.Services;

namespace ShinyWonderland;

public partial class AppShell : Shell
{
    public AppShell()
    {
        this.InitializeComponent();
    }

    
    // protected override void OnNavigating(ShellNavigatingEventArgs args)
    // {
    //     Console.WriteLine("Before: " + CurrentPage?.GetType().FullName ?? "NONE");
    //     base.OnNavigating(args);
    //     Console.WriteLine("After: " + CurrentPage?.GetType().FullName ?? "NONE");
    // }


    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        if (this.CurrentPage.BindingContext == null)
        {
            var navBuilder = Host.GetService<ShinyNavigationBuilder>()!;
            var viewModelType = navBuilder.GetViewModelTypeForPage(this.CurrentPage.GetType());
            if (viewModelType != null)
            {
                // this.Window.Resumed
                var vm = Host.Current.Services.GetService(viewModelType);
                this.CurrentPage!.BindingContext = vm;
            }
        }
        if (this.CurrentPage.BindingContext is INavigatedAware navAware)
            navAware.OnNavigatedTo();
        
        base.OnNavigated(args);
    }
}