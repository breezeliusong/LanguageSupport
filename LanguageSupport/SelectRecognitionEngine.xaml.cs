using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LanguageSupport
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectRecognitionEngine : Page
    {
        public SelectRecognitionEngine()
        {
            this.InitializeComponent();
            // Set supported inking device types.
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

            // Set initial ink stroke attributes.
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);

            // Populate the recognizer combo box with installed recognizers.
            InitializeRecognizerList();

            // Listen for combo box selection.
            comboInstalledRecognizers.SelectionChanged +=
                comboInstalledRecognizers_SelectionChanged;

            // Listen for button click to initiate recognition.
            buttonRecognize.Click += Recognize_Click;

        }
        private InkRecognizerContainer inkRecognizerContainer;

        // Populate the recognizer combo box with installed recognizers.
        private void InitializeRecognizerList()
        {
            // Create a manager for the handwriting recognition process.
            inkRecognizerContainer = new InkRecognizerContainer();
            // Retrieve the collection of installed handwriting recognizers.
            IReadOnlyList<InkRecognizer> installedRecognizers =
                inkRecognizerContainer.GetRecognizers();
            Debug.WriteLine(installedRecognizers.Count);
            // inkRecognizerContainer is null if a recognition engine is not available.
            if (!(inkRecognizerContainer == null))
            {
                comboInstalledRecognizers.ItemsSource = installedRecognizers;
                buttonRecognize.IsEnabled = true;
            }
        }

        // Handle recognizer change.
        private void comboInstalledRecognizers_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            inkRecognizerContainer.SetDefaultRecognizer(
                (InkRecognizer)comboInstalledRecognizers.SelectedItem);
        }

        //change language in c# code
        private async void ChangeLagButton_Click(object sender, RoutedEventArgs e)
        {
            var list = Windows.Globalization.ApplicationLanguages.ManifestLanguages;
            Debug.Write(list.Count);
            if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride=="en-US")
            {
                TestButton.Content = "HELLO english";
                var culture = new System.Globalization.CultureInfo("pl");
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
                //
                await Task.Delay(100);
                //To refresh the UI without restart the phone
                this.Frame.Navigate(this.GetType());
            }else if(Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride == "zh-CN")
            {
                TestButton.Content = "HELLO china";
            }
        }

        // Handle button click to initiate recognition.
        private async void Recognize_Click(object sender, RoutedEventArgs e)
        {
            // Get all strokes on the InkCanvas.
            IReadOnlyList<InkStroke> currentStrokes =
                inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Ensure an ink stroke is present.
            if (currentStrokes.Count > 0)
            {
                // inkRecognizerContainer is null if a recognition engine is not available.
                if (!(inkRecognizerContainer == null))
                {
                    // Recognize all ink strokes on the ink canvas.
                    IReadOnlyList<InkRecognitionResult> recognitionResults =
                        await inkRecognizerContainer.RecognizeAsync(
                            inkCanvas.InkPresenter.StrokeContainer,
                            InkRecognitionTarget.All);
                    // Process and display the recognition results.
                    if (recognitionResults.Count > 0)
                    {
                        string str = "Recognition result\n";
                        // Iterate through the recognition results.
                        foreach (InkRecognitionResult result in recognitionResults)
                        {
                            // Get all recognition candidates from each recognition result.
                            IReadOnlyList<string> candidates =
                                result.GetTextCandidates();
                            str += "Candidates: " + candidates.Count.ToString() + "\n";
                            foreach (string candidate in candidates)
                            {
                                str += candidate + " ";
                            }
                        }
                        // Display the recognition candidates.
                        recognitionResult.Text = str;
                        // Clear the ink canvas once recognition is complete.
                        inkCanvas.InkPresenter.StrokeContainer.Clear();
                    }
                    else
                    {
                        recognitionResult.Text = "No recognition results.";
                    }
                }
                else
                {
                    Windows.UI.Popups.MessageDialog messageDialog =
                        new Windows.UI.Popups.MessageDialog(
                            "You must install handwriting recognition engine.");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                recognitionResult.Text = "No ink strokes to recognize.";
            }
        }
    }
}
