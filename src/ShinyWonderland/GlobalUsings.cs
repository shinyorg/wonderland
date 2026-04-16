global using System;
global using System.Reactive.Linq;
global using System.Reactive.Threading.Tasks;
global using System.ComponentModel;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using Shiny;
global using Shiny.Locations;
global using ShinyWonderland.Services;

[assembly: XmlnsDefinition(
    "http://schemas.microsoft.com/dotnet/maui/global",
    "http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
)]
[assembly:XmlnsDefinition(
    "clr-namespace:ShinyWonderland",
    "shinyWonderland"
 )]
[assembly: XmlnsDefinition(
    "http://shiny.net/maui/controls",
    "shiny"
)]