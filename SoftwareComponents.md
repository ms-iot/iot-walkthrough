---
---
# Software components

## Applications

This project's goal is to show intended usage guidelines of Windows on IoT. To fulfill this goal, our solution will have two applications:

* One background application to receive sensor data and send it to the cloud. Receiving sensor data and analysing it are important tasks in IoT and a device will often operate in "headless" mode for monitoring; thus, we separate these tasks in an independent app.
* One foreground application for user interaction. This application shows local weather (read by the background app) and information from the internet.

## External services

The project has some integrations with third-party services, coming from Windows APIs, SDKs available for Universal Windows Platform or use of REST APIs. These allow us to show the user customized information on the foreground app. The following integrations will be used:

| Service               | Used features                                                   |
|-----------------------|-----------------------------------------------------------------|
| Azure                 | Log sensor data, save settings on the cloud and fetch API keys  |
| Open Weather Maps     | Fetch weather condition                                         |
| Bing                  | Displaying current news                                         |
| OneDrive              | Fetch pictures from the user's account                          |
