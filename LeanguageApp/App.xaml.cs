using System;
using System.IO;
using System.Windows;

namespace LeanguageApp
{
    public partial class App : Application
    {
        public static string AppDataFolder { get; private set; }
        public static string FlashcardsFile { get; private set; }
        public static string ImagesFolder { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Инициализация путей
            InitializeAppPaths();

            base.OnStartup(e);

            var mainScreen = new MainScreen();
            mainScreen.Show();
        }

        private void InitializeAppPaths()
        {
            try
            {
                // Папка данных приложения - в AppData пользователя
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                AppDataFolder = Path.Combine(appData, "LeanguageApp");

                // Конкретные пути для файлов
                FlashcardsFile = Path.Combine(AppDataFolder, "flashcards.json");
                ImagesFolder = Path.Combine(AppDataFolder, "Images");

                // Создаем папки если не существуют
                Directory.CreateDirectory(AppDataFolder);
                Directory.CreateDirectory(ImagesFolder);

                Console.WriteLine($"App data folder: {AppDataFolder}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}