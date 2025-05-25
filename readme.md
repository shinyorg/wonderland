# Canada's Wonderland Ride Times

This app does serve a functional purpose in that it shows available ride times to Canada's Wonderland
This data is pulled from [ThemeParks WIKI](https://themeparks.wiki/)

This app works offline without issue.
  
## Technology
This app uses several .NET open source technologies from [Shiny](https://github.com/shinyorg)

* [Shiny Mediator](https://github.com/shinyorg/mediator) - [Documentation](https://shinylib.net/mediator/)
  * [Smart HTTP client generation](https://shinylib.net/mediator/extensions/http/)
  * [Persistent app cache](https://shinylib.net/mediator/middleware/caching/#persistent-cache)
  * [Event Broadcasting](https://shinylib.net/mediator/events/) with [MAUI support](https://shinylib.net/mediator/extensions/maui/)
* [Shiny Mobile Libraries](https://github.com/shinyorg/shiny) - [Documentation](https://shinylib.net)
  * Periodic Background Jobs - Shiny.Jobs
  * Background GPS - Shiny.Locations
  * Geofencing - Shiny.Locations
  * Local Notifications - Shiny.Notifications
  * Mobile Centric AppSettings.json for Microsoft.Extensions.Configuration - Shiny.Extensions.Configuration

The HTTP API to themeparks wiki is generated using [Shiny Mediators OpenAPI source generator](https://shinylib.net/client/mediator/extensions/http/).  Look in the csproj for the following

```xml
<ItemGroup>
    <MediatorHttp Include="OpenApiRemote"
                  Uri="https://api.themeparks.wiki/docs/v1.yaml"
                  Namespace="ShinyWonderland.ThemeParksApi"
                  ContractPostfix="HttpRequest"
                  Visible="false" />
</ItemGroup>
```

## APP FEATURES
* Show ride times (including paid times if available) for Canada's Wonderland whether you are inside the park or now
* Ability to filter out closed rides, & rides that don't have an estimated time
* Basic parking locator
* Geofence notification reminder for entering to remind user to open app (so GPS is enabled) and to set parking area
* GPS based notifications while in the park notifying you if ride times have gone down - GPS shuts off once outside of park

## KNOWN ISSUES
* "You are currently offline" mediator event is not firing this until the page is hit again
* Android maps not setup - users need to setup their own keys and stuff
* Theme park API returns data even park is closed
* Hours (WIP)

## FEATURE IDEAS
* GPS distancing to each ride from current
* Peak times - requires server
    * Need weather at current time request
* Wonderland Map
* Restaurant Points with Menu & Prices
  * This data does not come back
  * Could do manual pins?