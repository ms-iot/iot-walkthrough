---
---
# Integration with the OpenWeatherMap service

To show local weather information, we will be using the [OpenWeatherMap](https://openweathermap.org/) API. With it we are able to show local temperature, humidity, pressure and a description of the weather. Their API provides much more data, but we will show the user only the most interesting fields. You can see their plan options [here](https://home.openweathermap.org/subscriptions); we will be using the free plan, which provides up to 60 queries per minute.

**TODO screenshot of the final result**

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
