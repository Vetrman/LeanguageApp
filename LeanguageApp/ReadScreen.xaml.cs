using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeanguageApp
{
    public partial class ReadScreen : Page
    {
        private List<Flashcard> flashcards;
        private int currentCardIndex = 0;
        private bool isFlipped = false;
        private string sourceImagePath = "";
        private Flashcard cardToDelete = null;

        public class Flashcard
        {
            public string ImageFileName { get; set; }
            public string FrontText { get; set; }
            public string BackText { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public ReadScreen()
        {
            InitializeComponent();
            InitializeFlashcards();
            ShowCurrentCard();
            UpdateFlipButton();
        }

        // ========== ОСНОВНЫЕ МЕТОДЫ КАРТОЧЕК ==========
        private void InitializeFlashcards()
        {
            flashcards = new List<Flashcard>();
            LoadFlashcardsFromFile();
        }

        private void LoadFlashcardsFromFile()
        {
            try
            {
                if (File.Exists(App.FlashcardsFile))
                {
                    string json = File.ReadAllText(App.FlashcardsFile);
                    flashcards = JsonSerializer.Deserialize<List<Flashcard>>(json);
                }
            }
            catch (Exception ex)
            {
                ShowMessageDialog($"Error loading flashcards: {ex.Message}");
            }
        }

        private void SaveFlashcardsToFile()
        {
            try
            {
                string json = JsonSerializer.Serialize(flashcards, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(App.FlashcardsFile, json);
            }
            catch (Exception ex)
            {
                ShowMessageDialog($"Error saving flashcards: {ex.Message}");
            }
        }

        private void ShowCurrentCard()
        {
            if (flashcards.Count == 0)
            {
                FrontText.Text = "No flashcards available";
                BackText.Text = "Click 'Add Card' to create your first flashcard!";
                CardImage.Visibility = Visibility.Collapsed;
                return;
            }

            var card = flashcards[currentCardIndex];

            FrontText.Text = card.FrontText;
            BackText.Text = card.BackText;

            if (!string.IsNullOrEmpty(card.ImageFileName))
            {
                string imagePath = Path.Combine(App.ImagesFolder, card.ImageFileName);
                if (File.Exists(imagePath))
                {
                    try
                    {
                        CardImage.Source = new BitmapImage(new Uri(imagePath));
                        CardImage.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        CardImage.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    CardImage.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                CardImage.Visibility = Visibility.Collapsed;
            }

            ResetCardFlip();
            UpdateCounter();
            UpdateNavigationButtons();
            UpdateFlipButton();
        }

        private void UpdateNavigationButtons()
        {
            btnPrev.IsEnabled = flashcards.Count > 1;
            btnNext.IsEnabled = flashcards.Count > 1;
            btnFlip.IsEnabled = flashcards.Count > 0;
            btnAddCard.IsEnabled = true;
            btnViewAll.IsEnabled = flashcards.Count > 0;
            btnDelete.IsEnabled = flashcards.Count > 0;
        }

        private void UpdateCounter()
        {
            txtCounter.Text = $"{currentCardIndex + 1}/{flashcards.Count}";
        }

        private void ResetCardFlip()
        {
            isFlipped = false;
            FrontCard.Opacity = 1;
            BackCard.Opacity = 0;
            UpdateFlipButton();
        }

        // ========== ОБНОВЛЕННАЯ ЛОГИКА КНОПКИ FLIP ==========
        private void UpdateFlipButton()
        {
            if (isFlipped)
            {
                btnFlip.Content = "❌ Close Card";
                btnFlip.Background = new SolidColorBrush(Color.FromRgb(211, 47, 47));
            }
            else
            {
                btnFlip.Content = "🔄 Flip Card";
                btnFlip.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
            }
        }

        private void FlipCard()
        {
            if (flashcards.Count == 0) return;

            var animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            if (!isFlipped)
            {
                animation.To = 0;
                FrontCard.BeginAnimation(OpacityProperty, animation);
                animation.To = 1;
                BackCard.BeginAnimation(OpacityProperty, animation);
            }
            else
            {
                animation.To = 0;
                BackCard.BeginAnimation(OpacityProperty, animation);
                animation.To = 1;
                FrontCard.BeginAnimation(OpacityProperty, animation);
            }

            isFlipped = !isFlipped;
            UpdateFlipButton();
        }

        // ========== ДИАЛОГИ УДАЛЕНИЯ И УСПЕХА ==========
        private void ShowDeleteConfirmDialog(Flashcard card)
        {
            cardToDelete = card;
            DeleteConfirmText.Text = $"Are you sure you want to delete '{card.FrontText}'?";

            DeleteConfirmDialog.Visibility = Visibility.Visible;
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            DeleteConfirmDialog.BeginAnimation(OpacityProperty, animation);

            CardArea.Opacity = 0.3;
        }

        private void HideDeleteConfirmDialog()
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, e) =>
            {
                DeleteConfirmDialog.Visibility = Visibility.Collapsed;
                CardArea.Opacity = 1;
                cardToDelete = null;
            };

            DeleteConfirmDialog.BeginAnimation(OpacityProperty, animation);
        }

        private async void ShowSuccessDialog(string message)
        {
            SuccessText.Text = message;

            SuccessDialog.Visibility = Visibility.Visible;
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            SuccessDialog.BeginAnimation(OpacityProperty, animation);

            await Task.Delay(2000);
            HideSuccessDialog();
        }

        private void HideSuccessDialog()
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, e) =>
            {
                SuccessDialog.Visibility = Visibility.Collapsed;
            };

            SuccessDialog.BeginAnimation(OpacityProperty, animation);
        }

        private void DeleteCard(Flashcard card)
        {
            if (!string.IsNullOrEmpty(card.ImageFileName))
            {
                string imagePath = Path.Combine(App.ImagesFolder, card.ImageFileName);
                if (File.Exists(imagePath))
                {
                    try { File.Delete(imagePath); } catch { }
                }
            }

            flashcards.Remove(card);
            SaveFlashcardsToFile();

            if (currentCardIndex >= flashcards.Count)
                currentCardIndex = Math.Max(0, flashcards.Count - 1);

            ShowCurrentCard();

            ShowSuccessDialog("Card deleted successfully!");
        }

        private void DeleteCurrentCard()
        {
            if (flashcards.Count == 0) return;
            ShowDeleteConfirmDialog(flashcards[currentCardIndex]);
        }

        // ========== ФОРМА ДОБАВЛЕНИЯ КАРТОЧКИ ==========
        private void ShowAddCardForm()
        {
            txtFront.Text = "";
            txtBack.Text = "";
            txtImagePath.Text = "";
            imgPreview.Source = null;
            imgPreview.Visibility = Visibility.Collapsed;
            sourceImagePath = "";

            ShowFormWithAnimation(AddCardForm);
        }

        private void HideAddCardForm()
        {
            HideFormWithAnimation(AddCardForm, () =>
            {
                CardArea.Opacity = 1;
                UpdateNavigationButtons();
            });
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
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
                }
                catch (Exception ex)
                {
                    ShowMessageDialog($"Error loading image: {ex.Message}");
                    imgPreview.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SaveNewFlashcard()
        {
            if (string.IsNullOrWhiteSpace(txtFront.Text))
            {
                ShowMessageDialog("Please fill in the front side of the card (word/phrase).");
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
                    ShowMessageDialog($"Error copying image: {ex.Message}");
                    return;
                }
            }

            var newCard = new Flashcard
            {
                FrontText = txtFront.Text.Trim(),
                BackText = txtBack.Text.Trim(),
                ImageFileName = imageFileName,
                CreatedDate = DateTime.Now
            };

            flashcards.Add(newCard);
            currentCardIndex = flashcards.Count - 1;
            SaveFlashcardsToFile();

            HideAddCardForm();
            ShowCurrentCard();

            ShowSuccessDialog("Card added successfully!");
        }

        // ========== СПИСОК ВСЕХ КАРТОЧЕК ==========
        private void ShowCardsList()
        {
            CardsList.ItemsSource = null;
            CardsList.ItemsSource = flashcards;
            ShowFormWithAnimation(CardsListForm);
        }

        private void HideCardsList()
        {
            HideFormWithAnimation(CardsListForm, () =>
            {
                CardArea.Opacity = 1;
                UpdateNavigationButtons();
            });
        }

        private void BtnViewCard_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Flashcard card)
            {
                currentCardIndex = flashcards.IndexOf(card);
                HideCardsList();
                ShowCurrentCard();
            }
        }

        private void BtnDeleteFromList_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Flashcard card)
            {
                ShowDeleteConfirmDialog(card);
            }
        }

        // ========== АНИМАЦИИ ==========
        private void ShowFormWithAnimation(FrameworkElement form)
        {
            form.Visibility = Visibility.Visible;
            CardArea.Opacity = 0;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            form.BeginAnimation(OpacityProperty, animation);

            btnPrev.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnFlip.IsEnabled = false;
            btnAddCard.IsEnabled = false;
            btnViewAll.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private void HideFormWithAnimation(FrameworkElement form, Action onCompleted = null)
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, e) =>
            {
                form.Visibility = Visibility.Collapsed;
                onCompleted?.Invoke();
            };

            form.BeginAnimation(OpacityProperty, animation);
        }

        // ========== КАСТОМНЫЙ ДИАЛОГ ДЛЯ СООБЩЕНИЙ ==========
        private void ShowMessageDialog(string message)
        {
            MessageDialogText.Text = message;
            MessageDialog.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            MessageDialog.BeginAnimation(OpacityProperty, animation);

            CardArea.Opacity = 0.3;
        }

        private void HideMessageDialog()
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, e) =>
            {
                MessageDialog.Visibility = Visibility.Collapsed;
                CardArea.Opacity = 1;
            };

            MessageDialog.BeginAnimation(OpacityProperty, animation);
        }

        // ========== КЛИКИ МЫШКОЙ ==========
        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                if (CardsListForm.Visibility != Visibility.Visible &&
                    AddCardForm.Visibility != Visibility.Visible &&
                    DeleteConfirmDialog.Visibility != Visibility.Visible &&
                    MessageDialog.Visibility != Visibility.Visible)
                {
                    ShowCardsList();
                }
            }
        }

        private void CardArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                if (CardsListForm.Visibility != Visibility.Visible &&
                    AddCardForm.Visibility != Visibility.Visible &&
                    DeleteConfirmDialog.Visibility != Visibility.Visible &&
                    MessageDialog.Visibility != Visibility.Visible)
                {
                    ShowCardsList();
                }
                e.Handled = true;
            }
        }

        // ========== ГОРЯЧИЕ КЛАВИШИ ==========
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Left:
                    BtnPrev_Click(null, null);
                    break;
                case Key.Right:
                    BtnNext_Click(null, null);
                    break;
                case Key.Space:
                    FlipCard();
                    break;
                case Key.Delete:
                    if (DeleteConfirmDialog.Visibility != Visibility.Visible)
                        DeleteCurrentCard();
                    break;
                case Key.Escape:
                    if (DeleteConfirmDialog.Visibility == Visibility.Visible)
                        HideDeleteConfirmDialog();
                    else if (MessageDialog.Visibility == Visibility.Visible)
                        HideMessageDialog();
                    else if (isFlipped)
                        FlipCard();
                    break;
            }
        }

        // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========
        private void BtnFlip_Click(object sender, RoutedEventArgs e) => FlipCard();

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (flashcards.Count == 0) return;
            currentCardIndex = (currentCardIndex - 1 + flashcards.Count) % flashcards.Count;
            ShowCurrentCard();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (flashcards.Count == 0) return;
            currentCardIndex = (currentCardIndex + 1) % flashcards.Count;
            ShowCurrentCard();
        }

        private void BtnAddCard_Click(object sender, RoutedEventArgs e) => ShowAddCardForm();

        private void BtnCancelAdd_Click(object sender, RoutedEventArgs e) => HideAddCardForm();

        private void BtnSaveCard_Click(object sender, RoutedEventArgs e) => SaveNewFlashcard();

        private void BtnViewAll_Click(object sender, RoutedEventArgs e) => ShowCardsList();

        private void BtnBackFromList_Click(object sender, RoutedEventArgs e) => HideCardsList();

        private void BtnDelete_Click(object sender, RoutedEventArgs e) => DeleteCurrentCard();

        private void BtnCancelDelete_Click(object sender, RoutedEventArgs e) => HideDeleteConfirmDialog();

        private void BtnConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cardToDelete != null)
            {
                DeleteCard(cardToDelete);
                HideDeleteConfirmDialog();
            }
        }

        private void BtnSuccessOK_Click(object sender, RoutedEventArgs e) => HideSuccessDialog();

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainScreen mainScreen)
            {
                mainScreen.LoadMainMenu();
            }
        }

        private void CardGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FlipCard();
        }

        private void MessageDialogOK_Click(object sender, RoutedEventArgs e)
        {
            HideMessageDialog();
        }
    }
}