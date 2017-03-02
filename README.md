# Developer Experience showcase project

## Introduction

1. About the project
    * [Software components](SoftwareComponents.md)
    * External services
    * [Wiring of weather shield to DragonBoard 410c](Wiring/README.md)
2. Background application
    * [Installing IoT templates and deploying a background app](Background/Installation/README.md)
    * [Collecting sensor data through I2C](Background/Sensing/README.md)
    * Connecting to an Azure Provisioned Remote Monitoring solution
3. Inter-application communication
    * [Associating the app with the Windows store](StoreDeployment/README.md)
    * Creating an app service
    * Connecting to the app service
4. Foreground application
    * Creating a base foreground application
    * Bundling the app service
    * Showing local weather data
    * Usage of text-to-speech
    * Receiving voice commands
    * Showing a slideshow
    * Playing media
5. Integration with third-party services
    * Saving application keys on Azure
    * Synchronizing settings with Azure
    * [Integration with OpenWeatherMap](OpenWeatherMapsIntegration.md)
    * Integration with Bing news
    * Showing OneDrive pictures
6. Preparing for deployment
    * [Enabling Secure Boot, BitLocker and the TPM module](Security/README.md)
    * [Saving Azure keys to the TPM module and connecting with tokens](Security/TPM/README.md)
    * Provisioning an IoT image
    * Zero touch provisioning
