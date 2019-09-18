# SimpleTotp
### Simple TOTP library for .NET Standard 1.3

This is a simple library that provides server-side code (and simple client-side code) required to use [**T**ime-based **O**ne-**T**ime **P**asswords](https://en.wikipedia.org/wiki/Time-based_One-time_Password_algorithm) as [Two-factor authentication](https://en.wikipedia.org/wiki/Multi-factor_authentication) in your app.

Both the TOTP generation/validation and the registration info should work correctly with any existing TOTP 2FA authenticator (we did test it with [Google Authenticator](https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2) and [Microsoft Authenticator](https://play.google.com/store/apps/details?id=com.azure.authenticator)).

## Motivation
When looking for an existing TOTP library on [NuGet](https://www.nuget.org/) we've realized that there's only a couple available and all of them seem to suffer from one of the following problems (or a mixture of them):

* Dubious licensing (dev-unfriendly license, utilizing third-party components (e.g. [Google Chart API](https://en.wikipedia.org/wiki/Google_Chart_API)), which impose their own licenses, etc.)
* Overcomplicated code
* Dependence on a fixed, third-part method of QR code generation

We've ended up writing our own code and we thought it would be useful to share with the community.

The project is provided as-is, but we're more than willing to improve and work on any issues that might have slipped our radar - just raise an issue (or feel free to just send a PR with a fix our way).

## Installation

*TBD - once we get the package out on NuGet*

## Examples of use

*Note: all code examples below are simplified to focus on how to use the library, they do not necessarily reflect good programming practices.*

### 1. Registering an authenticator

```csharp
// generate a secret key (unique string)
// (make sure it can't be guessed, so don't use e.g. user name)
// remember to persist it somehow (if you're using encryption,
// make sure it's a reversible algorithm - you're going to need this later)
var secretKey = YourCode_GenerateTheSecretKey();

// Use the TotpProvider to generate registration data
// (use an actual account name and issuer)
var provider = new TotpProvider();
var registrationData = provider.GetAuthenticatorRegistrationData("ACCOUNT_NAME", "ISSUER", secretKey);

// generate the QR code using some third-party library and present to the user
YourCode_GenerateTheQrCode(registrationData.QrCodeUri);

// alternatively, present the key for the user so they can register manually
YourCode_DisplayManualCode(registrationData.ManualRegistrationKey)

// (actually, do both - and let the user decide)
```

Because we wanted to avoid having a dependency on specific QR code generation library, `RegistrationData.QrCodeUri` is a String that you need to encode in a QR code. Here's a simple way how you could use [QRCoder](https://www.nuget.org/packages/QRCoder/), a library we ended up using in our project (note: we're not responsible for it any way or form).

1. Install **QRCoder**
```powershell
PM> Install-Package QRCoder
```
2. Use a `QRCodeGenerator` to create your QR image
```csharp
var qrCode = new QRCodeGenerator().CreateQrCode(registrationData.QrCodeUri,
                                                QRCodeGenerator.ECCLevel.L);
var png = new PngByteQRCode(qrCode);
var image = png.GetGraphic(20);
yourCode_ImageSrc = $"data:image/gif;base64,{Convert.ToBase64String(image)}";
```

Note that if you don't want to worry about generating the secret key yourself, we've got you covered - a new GUID (as String) will be generated if you don't provide your own secret key:

```csharp
var provider = new TotpProvider();
var registrationData = provider.GetAuthenticatorRegistrationData("ACCOUNT_NAME", "ISSUER");

// you still need to persist the key somehow though
// (and be able to retrieve it later)
YourCode_SaveTheSecretKey(registrationData.SecretKey);
```

##### IMPORTANT
There's always some possibility of human error, especially for users not familiar with two-factor authentication. Before you actually mark the user as using 2FA in your system, make sure their authenticator is properly registered. The easiest way to do it is to ask the user to input a code generated from the authenticator and validate it:

```csharp
var registrationSuccessful = provider.ValidateCode(secretKey,
                                                  yourCode_TheCodeUserInputted,
                                                  DateTimeOffset.Now);
```

### 2. Validating user's code
After the user registered for 2FA on your system, you need to start asking them for the 2FA codes and validate them in your system

```csharp
// this is why we've asked you to persist the user's secret key
var twoFASuccess = provider.ValidateCode(yourCode_UsersSecretKey,
                                         yourCode_TheCodeUserInputted,
                                         DateTimeOffset.Now);

// there are also overloads that allow you to fine-tune the tolerance of
// checking past/future codes (both because it takes a moment for the user
// to input the code and because their device's clock might be slightly off)
```

### 3. Displaying the current code
If you feel adventurous and want to write your own Authenticator app, you can use the `TotpProvider` to display the current code (and remaining time until it changes)

```csharp
var provider = new TotpProvider();
var code = provider.GetCodeAtSpecificTime(yourCode_UsersSecretKey,
                                          DateTimeOffset.Now);
// alternatively
code = provider.GetCodeAtSpecificTime(yourCode_UsersSecretKey,
                                      DateTimeOffset.Now,
                                      out TimeSpan remaining);
```

## Contribute

As mentioned above, feel free to contribute to our project. We do not have any specific guidelines for contribution at the moment (mainly because we don't expect a lot of it, so it seemed redundant to write them) - so just fork away and we'll try to work something out if needed (feel free to contact us ahead of time if you want).

## License

**SimpleTotp** is licensed under MIT License.

Copyright (c) 2019 Al4ric, decPL & kryzalid87