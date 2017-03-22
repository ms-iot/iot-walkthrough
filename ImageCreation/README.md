---
---
# IoT image creation

## Introduction

To deploy multiple IoT boards, it is desirable to have a final image of the operating system and all apps and configurations. The device can be ready for deployment after flashing the image, without requiring the installation of apps and settings separately, speeding up the deployment. We will be targeting the Qualcomm DragonBoard in this tutorial.

## Required tools

[Follow the instructions here to download required tools.](https://msdn.microsoft.com/windows/hardware/commercialize/manufacture/iot/set-up-your-pc-to-customize-iot-core) After downloading the ISO with packages and manifests, install the ARM package (*Windows_10_IoT_Core_ARM_Packages.msi*).

Download the DragonBoard packages by contacting a Qualcomm provider.  Save them to `C:\Program Files (x86)\Windows Kits\10\MSPackages\Retail\ARM\fre`.

## Creating a basic image

[Follow the instructions here](https://msdn.microsoft.com/windows/hardware/commercialize/manufacture/iot/create-a-basic-image) up to *Create a test project*. The name *Showcase* will be assumed for the rest of the tutorial; if your project has a different name, replace it accordingly. The default ARM setup targets a Raspberry Pi and a few changes are required to build for a DragonBoard:

* Copy the contents of `C:\IoT-ADK-AddonKit\Source-arm\BSP\QCDB410C\OEMInputSamples\TestOEMInput.xml` to `C:\IoT-ADK-AddonKit\Source-arm\Products\Showcase\TestOEMInput.xml`.
* Run `buildimage Showcase Test`. A FFU image will be available at `C:\IoT-ADK-AddonKit\Build\arm\Showcase\Test`.
* Flash it to the DragonBoard using the `DragonBoardUpdateTool`. [More instructions are available here](https://developer.microsoft.com/en-us/windows/iot/docs/getstarted/dragonboard/getstartedstep2).

## Adding apps to the image

[These instructions are also available here.](https://msdn.microsoft.com/en-us/windows/hardware/commercialize/manufacture/iot/deploy-your-app-with-a-standard-board)

Open your solution, right click the desired project and choose *Store > Create App Packages...*.

![Creating App Package.png](Creating App Package.png)

Choose *No* when asked whether you want to upload to the store. Choose an *Output location* without spaces, *Never* at *Generate app bundle* and keep the *ARM* architecture in *Debug* or *Release* mode.

Inside an *IoTCoreShell*, run `newappxpkg "C:\<Output location>\<Build folder>\<appx file>" Appx.Showcase` (e.g. `newappxpkg C:\Users\username\Showcase\AppPackages\Showcase_1.1.1.0_ARM_Test\Showcase_1.1.1.0_ARM.appx Appx.Showcase`). This will create the folder `C:\IoT-ADK-AddonKit\Source-arm\Packages\Appx.Showcase` with files to build your package. Run `buildpkg Appx.Showcase` to build it.

To add a second app (for example, the background app for the weather station), use `newappxpkg`, but do a few required changes before building. We will do changes to `AppInstall.cmd` and remove `Microsoft.NET.CoreRuntime.1.1.appx` and `Microsoft.VCLibs.ARM.Debug.14.00.appx` (the build tool will fail if file conflicts are detected, and these files and packages will be already provided by Appx.Showcase).

First, rename `AppInstall.cmd` to `AppInstallBackground.cmd` and replace its contents with this slightly modified script, which skips installation of dependencies and calls `iotstartup.exe add headless` instead of headed:

```bat
@echo off

::
:: Ensure we execute batch script from the folder that contains the batch script
::
pushd %~dp0
SETLOCAL

if not exist %systemdrive%\windows\system32\deployappx.exe (
    echo Error: deployappx.exe not found. exiting.
    exit /b 1
)

call AppxConfigBackground.cmd

echo Appx Name :%AppxName%

REM Set defaults for optional parameters
if not defined forceinstall ( set forceinstall=0 )
if not defined launchapp ( set launchapp=1 )
if not exist .\logs ( mkdir logs ) else ( del /Q .\logs\*.* )

REM
REM Install the Main Appx
REM
if %forceinstall% == 1 (
    set INSTALL_PARAMS=install force %AppxName%.appx
) else (
    set INSTALL_PARAMS=install %AppxName%.appx
)
echo Installing %AppxName%.appx with %INSTALL_PARAMS%
deployappx.exe %INSTALL_PARAMS% > %temp%\%AppxName%_result.txt
if "%ERRORLEVEL%"=="0" (
    if %launchapp% == 1 (
        call :LAUNCH_APP
    )
) else (
    echo. Error in installing %AppxName%.appx.
    echo. Result:%ERRORLEVEL%
)

goto :CLEANUP

:LAUNCH_APP
deployappx.exe getpackageid %AppxName%.appx > .\logs\packageid.txt
for /f "tokens=2,5 delims=:_" %%A in (.\logs\packageid.txt) do (
    set AppxID=%%A_%%B
)
set AppxID=%AppxID: =%
echo Launching %AppxID%
REM Trigger IoTStartup
iotstartup.exe add headless %AppxID%

exit /b

:SUB_CHECKERROR
if "%ERRORLEVEL%"=="0" exit/b
echo.
echo Error %1
echo Result=%ERRORLEVEL%
echo.
exit /b %ERRORLEVEL%

:CLEANUP
popd
ENDLOCAL
exit /b
```

Change the package's manifest (`Appx.BackgroundWeatherStation.pkg.xml`) to install only the app, the certificate, `AppxConfigBackground.cmd` and `AppInstallBackground.cmd`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Package xmlns="urn:Microsoft.WindowsPhone/PackageSchema.v8.00"
         Owner="$(OEMNAME)" OwnerType="OEM" ReleaseType="Production"
         Platform="arm" Component="Appx" SubComponent="BackgroundWeatherStation">
   <Components>
      <OSComponent>
         <Files>
            <File Source="AppInstall\AppInstallBackground.cmd"
                  DestinationDir="$(runtime.root)\AppInstall"
                  Name="AppInstallBackground.cmd" />
            <File Source="AppInstall\AppxConfigBackground.cmd"
                  DestinationDir="$(runtime.root)\AppInstall"
                  Name="AppxConfigBackground.cmd" />
            <File Source="AppInstall\BackgroundWeatherStation_1.0.3.0_ARM_Debug.appx"
                  DestinationDir="$(runtime.root)\AppInstall"
                  Name="BackgroundWeatherStation_1.0.3.0_ARM_Debug.appx" />
            <File Source="AppInstall\BackgroundWeatherStation_1.0.3.0_ARM_Debug.cer"
                  DestinationDir="$(runtime.root)\AppInstall"
                  Name="BackgroundWeatherStation_1.0.3.0_ARM_Debug.cer" />
         </Files>
      </OSComponent>
   </Components>
</Package>

```

Then run `buildpkg Appx.BackgroundWeatherStation`.

Open file `C:\IoT-ADK-AddonKit\Source-arm\Packages\OEMFM.xml` and add your package file to the OEM features:

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

Next, open `C:\IoT-ADK-AddonKit\Source-arm\Products\Showcase\TestOEMInput.xml` and add `<AdditionalFM>%COMMON_DIR%\Packages\OEMCommonFM.xml</AdditionalFM>` and `<AdditionalFM>%SRC_DIR%\Packages\OEMFM.xml</AdditionalFM>` to the `AdditionalFMs` block. Add the required OEM packages to the OEM features (`OEM_AppxMain`, `OEM_CustomCmd`, `OEM_ProvAuto` and `OEM_AppxHelloWorld`) and comment the sample packages. Furthermore, if speech synthesis is desired, add the `IOT_SPEECHDATA_EN_US` package to the list of features.

**Note:** Your app might crash if features are missing. [Check the complete list of features if you need to find a missing feature.](https://msdn.microsoft.com/en-us/windows/hardware/commercialize/manufacture/iot/iot-core-feature-list) There is a limit to the number of times a startup app is allowed to crash before a board reboot is issued; if your app works on a production image but not on an OEM image (eg. app stuck at the loading screen and the board reboots after some time), you are probably missing some feature.

The final manifest should look like:

```xml
<?xml version="1.0" encoding="utf-8"?>
<OEMInput
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xmlns:xsd="http://www.w3.org/2001/XMLSchema"
  xmlns="http://schemas.microsoft.com/embedded/2004/10/ImageUpdate">
  <Description>Windows 10 IoT Core 8016sbc Test FFU generation for arm.fre with build number 20150812-1709 by wesign</Description>
  <SOC>QC8016</SOC>
  <SV>Qualcomm</SV>
  <Device>8016sbc</Device>
  <ReleaseType>Test</ReleaseType>
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
    <AdditionalFM>%AKROOT%\FMFiles\arm\QCDB410CTestFM.xml</AdditionalFM>
    <!-- Including OEM feature manifest -->
    <AdditionalFM>%COMMON_DIR%\Packages\OEMCommonFM.xml</AdditionalFM>
    <AdditionalFM>%SRC_DIR%\Packages\OEMFM.xml</AdditionalFM>
    <!-- Including the test features -->
    <AdditionalFM>%AKROOT%\FMFiles\arm\IoTUAPNonProductionPartnerShareFM.xml</AdditionalFM>
  </AdditionalFMs>
  <Features>
    <Microsoft>
      <Feature>IOT_EFIESP_TEST</Feature>
      <Feature>IOT_KDUSB_SETTINGS</Feature>
      <Feature>IOT_EFIESP_BCD</Feature>
      <Feature>IOT_DISABLEBASICDISPLAYFALLBACK</Feature>
      <Feature>PRODUCTION_CORE</Feature>
      <Feature>PRODUCTION</Feature>
      <Feature>IOT_UAP_OOBE</Feature>
      <Feature>IOT_TOOLKIT</Feature>
      <Feature>IOT_WDTF</Feature>
      <Feature>IOT_SSH</Feature>
      <Feature>IOT_SIREP</Feature>
      <Feature>IOT_WEBB_EXTN</Feature>
      <Feature>IOT_UMDFDBG_SETTINGS</Feature>
      <Feature>IOT_NETCMD</Feature>
      <Feature>IOT_POWERSHELL</Feature>
      <Feature>IOT_DIRECTX_TOOLS</Feature>
      <!-- <Feature>IOT_ALLJOYN_APP</Feature> -->
      <Feature>IOT_ENABLE_TESTSIGNING</Feature>
      <Feature>IOT_DISABLE_UMCI</Feature>
      <Feature>IOT_CRT140</Feature>
      <!-- <Feature>IOT_BERTHA</Feature> -->
      <Feature>IOT_APP_TOOLKIT</Feature>
      <Feature>IOT_CP210x_MAKERDRIVER</Feature>
      <Feature>IOT_FTSER2K_MAKERDRIVER</Feature>
      <Feature>IOT_SPEECHDATA_EN_US</Feature>
      <!-- <Feature>IOT_ENABLE_ADMIN</Feature> -->
    </Microsoft>
    <OEM>
      <Feature>QC_UEFI_TEST</Feature>
      <Feature>SBC</Feature>
      <Feature>QCDB410C_DEVICE_TARGETINGINFO</Feature>
      <Feature>QCDB410C_DEVICE_INFO</Feature>
      <Feature>OEM_CustomCmd</Feature>
      <Feature>OEM_ProvAuto</Feature>
      <Feature>OEM_AppxShowcase</Feature>
      <Feature>OEM_AppxBackgroundWeatherStation</Feature>
    </OEM>
  </Features>
  <Product>Windows 10 IoT Core</Product>
</OEMInput>
```

To make your app install automatically and become the default startup app, edit `C:\IoT-ADK-AddonKit\Source-arm\Products\Showcase\oemcustomization.cmd` and uncomment the lines that execute `C:\AppInstall\AppInstall.cmd` and add commands to run `C:\AppInstall\AppInstallBackground.cmd`. The final script is:

```bat
@echo off
REM OEM Customization Script file
REM This script if included in the image, is called everytime the system boots.

REM Enable Administrator User
net user Administrator p@ssw0rd /active:yes

if exist C:\OEMTools\InstallAppx.cmd (
    REM Run the Appx Installer. This will install the appx present in C:\OEMApps\
    call C:\OEMTools\InstallAppx.cmd
)

if exist C:\AppInstall\AppInstallBackground.cmd (
    REM Install background app
    call C:\AppInstall\AppInstallBackground.cmd > %temp%\BackgroundAppInstallLog.txt
)

if exist C:\AppInstall\AppInstall.cmd (
    REM Install app
    call C:\AppInstall\AppInstall.cmd > %temp%\AppInstallLog.txt
)

cd \
rmdir /S /Q C:\OEMInstall
```

Build the image with `buildimage Showcase Test` and your app will show up after boot!
