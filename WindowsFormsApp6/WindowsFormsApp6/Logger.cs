using System;
using System.IO;
using System.Windows.Forms;

public static class Logger
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");

    public static void LogError(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            // Если запись в лог не удалась, логирование ошибок
            MessageBox.Show($"Ошибка при записи в лог: {ex.Message}", "Ошибка логирования", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
