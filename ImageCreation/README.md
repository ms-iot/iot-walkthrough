---
---
# IoT image creation

## Introduction

To deploy multiple IoT boards, it is desirable to have a final image of the operating system and all apps and configurations. The device can be ready for deployment after flashing the image, without requiring the installation of apps and settings separately, speeding up the deployment.

## Required tools

[Follow the instructions here to download required tools.](https://msdn.microsoft.com/windows/hardware/commercialize/manufacture/iot/set-up-your-pc-to-customize-iot-core) After downloading the ISO with packages and manifests, install the ARM package (*Windows_10_IoT_Core_ARM_Packages.msi*) to be able to deploy to the DragonBoard.

## Creating a basic image

[Follow the instructions here](https://msdn.microsoft.com/windows/hardware/commercialize/manufacture/iot/create-a-basic-image) to set the OEM name and open a shell. Choose an ARM environment when prompted. Install the OEM certificates, build the packages and create a package (`newproduct Showcase`, for example); however don't build your image yet. The default ARM setup targets a Raspberry Pi and a few changes are required to target a DragonBoard:

* Download the DragonBoard packages from directly from Microsoft (**TODO:** will these packages be public in the future?) and save them to `C:\Program Files (x86)\Windows Kits\10\MSPackages\Retail\ARM\fre`.
* Copy the contents of `C:\IoT-ADK-AddonKit\Source-arm\BSP\QCDB410C\OEMInputSamples\TestOEMInput.xml` to `C:\IoT-ADK-AddonKit\Source-arm\Products\Showcase\TestOEMInput.xml`.
* Run `buildimage Showcase Test`. A FFU image will be available at `C:\IoT-ADK-AddonKit\Build\arm\Showcase\Test`.

To test the image, [flash it to a DragonBoard following these instructions](https://developer.microsoft.com/en-us/windows/iot/docs/getstarted/dragonboard/getstartedstep2).

## Adding apps to the image
