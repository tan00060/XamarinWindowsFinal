# Xamarin Windows Final

This is a two page Xamarin application that builds/deploys on the following three platforms:

#### Android ,iOS, UWP

## The application in UWP


### Start Page
<img src="https://raw.githubusercontent.com/tan00060/XamarinWindowsFinal/main/Images/StartScreen.PNG?token=ANBFY5LJFGTXZX53F36PMCTAQRPHU" width="250" height="400"/>

### Search for a random image
<img src="https://raw.githubusercontent.com/tan00060/XamarinWindowsFinal/main/Images/SearchRandomImage.PNG?token=ANBFY5MGQFU4ZP6EJHJFKS3AQRPZI" width="250" height="400"/>

### Upload your own image
<img src="https://github.com/tan00060/XamarinWindowsFinal/blob/main/Images/UploadImage.PNG" width="300" height="400"/>

### Capture a picture from your device
<img src="https://raw.githubusercontent.com/tan00060/XamarinWindowsFinal/main/Images/CameraCapture.PNG?token=ANBFY5IM76BLEYEH4J2YZ4TAQRP4C" width="250" height="400"/>

### Settings page
<img src="https://raw.githubusercontent.com/tan00060/XamarinWindowsFinal/main/Images/SettingsPage.PNG?token=ANBFY5KYWONBYAKLN76PJALAQRP52" width="250" height="400"/>

## Requirements

- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- [Azure Portal Account](https://portal.azure.com/#home)

This application uses the following technologies:
- [Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/)
- [Computer Vision Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/)

To set up your cognitivite services account please head over to your [azure portal](https://portal.azure.com/#home) and click create a resource. Search for the following two items and create new instances of the services:

- Computer Vision
- Face

### Getting your API

When you have created your resource you will see an instance of Computer Vision Service/Face.

Click on the go to resource button to see the detail of the instance that you ahve created.

On the left side of your menu you will see a Keys and Endpoint.

<img src="https://i.imgur.com/AoSl7bJ.png" width="100" height="250" />

You will be redirected to this page, where it provides you with two masked keys and an endpoint.

Copy one key and the endpoint as we will be using these in our project. Remember to do it for both Computer Vision and Face.


### Downloading the project

Start by cloning the repo onto your desktop.

```
git clone https://github.com/tan00060/XamarinWindowsFinal.git
```

### Adding your API to the project.

Open your project in Visual Studio 2019.

In your MainPage.xaml.cs file in your field region, you will notice that we have added our APIKEY and ENDPOINT as private const that we can use within our application. Add your newly generated keys into these regions.

<img src="https://i.imgur.com/Ao0rhvm.jpg" height="200" width="500" />

Once you have saved your the file with your apikeys and endpoints you can build the project.

### Contact

If you anyone questions please contact me at [e-mail](mailto:tan00060@algonquinlive.com)
