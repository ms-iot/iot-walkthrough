# Developer Experience showcase project

## Introduction

This project's goal is to show intended usage guidelines of Windows on IoT. To fulfill this goal, our solution will have two applications:

* One background application to receive sensor data and send it to the cloud. Receiving sensor data and analyzing it are important tasks in IoT and a device will often operate in "headless" mode for monitoring; thus, we separate these tasks in an independent app.
* One foreground application for user interaction. This application shows local weather (read by the background app), information from the internet (news and regional weather) and interacts with the user (playing media or showing a slideshow).

The applications are written using Universal Windows Platform (UWP); thus, the same foreground app can be run on both IoT and Desktop.

## Guides

1. About the project
    * [Software components](SoftwareComponents.md)
    * [Wiring of weather shield to DragonBoard 410c](Wiring/README.md)
2. Background application
    * [Installing IoT templates and deploying a background app](Background/Installation/README.md)
    * [Collecting sensor data through I2C](Background/Sensing/README.md)
3. Inter-application communication
    * [Associating the app with the Windows store](StoreDeployment/README.md)
    * Creating an app service
    * Connecting to the app service
4. Foreground application
    * Creating a foreground application
    * Bundling the app service
    * Showing local weather data
    * Usage of text-to-speech
    * Receiving voice commands
    * Showing a slideshow
    * Playing media
5. Connecting to the Azure cloud
    * [Connecting to an Azure IoT Hub using the Provisioned Remote Monitoring solution](Azure/IoTHubProvisionedSolution)
    * Saving application keys on Azure
    * Synchronizing settings with Azure
6. Integration with third-party services
    * [Integration with OpenWeatherMap](OpenWeatherMapsIntegration.md)
    * Integration with Bing news
    * Showing OneDrive pictures
7. Preparing for deployment
    * [Enabling Secure Boot, BitLocker and ConfigCI](Security/README.md)
    * [Saving Azure keys to the TPM module and connecting with tokens](Security/TPM/README.md)
    * [IoT image creation](ImageCreation/README.md)
    * Zero touch provisioning
8. Deployment
    * [Creating a retail OEM image](ImageCreation/Retail/README.md)
