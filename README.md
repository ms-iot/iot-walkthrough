
# Windows 10 IoTCore Walkthrough
View the walkthrough here: https://ms-iot.github.io/iot-walkthrough/ 

## Introduction

This project's goal is to demonstrate guidelines for creating a Windows 10 IoTCore based product and walk through the creation of an IoT device, from implementation to final deployment.

The project has two applications:

* One background application to receive sensor data and send it to the Azure cloud. Receiving sensor data and analyzing it are important tasks in IoT and a device will often operate in "headless" mode for monitoring; thus, we separate these tasks in an independent app. It also receives application keys securely and saves user settings to Azure.
* One foreground application for user interaction. This application shows local weather (read by the background app), information from the internet (news and regional weather) and interacts with the user (playing media or showing a slideshow). A settings page is also available to change settings.

![App communication](AppCommunication.png)

The applications are written using Universal Windows Platform (UWP); thus, the same foreground app can be run on both IoT and Desktop.

## Guides

Steps from implementation of apps to deployment are documented with an end-to-end solution. Each tutorial shows small code snippets and then links to the code running in the walkthrough project.

![Sections](Sections.png)

1. About the project
    * [Software components](SoftwareComponents.md)
    * [Wiring of weather shield to DragonBoard 410c](Wiring/README.md)
2. Background application
    * [Installing IoT templates and deploying a background app](Background/Installation/README.md)
    * [Collecting sensor data through I2C](Background/Sensing/README.md)
3. Foreground application
    * [Creating a foreground application](Foreground/Creating/README.md)
    * [Usage of text-to-speech](Speech/TextToSpeech/README.md)
    * [Receiving voice commands](Speech/VoiceCommands/README.md)
    * [Playing media](Foreground/MediaPlayer/README.md)
4. Inter-application communication
    * [Creating an app service](AppService/Creation/README.md)
    * [Communication between applications](AppService/Communication/README.md)
    * [Showing local weather data](AppService/ShowingWeatherData/README.md)
5. Connecting to the Azure cloud
    * [Connecting to an Azure IoT Hub using the Preconfigured Remote Monitoring solution](Azure/IoTHubPreconfiguredSolution/README.md)
    * [Saving application keys on Azure](Azure/DeviceTwin/DesiredProperties/README.md)
    * [Synchronizing settings with Azure](Azure/DeviceTwin/ReportedProperties/README.md)
6. Integration with third-party services
    * [Integration with OpenWeatherMap](Integrations/OpenWeatherMap/README.md)
    * [Integration with Bing news](Integrations/BingNews/README.md)
    * [Associating the app with the Windows Store](StoreDeployment/README.md)
    * [OneDrive picture slideshow](Integrations/OneDrive/README.md)
7. Preparing for deployment
    * [Enabling Secure Boot, BitLocker and ConfigCI](Security/README.md)
    * [Saving Azure keys to the TPM module and connecting with tokens](Security/TPM/README.md)
    * IoT image creation
8. Deployment
    * Creating a retail OEM image


This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
