# Canada's Wonderland Ride Times

This app does serve a functional purpose in that it shows available ride times to Canada's Wonderland
This data is pulled from [ThemeParks WIKI](https://themeparks.wiki/)

This app works offline without issue.
  
## Technology

This app also serves as a great example of using [Shiny Mediator](https://shinylib.net/client/mediator/)
At the time of this publishing, we used v4's [new persistent cache](https://shinylib.net/client/mediator/middleware/caching/) 
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