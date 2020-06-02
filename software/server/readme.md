# Overview
This project enables live streaming music identification from PCM audio data.

### ACRCloud
##### Account
To use ACRCloud, you need to create an account here: [ACRCloud sign-up](https://console.acrcloud.com/signup)

After signing up, you will need to create a project as described here: [Recognize Music](https://docs.acrcloud.com/docs/acrcloud/tutorials/identify-music-by-sound/)
- Choose **Recorded Audio** for the audio source to deal with noise
- Choose **ACRCloudMusic ** bucket
- Enable **3rd Party ID Integration **
- Save the `host`, `access_key`, `access_secret` of your project

Then you will need to copy you project details to access ACRCloud:
- Copy `ACRCloudClientId.default.xml` to `ACRCloudClientId.xml` and enter the `host`, `access_key`, `access_secret` details

##### SDK
The ACRCloud SDK can be obtained from the [acrcloud/acrcloud_sdk_csharp repository](https://github.com/acrcloud/acrcloud_sdk_csharp#functions) on GitHub.
The `libs-vs2017` folder from (GitHub](https://github.com/acrcloud/acrcloud_sdk_csharp/tree/master/libs-vs2017) should be copied to the
AudioIdentification.ACRCloud folder. 

The `ACRCloudReference.cs` file in the `AudioIdentification.ACRCloud.UnitTests` project comes
from the `recognizer.cs` file in the `libs-vs2017` folder and is used to compare fingerprint results.

### Gracenote
##### Account
To use Gracenote, you need to create an account here: [Gracenote sign-up](https://www.gracenote.com/dev-zone/)

When I first started this project, Gracenote was supporting non-commercial developers. I build support for Gracenote
and then during 2019, one could no longer create or manage applications in Gracenote and my license was not working.

As of this writing, Gracenote now support only Commercial developers, here's the info from this link above:
> Contact us for information on becoming a licensed Commercial software developer.

After signing up, you will need to create a application.
- Save the `AppVersion`, `ClientId`, `ClientTag`, and `License` of your application

Then you will need to copy you project details to access ACRCloud:
- Copy `GracenoteClientId.default.xml` to `GracenoteClientId.xml` and enter the `AppVersion`, `ClientId`, `ClientTag` details
- Copy `GracenoteLicense.defailt.txt` to `GracenoteLicense.txt` and enter the `License`, include the `-- BEGIN LICENSE` and `END LICENSE xxxxxxxx --\r\n` text.

##### SDK
The Gracenote SDK, gnsdk, can be downloaded in three parts. The zip files should extracted to the gnsdk
folder within the CrazyGiraffe.AudioIdentification.Gracenote project.