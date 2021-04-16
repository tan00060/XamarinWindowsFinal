using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ExportFont("fa-regular-400.ttf", Alias = "FA-R")]
[assembly: ExportFont("fa-solid-900.ttf", Alias = "FA-S")]
[assembly: ExportFont("fa-brands-400.ttf", Alias = "FA-B")]

namespace MichaelTanWindowFinal
{
    public partial class App : Application
    {

        public static bool UseSpeech { get; set; } = true;
        public static bool GetFaceData { get; set; } = true;


        public App()
        {
            InitializeComponent();

            //MainPage = new MainPage();
            MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.FromHex("#2196f3"),
                BarTextColor = Color.White
            };
        }

        protected override void OnStart()
        {
            UseSpeech = Preferences.Get("UseSpeech", true);
            GetFaceData = Preferences.Get("GetFaceData", true);
        }

        protected override void OnSleep()
        {
            Preferences.Set("UseSpeech", UseSpeech);
            Preferences.Set("GetFaceData", GetFaceData);
        }

        protected override void OnResume()
        {
        }
    }
}
