# Canada's Wonderland Ride Times

This app does serve a functional purpose in that it shows available ride times to Canada's Wonderland
This data is pulled from [ThemeParks WIKI](https://themeparks.wiki/)

This app works offline without issue.
  
## Technology

This app also serves as a great example of using [Shiny Mediator](https://shinylib.net/client/mediator/)
At the time of this publishing, we used v3's [new persistent cache](https://shinylib.net/client/mediator/middleware/caching/) 
that is updated in the background via a [Shiny Background Job](https://shinylib.net/client/jobs/)

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

## TODO
* GPS starts when app is open AND inside wonderland - stops when outsides
    * Forces job to run consistently
    * Notifications only fire when inside park

## Considering
* Notification of wait time drops for favourites only
* GPS nearby rides filter
  
## STRETCH FEATURES
* Parking Locator
* PIN coordinates to wonderland map where the drink stations are
* Peak times - requires server
    * Need weather at current time request
* Wonderland Map
* Restaurant Points with Menu & Prices