using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

public class Load
{
    private GameManager gameManager;
    private List<Pentamino> placedPentaminos;
    private List<Pentamino> unavailablePentaminos;

    public Load(GameManager gameManager, List<Pentamino> placedPentaminos, List<Pentamino> unavailablePentaminos)
    {
        this.gameManager = gameManager;
        this.placedPentaminos = placedPentaminos;
        this.unavailablePentaminos = unavailablePentaminos;
    }

    public List<Pentamino> LoadGameState(string filePath)
    {
        List<Pentamino> loadedPentaminos = new List<Pentamino>();

        if (!File.Exists(filePath)) return loadedPentaminos;

        using (StreamReader reader = new StreamReader(filePath))
        {
            int rows = int.Parse(reader.ReadLine());
            int columns = int.Parse(reader.ReadLine());

            // Инициализация новой игры с заданными размерами
            gameManager = new GameManager(rows, columns);

            placedPentaminos.Clear();  // Очистка списка размещенных фигур
            unavailablePentaminos.Clear();  // Очистка списка недоступных фигур

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split('|');
                Color color = Color.FromArgb(int.Parse(parts[0]));
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                var cells = parts[3].Split(',').Select(c =>
                {
                    var coords = c.Split(':');
                    return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                }).ToList();

                // Создание фигуры из сохраненных данных
                var pentamino = new Pentamino(new Point(x, y), color, cells)
                {
                    IsUsed = true  // Помечает фигуру как использованную
                };

                // Добавление фигуры к списку загруженных пентаминов
                loadedPentaminos.Add(pentamino);

                // Обновление сетки доски, чтобы учитывать размещенные фигуры
                foreach (var cell in pentamino.Cells)
                {
                    int row = pentamino.Position.Y + cell.Y;
                    int column = pentamino.Position.X + cell.X;
                    if (row >= 0 && row < gameManager.Board.Rows && column >= 0 && column < gameManager.Board.Columns)
                    {
                        gameManager.Board.Grid[row, column] = pentamino.Color;
                    }
                }
            }

            // Очистка из списка доступных пентаминов те, которые уже были размещены
            RemovePlacedPentaminosFromAvailable(loadedPentaminos);
        }

        return loadedPentaminos;
    }

    private void RemovePlacedPentaminosFromAvailable(List<Pentamino> loadedPentaminos)
    {
        // Удаление размещённых пентаминов из списка доступных
        foreach (var loadedPentamino in loadedPentaminos)
        {
            gameManager.Pentaminos.RemoveAll(p => p.Cells.SequenceEqual(loadedPentamino.Cells));
        }
    }
}
