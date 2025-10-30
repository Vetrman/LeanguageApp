using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace LeanguageApp
{
    public partial class MainScreen : Window
    {
        public MainScreen()
        {
            InitializeComponent();

            // Загружаем главное меню при запуске
            LoadMainMenu();
        }

        public void LoadMainMenu()
        {
            // Создаем страницу главного меню
            var mainMenuPage = new Page();
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));

            // Основной контейнер
            var stackPanel = new StackPanel();
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            stackPanel.Margin = new Thickness(40);

            // Заголовок
            var titleText = new TextBlock();
            titleText.Text = "🌟 Language Learning App";
            titleText.Foreground = Brushes.White;
            titleText.FontSize = 32;
            titleText.FontWeight = FontWeights.Bold;
            titleText.TextAlignment = TextAlignment.Center;
            titleText.Margin = new Thickness(0, 0, 0, 30);

            // Описание
            var descriptionText = new TextBlock();
            descriptionText.Text = "Добро пожаловать в приложение для изучения языков!\n\n" +
                                 "Выберите один из разделов для начала обучения:\n\n" +
                                 "• 📖 Reading - практика чтения и понимания текстов\n" +
                                 "• 🎤 Speaking - развитие навыков говорения\n" +
                                 "• 🔊 Listening - тренировка восприятия на слух\n" +
                                 "• ✏️ Writing - улучшение письменных навыков";
            descriptionText.Foreground = Brushes.LightGray;
            descriptionText.FontSize = 16;
            descriptionText.TextAlignment = TextAlignment.Center;
            descriptionText.TextWrapping = TextWrapping.Wrap;
            descriptionText.Margin = new Thickness(0, 0, 0, 40);
            descriptionText.LineHeight = 24;

            // Инструкция
            var instructionText = new TextBlock();
            instructionText.Text = "Нажмите на кнопку выше, чтобы начать обучение!";
            instructionText.Foreground = Brushes.Gray;
            instructionText.FontSize = 14;
            instructionText.FontStyle = FontStyles.Italic;
            instructionText.TextAlignment = TextAlignment.Center;
            instructionText.Margin = new Thickness(0, 20, 0, 0);

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descriptionText);
            stackPanel.Children.Add(instructionText);

            grid.Children.Add(stackPanel);
            mainMenuPage.Content = grid;

            NavigateToScreen(mainMenuPage);
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            if (button.Name == "btnReading")
            {
                NavigateToScreen(new ReadScreen());
            }
            else
            {
                // Для остальных кнопок временно показываем сообщение
                ShowComingSoonScreen(button.Content.ToString());
            }
        }

        private void ShowComingSoonScreen(string featureName)
        {
            var comingSoonPage = new Page();
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));

            var stackPanel = new StackPanel();
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Center;

            var iconText = new TextBlock();
            iconText.Text = "🚧";
            iconText.FontSize = 48;
            iconText.HorizontalAlignment = HorizontalAlignment.Center;
            iconText.Margin = new Thickness(0, 0, 0, 20);

            var messageText = new TextBlock();
            messageText.Text = $"{featureName}\n\nComing Soon!";
            messageText.Foreground = Brushes.White;
            messageText.FontSize = 20;
            messageText.TextAlignment = TextAlignment.Center;
            messageText.TextWrapping = TextWrapping.Wrap;

            var backButton = new Button();
            backButton.Content = "← Back to Main Menu";
            backButton.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            backButton.Foreground = Brushes.White;
            backButton.Padding = new Thickness(20, 10, 20, 10);
            backButton.Margin = new Thickness(0, 30, 0, 0);
            backButton.Click += (s, e) => LoadMainMenu();

            stackPanel.Children.Add(iconText);
            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(backButton);

            grid.Children.Add(stackPanel);
            comingSoonPage.Content = grid;

            NavigateToScreen(comingSoonPage);
        }

        private void NavigateToScreen(Page screen)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            MainFrame.Navigate(screen);

            if (MainFrame.Content is FrameworkElement content)
            {
                content.BeginAnimation(OpacityProperty, animation);
            }
        }
    }
}