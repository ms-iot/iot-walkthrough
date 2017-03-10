---
---
# Creating a retail OEM image

## Introduction

Now that we have created a test OEM image and ensured that everything works, it's time to create a retail OEM image. The retail image does not include development tools (eg. PowerShell access), being locked from external access.

It is assumed that the tools listed in [IoT image creation](../README.md) are installed.

## Creating a retail image

We'll start with the template manifest for retail images and add the features we need. Copy `C:\IoT-ADK-AddonKit\Source-arm\BSP\QCDB410C\OEMInputSamples\RetailOEMInput.xml` to `C:\IoT-ADK-AddonKit\Source-arm\Products\Showcase`. The next steps are similar to the ones used in a test OEM image:

* Create and build your packages with `newappxpkg` and `buildpkg`. More documentation is available at the [IoT image creation](../README.md) page.
* Add your app packages to one OEM feature manifest. For example, add the following to `C:\IoT-ADK-AddonKit\Source-arm\Packages\OEMFM.xml` to make `Appx.Showcase` and `Appx.BackgroundWeatherStation` available:

```xml
<PackageFile Path="%PKGBLD_DIR%" Name="%OEM_NAME%.Appx.Showcase.cab">
  <FeatureIDs>
    <FeatureID>OEM_AppxShowcase</FeatureID>
  </FeatureIDs>
</PackageFile>
<PackageFile Path="%PKGBLD_DIR%" Name="%OEM_NAME%.Appx.BackgroundWeatherStation.cab">
  <FeatureIDs>
    <FeatureID>OEM_AppxBackgroundWeatherStation</FeatureID>
  </FeatureIDs>
</PackageFile>
```

* On the top level manifest, uncomment the lines including OEM feature manifests and comment unneeded features.
* Add your app to the desired features. Also add the scripts that will automatically set your app as the default app:

```xml
<!-- OEM application -->
<Feature>OEM_CustomCmd</Feature>
<Feature>OEM_ProvAuto</Feature>
<Feature>OEM_AppxShowcase</Feature>
<Feature>OEM_AppxBackgroundWeatherStation</Feature>
```

* The final top-level manifest is:

```xml
<?xml version="1.0" encoding="utf-8"?>
<OEMInput
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xmlns:xsd="http://www.w3.org/2001/XMLSchema"
  xmlns="http://schemas.microsoft.com/embedded/2004/10/ImageUpdate">
  <Description>Windows 10 IoT Core 8016sbc Retail FFU generation for arm.fre with build number 20150812-1709 by wesign</Description>
  <SOC>QC8016</SOC>
  <SV>Qualcomm</SV>
  <Device>8016sbc</Device>
  <ReleaseType>Production</ReleaseType>
  <BuildType>fre</BuildType>
  <SupportedLanguages>
    <UserInterface>
      <Language>en-us</Language>
    </UserInterface>
    <Keyboard>
      <Language>en-us</Language>
    </Keyboard>
    <Speech>
      <Language>en-us</Language>
    </Speech>
  </SupportedLanguages>
  <BootUILanguage>en-us</BootUILanguage>
  <BootLocale>en-us</BootLocale>
  <Resolutions>
    <Resolution>1024x768</Resolution>
  </Resolutions>
  <AdditionalFMs>
    <!-- Including BSP feature manifest -->
    <AdditionalFM>%BSPSRC_DIR%\QCDB410C\Packages\QCDB410CFM.xml</AdditionalFM>
    <!-- Including OEM feature manifest -->
    <AdditionalFM>%COMMON_DIR%\Packages\OEMCommonFM.xml</AdditionalFM>
    <AdditionalFM>%SRC_DIR%\Packages\OEMFM.xml</AdditionalFM>
  </AdditionalFMs>
  <Features>
    <Microsoft>
      <Feature>IOT_EFIESP</Feature>
      <Feature>IOT_EFIESP_BCD</Feature>
      <!-- <Feature>IOT_DMAP_DRIVER</Feature> -->
      <Feature>IOT_CP210x_MAKERDRIVER</Feature>
      <Feature>IOT_FTSER2K_MAKERDRIVER</Feature>
      <!-- <Feature>IOT_GENERIC_POP</Feature> -->
      <!-- Following two required for Appx Installation -->
      <Feature>IOT_UAP_OOBE</Feature>
      <Feature>IOT_APP_TOOLKIT</Feature>
      <Feature>IOT_SPEECHDATA_EN_US</Feature>
      <!-- for Connectivity -->
      <!-- <Feature>IOT_SSH</Feature> -->
      <!-- <Feature>IOT_ENABLE_ADMIN</Feature> -->
    </Microsoft>
    <OEM>
      <!-- Include BSP Features -->
      <Feature>QC_UEFI</Feature>
      <Feature>SBC</Feature>
      <Feature>QCDB410C_DEVICE_TARGETINGINFO</Feature>
      <Feature>QCDB410C_DEVICE_INFO</Feature>
      <!-- OEM application -->
      <Feature>OEM_CustomCmd</Feature>
      <Feature>OEM_ProvAuto</Feature>
      <Feature>OEM_AppxShowcase</Feature>
      <Feature>OEM_AppxBackgroundWeatherStation</Feature>
    </OEM>
  </Features>
  <Product>Windows 10 IoT Core</Product>
</OEMInput>
```
