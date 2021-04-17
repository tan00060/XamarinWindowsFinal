using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using SkiaSharp;

namespace MichaelTanWindowFinal
{
    public partial class MainPage : ContentPage
    {

        #region Fields
        const string DELIMITER = ", ";
        private readonly float density = 1.0f;
        private int sourceImageWidth = 0;
        private int sourceImageHeight = 0;
        private float displayedImageHieght = 0;
        private float displayedImageWidth = 0;
        private float scale = 1.0f;
        private int leftPaddingAdjustment = 0;
        private int topPaddingAdjustment = 0;
        private const string BASEURL = "https://source.unsplash.com/random/";
        private const string PORTRAITRESOLUTION = "960x1280";
        private const string LANDSCAPERESOLUTION = "1280x960";
        private const string APIKEY = "";
        private const string ENDPOINT = "";
        private const string FACEAPIKEY = "";
        private const string FACEENDPOINT = "";
        private readonly List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
        {
            VisualFeatureTypes.Categories, VisualFeatureTypes.Description, VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType, VisualFeatureTypes.Tags
        };
        private CancellationTokenSource cts = null;
        private readonly FaceAttributeType?[] faceAttributes =
        {
            FaceAttributeType.Age,
            FaceAttributeType.Gender,
            FaceAttributeType.Emotion,
            FaceAttributeType.Smile,
            FaceAttributeType.Hair,
            FaceAttributeType.Glasses,
            FaceAttributeType.Accessories
        };
        Microsoft.Azure.CognitiveServices.Vision.Face.Models.FaceRectangle[] faceRectangles = null;
        private string[] faceDescriptions;
        private MemoryStream faceImageMemeoryStream = new MemoryStream();
        #endregion

        #region constructor
        public MainPage()
        {
            InitializeComponent();
            if(DeviceInfo.Platform == DevicePlatform.UWP)
            {
                theActivityIndicator.Scale = 3.0;
            }

            if(DeviceInfo.Platform == DevicePlatform.iOS)
            {
                theActivityIndicator.Scale = 2.0;
            }

            density = (float)DeviceDisplay.MainDisplayInfo.Density;
        }

        #endregion

        #region Buttons

        private async void webImageButton_Clicked(object sender, EventArgs e)
        {
            theActivityIndicator.IsRunning = true;
            Uri webImageUri = new Uri($"{BASEURL}{PORTRAITRESOLUTION}");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetStreamAsync(webImageUri))
                    {
                        var memoryStream = new MemoryStream();
                        await response.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        try
                        {
                            var result = await GetImageDescription(memoryStream);
                            ProcessImageResults(result);
                            memoryStream.Position = 0;
                            theImage.Source = ImageSource.FromStream(() => memoryStream);
                        }
                        catch (ComputerVisionErrorException ex)
                        {
                            theResults.Text = ex.Message;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                theResults.Text = "Failed to load image: " + ex.Message;
            }
            theActivityIndicator.IsRunning = false;
        }

        private async void imageButton_Clicked(object sender, EventArgs e)
        {
            var file = await MediaPicker.PickPhotoAsync (new MediaPickerOptions {
                Title = "Please choose an image"
            });

            if(file != null)
            {

                theActivityIndicator.IsRunning = true;

                try
                {
                    var stream = await file.OpenReadAsync();
                    var result = await GetImageDescription(stream);
                    ProcessImageResults(result);
                    theImage.Source = ImageSource.FromStream(() => stream);
                }
                catch(ComputerVisionErrorException ex)
                {
                    theResults.Text = $"{ex.Message}{Environment.NewLine}{ex.Response.Content}";

                }
                catch(Exception ex)
                {
                    theResults.Text = ex.Message;
                }
                theActivityIndicator.IsRunning = false;
            }
        }

        private async void cameraButton_Clicked(object sender, EventArgs e)
        {
            if (!MediaPicker.IsCaptureSupported)
            {
                await DisplayAlert("No Camera", ":( No Camera Available.", "OK");
                return;
            }

            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Camera Permission", "Please Enable Your Camera Permissions.", "OK");
                status = await Permissions.RequestAsync<Permissions.Camera>();

                if(status != PermissionStatus.Granted)
                {
                    return;
                }
            }

            var file = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions {
                Title = "Please Take a photo!" 
            });

            if(file == null)
            {
                return;
            }

            theActivityIndicator.IsRunning = true;

            try
            {
                var stream = await file.OpenReadAsync();
                var result = await GetImageDescription(stream);
                ProcessImageResults(result);
                theImage.Source = ImageSource.FromStream(() => stream);
            }
            catch (ComputerVisionErrorException ex)
            {
                theResults.Text = $"{ex.Message}{Environment.NewLine}{ex.Response.Content}";

            }
            catch (Exception ex)
            {
                theResults.Text = ex.Message;
            }
            theActivityIndicator.IsRunning = false;

        }

        private void faceButton_Clicked(object sender, EventArgs e)
        {
            GetFaceData();
        }

        private async void settingsButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        #endregion

        #region Image Method
        private async Task<ImageAnalysis> GetImageDescription(Stream imageStream)
        {
            ComputerVisionClient visionClient = new ComputerVisionClient(
                new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(APIKEY), new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = ENDPOINT
            };

            MemoryStream memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            imageStream.Position = 0;

            ResetFaceUiAndData();

            var results = await visionClient.AnalyzeImageInStreamAsync(memoryStream, features, null);

            if(results.Faces.Count > 0 && App.GetFaceData)
            {
                faceButton.IsEnabled = true;
                faceButton.IsVisible = true;

                await imageStream.CopyToAsync(faceImageMemeoryStream);
                faceImageMemeoryStream.Position = 0;
                imageStream.Position = 0;
            }

            sourceImageWidth = results.Metadata.Width;
            sourceImageHeight = results.Metadata.Height;
            return results;
        }

        private void ProcessImageResults(ImageAnalysis result)
        {
             if(result.Description.Captions.Count != 0)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach(var item in result.Description.Captions)
                {
                    stringBuilder.Append(item.Text);
                    var confidence = item.Confidence;
                    stringBuilder.Append($"{Environment.NewLine}Confidence: {Math.Round(item.Confidence, 2) * 100} % {Environment.NewLine}");
                }

                foreach(var tag in result.Tags)
                {
                    stringBuilder.Append(tag.Name + ", ");
                }


                theResults.Text = stringBuilder.ToString().TrimEnd(' ').TrimEnd(',');
            }
        }

        #endregion

        #region Speak Methods
        private async void Speak()
        {
            cts = new CancellationTokenSource();
            await TextToSpeech.SpeakAsync(theResults.Text, cts.Token);
        }

        private void CancelSpeak()
        {
            if(cts == null)
            {
                return;
            }

            cts.Cancel();
            cts = null;
        }

        private void theResults_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if( !App.UseSpeech || e.PropertyName != "Text")
            {
                return;
            }

            if (!string.IsNullOrEmpty(theResults.Text))
            {
                CancelSpeak();
                Speak();
            }
        }

        #endregion

        #region Face Methods
        private void ResetFaceUiAndData()
        {
            faceRectangles = null;
            mainCanvasControl.InvalidateSurface();
            faceButton.IsEnabled = false;
            faceButton.IsVisible = false;

        }

        private async void GetFaceData()
        {
            FaceClient faceClient = new FaceClient(
                new Microsoft.Azure.CognitiveServices.Vision.Face.ApiKeyServiceClientCredentials(FACEAPIKEY),
                new DelegatingHandler[] { })
            {
                Endpoint = FACEENDPOINT
            };

            MemoryStream memoryStream = new MemoryStream();
            await faceImageMemeoryStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            faceImageMemeoryStream.Position = 0;

            theActivityIndicator.IsRunning = true;

            try
            {
                IList<DetectedFace> faceList = await faceClient.Face.DetectWithStreamAsync(memoryStream, true, false, faceAttributes);

                if(faceList != null)
                {
                    var faceRects = faceList.Select(face => face.FaceRectangle);
                    faceRectangles = faceRects.ToArray();

                    faceDescriptions = new string[faceList.Count];
                    
                    for(int i = 0; i < faceList.Count; i++)
                    {
                        faceDescriptions[i] = GenerateFaceDescription(faceList[i]);
                    }

                    mainCanvasControl.InvalidateSurface();
                }
                else
                {
                    theResults.Text = "Unable to detect face(s).";
                }
            }




            catch(APIErrorException ex)
            {
                theResults.Text = $"{ex.Message}{Environment.NewLine}{ex.Response.Content}";
            }
            catch(Exception ex)
            {
                theResults.Text = ex.Message;
            }

            theActivityIndicator.IsRunning = false;
        }

        private string GenerateFaceDescription(DetectedFace detectedFace)
        {
            StringBuilder stringBuilder = new StringBuilder();

            //gender age and smile

           
            stringBuilder.Append("Face: ");
            stringBuilder.Append($"Gender: {detectedFace.FaceAttributes.Gender}{DELIMITER}");
            stringBuilder.Append($"Age: {detectedFace.FaceAttributes.Age}{DELIMITER}");
            stringBuilder.Append($"Smile: {detectedFace.FaceAttributes.Smile * 100:F1}%{DELIMITER}");
            stringBuilder.Append($"Emotion: {detectedFace.FaceAttributes.Emotion.Neutral * 100:F1}%{DELIMITER}");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(GetEmotion(detectedFace.FaceAttributes.Emotion));
            stringBuilder.Append(GetHairColor(detectedFace.FaceAttributes.Hair));
            stringBuilder.Append($"Glasses: {detectedFace.FaceAttributes.Glasses}{DELIMITER}");
            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();


        }

        private string GetEmotion(Emotion emotion)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Get emotion on the face
            string emotionType = string.Empty;
            double emotionValue = 0.1;

            if (emotion.Anger > emotionValue) { emotionValue = emotion.Anger; emotionType = "Anger"; }
            if (emotion.Contempt > emotionValue) { emotionValue = emotion.Contempt; emotionType = "Contempt"; }
            if (emotion.Disgust > emotionValue) { emotionValue = emotion.Disgust; emotionType = "Disgust"; }
            if (emotion.Fear > emotionValue) { emotionValue = emotion.Fear; emotionType = "Fear"; }
            if (emotion.Happiness > emotionValue) { emotionValue = emotion.Happiness; emotionType = "Happiness"; }
            if (emotion.Neutral > emotionValue) { emotionValue = emotion.Neutral; emotionType = "Neutral"; }
            if (emotion.Sadness > emotionValue) { emotionValue = emotion.Sadness; emotionType = "Sadness"; }
            if (emotion.Surprise > emotionValue) { emotionType = "Surprise"; }

            stringBuilder.Append($"Emotion: {emotionType} {emotionValue * 100} %{DELIMITER}");

            return stringBuilder.ToString();
        }

        private string GetHairColor(Hair hair)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string color = string.Empty;
            if(hair.HairColor.Count == 0)
            {
                if (hair.Invisible)
                {
                    color = "Invisabile";
                }
                else
                {
                    color = "Bald";
                }
            }
            HairColorType returnColor = HairColorType.Unknown;
            double maxConfidence = 0.0f;
            foreach (HairColor haircolor in hair.HairColor)
            {
                if(haircolor.Confidence <= maxConfidence) { continue; }
                maxConfidence = haircolor.Confidence; returnColor = haircolor.Color; color = returnColor.ToString();
            }
            stringBuilder.Append($"Hair: {color}{DELIMITER}");
            return stringBuilder.ToString();
        }
        #endregion

        #region Canvas Methods
        private void mainCanvasControl_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if(faceRectangles == null)
            {
                return;
            }

            leftPaddingAdjustment = (int)((mainCanvasControl.Width - displayedImageWidth) / 2 * density);
            topPaddingAdjustment = (int)((mainCanvasControl.Height - displayedImageHieght) / 2 * density);


            float strokeWidth = 8.0f;
            SKPaint redStrokePaint = new SKPaint()
            {
                StrokeWidth = strokeWidth,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            if(faceRectangles.Length > 0)
            {
                foreach(var faceRect in faceRectangles)
                {
                    canvas.DrawRect((faceRect.Left * scale + leftPaddingAdjustment), (faceRect.Top * scale + topPaddingAdjustment), (faceRect.Width * scale), (faceRect.Height *scale), redStrokePaint);
                }
            };
        }

        private void mainCanvasControl_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {
            if (faceRectangles == null || e.MouseButton != SkiaSharp.Views.Forms.SKMouseButton.Left || e.ActionType != SkiaSharp.Views.Forms.SKTouchAction.Pressed)
            {
                return;
            }

            SKPoint tappedPointXY = e.Location;

            for(int i = 0; i < faceRectangles.Length; i++)
            {
                var faceRect = faceRectangles[i];
                double left = faceRect.Left * scale + leftPaddingAdjustment;
                double top = faceRect.Top * scale + topPaddingAdjustment;
                double width = faceRect.Width * scale;
                double height = faceRect.Height * scale;

                if (tappedPointXY.X >= left && tappedPointXY.X <= left + width
                    && tappedPointXY.Y >= top && tappedPointXY.Y <= top + height)
                {
                    e.Handled = true;
                    DisplayAlert("Face Data", faceDescriptions[i], "OK");
                }
            };
            
        }
      

        private void theImage_SizeChanged(object sender, EventArgs e)
        {
            displayedImageWidth = (float)theImage.Width;
            displayedImageHieght = (float)theImage.Height;

            scale = displayedImageWidth / sourceImageWidth;
            scale *= density;
        }
#endregion
    }

}
