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
global using ShinyWonderland.Features;
global using ShinyWonderland.Features.AI;
global using ShinyWonderland.Features.AI.Handlers;
global using ShinyWonderland.Features.AI.Pages;
global using ShinyWonderland.Features.Hours;
global using ShinyWonderland.Features.Hours.Handlers;
global using ShinyWonderland.Features.Hours.Pages;
global using ShinyWonderland.Features.Hours.Tools;
global using ShinyWonderland.Features.MealTimes;
global using ShinyWonderland.Features.MealTimes.Delegates;
global using ShinyWonderland.Features.MealTimes.Handlers;
global using ShinyWonderland.Features.MealTimes.Pages;
global using ShinyWonderland.Features.Parking;
global using ShinyWonderland.Features.Parking.Delegates;
global using ShinyWonderland.Features.Parking.Pages;
global using ShinyWonderland.Features.Rides;
global using ShinyWonderland.Features.Rides.Delegates;
global using ShinyWonderland.Features.Rides.Handlers;
global using ShinyWonderland.Features.Rides.Pages;
global using ShinyWonderland.Features.Rides.Tools;
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
