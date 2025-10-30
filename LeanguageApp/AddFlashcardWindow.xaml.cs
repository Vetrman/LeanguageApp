using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace LeanguageApp
{
    public partial class AddFlashcardWindow : Window
    {
        public ReadScreen.Flashcard NewFlashcard { get; private set; }
        private string sourceImagePath = "";

        public AddFlashcardWindow()
        {
            InitializeComponent();
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select an image for the flashcard"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                sourceImagePath = openFileDialog.FileName;
                txtImagePath.Text = Path.GetFileName(sourceImagePath);

                try
                {
                    var image = new BitmapImage(new Uri(sourceImagePath));
                    imgPreview.Source = image;
                    imgPreview.Visibility = Visibility.Visible;

                    ShowMessageDialog("Image loaded successfully");
                }
                catch (Exception ex)
                {
                    ShowMessageDialog($"Error loading image: {ex.Message}");
                }
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFront.Text) || string.IsNullOrWhiteSpace(txtBack.Text))
            {
                ShowMessageDialog("Please fill in both front and back sides of the card");
                return;
            }

            string imageFileName = null;

            if (!string.IsNullOrEmpty(sourceImagePath))
            {
                try
                {
                    string extension = Path.GetExtension(sourceImagePath);
                    imageFileName = $"{Guid.NewGuid()}{extension}";
                    string destPath = Path.Combine(App.ImagesFolder, imageFileName);
                    File.Copy(sourceImagePath, destPath, true);
                }
                catch (Exception ex)
                {
                    ShowMessageDialog($"Error saving image: {ex.Message}");
                    return;
                }
            }

            NewFlashcard = new ReadScreen.Flashcard
            {
                FrontText = txtFront.Text.Trim(),
                BackText = txtBack.Text.Trim(),
                ImageFileName = imageFileName,
                CreatedDate = DateTime.Now
            };

            ShowMessageDialog("Card created successfully");

            await Task.Delay(800);
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtFront.Text) ||
                !string.IsNullOrWhiteSpace(txtBack.Text) ||
                !string.IsNullOrEmpty(sourceImagePath))
            {
                ShowConfirmDialog("You have unsaved changes. Are you sure you want to cancel?",
                    result =>
                    {
                        if (result)
                        {
                            DialogResult = false;
                            Close();
                        }
                    });
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }



        private void ShowMessageDialog(string message)
        {
            DialogMessageText.Text = message;
            DialogOKButton.Visibility = Visibility.Visible;
            DialogYesButton.Visibility = Visibility.Collapsed;
            DialogNoButton.Visibility = Visibility.Collapsed;

            ShowDialog();
        }

        private void ShowConfirmDialog(string message, Action<bool> callback)
        {
            DialogMessageText.Text = message;
            DialogOKButton.Visibility = Visibility.Collapsed;
            DialogYesButton.Visibility = Visibility.Visible;
            DialogNoButton.Visibility = Visibility.Visible;
            _confirmCallback = callback;

            ShowDialog();
        }

        private void ShowDialog()
        {
            DialogOverlay.Visibility = Visibility.Visible;
            MessageDialog.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
            DialogOverlay.BeginAnimation(OpacityProperty, animation);
            MessageDialog.BeginAnimation(OpacityProperty, animation);
        }

        private void HideDialog()
        {
            var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            animation.Completed += (s, e) =>
            {
                DialogOverlay.Visibility = Visibility.Collapsed;
                MessageDialog.Visibility = Visibility.Collapsed;
            };
            DialogOverlay.BeginAnimation(OpacityProperty, animation);
            MessageDialog.BeginAnimation(OpacityProperty, animation);
        }

     

        private Action<bool> _confirmCallback;

        private void DialogOK_Click(object sender, RoutedEventArgs e)
        {
            HideDialog();
        }

        private void DialogYes_Click(object sender, RoutedEventArgs e)
        {
            HideDialog();
            _confirmCallback?.Invoke(true);
        }

        private void DialogNo_Click(object sender, RoutedEventArgs e)
        {
            HideDialog();
            _confirmCallback?.Invoke(false);
        }
    }
}
