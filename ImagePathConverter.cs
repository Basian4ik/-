using System;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace БаязитовЛангуге
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string photoPath = value as string;

            if (string.IsNullOrEmpty(photoPath))
                return GetDefaultImage();

            // Пробуем найти файл
            string fullPath = FindExistingFile(photoPath);

            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                    return GetDefaultImage();
                }
            }

            System.Diagnostics.Debug.WriteLine($"Файл не найден: {photoPath}");
            return GetDefaultImage();
        }

        private string FindExistingFile(string photoPath)
        {
            // Нормализуем путь (заменяем / на \)
            string normalizedPath = photoPath.Replace('/', '\\');

            // 1. Ищем в папке bin\Debug\Клиенты
            string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, normalizedPath);
            System.Diagnostics.Debug.WriteLine($"Ищем в Debug: {debugPath}");
            if (File.Exists(debugPath))
                return debugPath;

            // 2. Ищем в папке проекта (на уровень выше)
            string projectFolder = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string projectPath = Path.Combine(projectFolder, normalizedPath);
            System.Diagnostics.Debug.WriteLine($"Ищем в проекте: {projectPath}");
            if (File.Exists(projectPath))
                return projectPath;

            // 3. Ищем только по имени файла в папке Клиенты (Debug)
            string fileName = Path.GetFileName(photoPath);
            string debugFileOnly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты", fileName);
            System.Diagnostics.Debug.WriteLine($"Ищем по имени в Debug: {debugFileOnly}");
            if (File.Exists(debugFileOnly))
                return debugFileOnly;

            // 4. Ищем по имени файла в папке Клиенты (проект)
            string projectFileOnly = Path.Combine(projectFolder, "Клиенты", fileName);
            System.Diagnostics.Debug.WriteLine($"Ищем по имени в проекте: {projectFileOnly}");
            if (File.Exists(projectFileOnly))
                return projectFileOnly;

            // 5. Рекурсивный поиск по всем подпапкам (медленно, но надёжно)
            var foundFile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (foundFile != null)
            {
                System.Diagnostics.Debug.WriteLine($"Найден рекурсивно: {foundFile}");
                return foundFile;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapImage GetDefaultImage()
        {
            // Сначала ищем заглушку в Debug
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты", "empty.png");

            // Если нет в Debug, ищем в проекте
            if (!File.Exists(defaultPath))
            {
                string projectFolder = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                defaultPath = Path.Combine(projectFolder, "Клиенты", "empty.png");
            }

            if (File.Exists(defaultPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(defaultPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}