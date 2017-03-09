---
---
# Integration with the OpenWeatherMap service

## Introduction

To show local weather information, we will be using the [OpenWeatherMap](https://openweathermap.org/) API. With it we are able to show local temperature, humidity, pressure and a description of the weather. Their API provides much more data, but we will show the user only the most interesting fields. You can see their plan options [here](https://home.openweathermap.org/subscriptions); we will be using the free plan, which provides up to 60 queries per minute.

**Note:** The free plan should be used for demo purposes only, since it doesn't support HTTPS. There is no way to guarantee data hasn't been tampered with before reaching your application.

The final interface will show weather data on the corner of the screen:

![Final UI](Final UI.png)

## Using the API

* Go to the [OpenWeatherMap signup page](http://home.openweathermap.org/users/sign_up). After you create an account, check your [API keys](https://home.openweathermap.org/api_keys) page and create one. Your API key identifies your application and will be added to all service requests as the `appid` parameter.
* We will use the search by ZIP code API, [available here](https://openweathermap.org/current#zip). We should do a GET request to `http://api.openweathermap.org/data/2.5/weather` with the parameter `zip=<zip code>,<country>`. For example, if you access the address `http://api.openweathermap.org/data/2.5/weather?zip=98052,us&appid=<you API key>` from your browser, you should get weather information for Redmond.
* The format of the weather response is [documented here](https://openweathermap.org/current#parameter). The fields of interest are:

| Field                 | Description                              |
|-----------------------|------------------------------------------|
| `weather.main`        | Name of the weather condition (eg. Snow) |
| `weather.description` | Longer description of weather condition  |
| `weather.icon`        | Weather icon ID                          |
| `main.temp`           | Temperature in Kelvin                    |
| `main.pressure`       | Pressure in hPa                          |
| `main.humidity`       | Humidity in %                            |

Since we will be parsing HTTP JSON responses in our code often, a helper function to do the request and check for errors is handy. [A simple helper is available here and should be trivial to understand](https://github.com/ms-iot/devex_project/blob/master/CS/Showcase/HttpHelper.cs). The `TryGetJsonAsync` function returns a `JsonObject` if successful, null otherwise.

Create a `OpenWeatherMap.cs` class. The function to build the request must include parameters specifying the location and app ID:

```cs
class OpenWeatherMap
{
    private const String ENDPOINT = "http://api.openweathermap.org/data/2.5/weather";
    // Properties
    private string _zip;
    private string _country;
    private string _key;

    private HttpRequestMessage BuildRequest()
    {
        Uri uri = new Uri(String.Format("{0}?zip={1},{2}&appid={3}", ENDPOINT, _zip, _country, _key));
        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
        return req;
    }
```
