---
---
# Enabling Secure Boot, BitLocker and the ConfigCI

## Introduction
Secure Boot enforces the verification of signatures of binaries before booting them. By enabling Secure Boot and checking for Microsoft signatures, we ensure that boot code has not been tampered with.

BitLocker is a full disk encryption feature of Windows, which also adds system integrity verification of early boot files. [The documentation for BitLocker can be found here.](https://technet.microsoft.com/en-us/library/cc732774(v=ws.11).aspx)

ConfigCI (Configurable Code Integrity) enforces signing of binaries to avoid execution of untrusted code.

To enable these security features, we will follow the [instructions on this page](https://developer.microsoft.com/en-us/windows/iot/docs/SecureBootAndBitLocker). A supported board is required. The steps described here are valid for the DragonBoard 410c, which contains a Trusted Platform Module (TPM) chip. Devices without a TPM won't support all BitLocker features.

It is assumed that the device has been previously flashed with a Windows 10 IoT Core image, and the PC has the required Windows SDK (usually installed alongside Visual Studio) for Windows 8.1 or newer.

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
Follow the [instructions on this page](https://developer.microsoft.com/en-us/windows/iot/Docs/TurnkeySecurity.htm) to generate certificates and private keys. 

Alternatively, for testing purposes, [pre-generated certificates](https://github.com/ms-iot/security/tree/master/PreGenPackage) can be used; however, they are **NOT** secure for production deployments.

* Download the files [from this folder](https://github.com/ms-iot/security/tree/master/CertGen). Run the **MakeSB.ps1** script in an administrative PowerShell console.
    * You might need to [set the PowerShell execution policy to a less restricted value](https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.security/set-executionpolicy).
    * The script assumes a x64 PC with the Windows Developer Kit tools for **Windows 8.1 or newer** installed to `C:\Program Files (x86)\Windows Kits\10\bin\x64\`. Edit the script if this is not the case.

## Enabling SecureBoot/BitLocker/ConfigCI
To enable these features, [follow the instructions at this page](https://developer.microsoft.com/en-us/windows/iot/Docs/TurnkeySecurity.htm).
