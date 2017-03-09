---
---
# Enabling Secure Boot, BitLocker and the ConfigCI

## Introduction
Secure Boot enforces the verification of signatures of binaries before booting them. By enabling Secure Boot and checking for Microsoft signatures, we ensure that boot code has not been tampered with and is exactly was expected.

To enable Secure Boot, we will follow the [instructions on this page](https://developer.microsoft.com/en-us/windows/iot/docs/SecureBootAndBitLocker). A supported board is required. The steps described here are valid for the DragonBoard 410c. It is assumed that the device has been previously flashed with a Windows 10 IoT Core image, and the PC has the required Windows SDK (usually installed alongside Visual Studio) for Windows 8.1 or newer.

## Enabling RPMB
A Replay Protected Memory Block (RPMB) is required to enable Secure Boot.

* Hold the **Vol-**, **Vol+** and **Power** button, then power up the board. The Boot Device Selection (BDS) screen should show up.
* Move the cursor to **Provision RPB** and press **Power**. Confirm by pressing **Vol+**.

## Resetting previous setup
If SecureBoot was enabled on the board previously, it is recommended to reset the board:
* Hold the **Vol-**, **Vol+** and **Power** button, then power up the board. The Boot Device Selection (BDS) screen should show up.
* Navigate to **UEFI** menu and choose:
    * *Clear UEFI BS Variables*
    * *Clear UEFI RT Variables and fTPM (Erase RPMB)*

## Getting certificates
Follow the [instructions on this page](https://developer.microsoft.com/en-us/windows/iot/Docs/SecureBootAndBitLocker.htm#Certificates) to generate certificates and private keys. Alternatively, for testing purposes, [pre-generated certificates](https://github.com/ms-iot/security/tree/master/PreGenPackage) can be used; however, they are **NOT** secure for production deployments.

To generate the certificates:
* Download the files [from this folder](https://github.com/ms-iot/security/tree/master/CertGen). Run the **MakeSB.ps1** script in an administrative PowerShell console.
    * You might need to [set the PowerShell execution policy to a less restricted value](https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.security/set-executionpolicy).
    * The script assumes a x64 PC with the Windows Developer Kit tools for **Windows 8.1 or newer** installed to `C:\Program Files (x86)\Windows Kits\10\bin\x64\`. Edit the script if this is not the case.

## Enabling SecureBoot/BitLocker/ConfigCI
After booting the device, [open a SSH session](https://developer.microsoft.com/en-us/windows/iot/docs/ssh) to it. Copy the files from **TODO put link here once it goes public** to `C:\iotsec` and run `C:\iotsec\setup.cmd` as administrator. Reboot the board.
