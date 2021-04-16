using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MichaelTanWindowFinal
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        #region constructor
        public SettingsPage()
        {
            InitializeComponent();
            speechSwitch.IsToggled = App.UseSpeech;
            faceDataSwitch.IsToggled = App.GetFaceData;

            DisplayDeviceInfo();
            DisplayAppInfo();
        }

        #endregion

        #region Method
        private void DisplayAppInfo()
        {
            //display version and build
            string infoString = $"Application: {AppInfo.Name}{Environment.NewLine}Version: {AppInfo.Version}{Environment.NewLine}Build: {AppInfo.BuildString}";
            appInformation.Text = infoString;
        }

        private void DisplayDeviceInfo()
        {
            string infoString = $"Manufacturer: {DeviceInfo.Manufacturer}{Environment.NewLine}Model: {DeviceInfo.Model}{Environment.NewLine}Name: {DeviceInfo.Name}{Environment.NewLine}Idiom: {DeviceInfo.Idiom}{Environment.NewLine}Device Type: {DeviceInfo.DeviceType}{Environment.NewLine}Platform: {DeviceInfo.Platform}";
            deviceInformation.Text = infoString;
        }

        #endregion

        #region Button and switch methods
        private void AppSettingsButton_Clicked(object sender, EventArgs e)
        {
            AppInfo.ShowSettingsUI();
        }

        private void FaceDataSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            App.GetFaceData = faceDataSwitch.IsToggled;
        }

        private void SpeechSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            App.UseSpeech = speechSwitch.IsToggled;
        }

        #endregion
    }
}