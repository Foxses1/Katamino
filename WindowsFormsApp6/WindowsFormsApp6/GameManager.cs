using System.Collections.Generic;
using System.Drawing;

public class GameManager
{

    public Board Board { get; private set; }
    public List<Pentamino> Pentaminos { get; private set; }

    public GameManager(int rows, int columns)
    {
        Board = new Board(rows, columns);
        Pentaminos = new List<Pentamino>();
        InitializePentaminos();
    }

    private void InitializePentaminos()
    {
        // Добавление несколько пентамино разных форм и цветов
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Red, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(4, 0)
        })); // Форма I

        Pentaminos.Add(new Pentamino(new Point(5, 0), Color.Blue, new List<Point>
        {
            new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(2, 0)
        })); // Форма U

        Pentaminos.Add(new Pentamino(new Point(0, 5), Color.Green, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(1, 2), new Point(2, 2)
        })); // Форма L
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Purple, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(2, 0), new Point(1, -1)
        })); // Форма T

        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Yellow, new List<Point>
        {
            new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 2)
        })); // Форма S

        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Cyan, new List<Point>
        {
            new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 2), new Point(2, 2)
        })); // Форма L (другая)

        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Brown, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(2, 0)
        })); // Линия из 3 кубов

        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Pink, new List<Point>
        {
            new Point(0, 0), new Point(1, 0)
        })); // Линия из 2 кубов
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Orange, new List<Point>
        {
            new Point(0, 0), new Point(0, 1)
        })); // Вертикальная линия из 2 кубов
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Magenta, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(1, 1)
        })); // Г-образная фигура из 3 кубов
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.LightBlue, new List<Point>
        {
            new Point(0, 0), new Point(0, 1), new Point(0, 2)
        })); // Вертикальная линия из 3 кубов
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.Coral, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(0, 1), new Point(2, 1)
        })); // Перевернутая буква "П" из 5 блоков
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.RoyalBlue, new List<Point>
        {  new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3)
        })); // Вертикальная линия из 4 блоков
        Pentaminos.Add(new Pentamino(new Point(0, 0), Color.DarkBlue, new List<Point>
        {
            new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)
        })); // Куб из 4 блоков
    }
    public void ResetBoard()
    {
        // Сброс всех ячеек доски на прозрачный цвет
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int column = 0; column < Board.Columns; column++)
            {
                Board.Grid[row, column] = Color.Transparent;
            }
        }

        // Очистка списка пентаминов и создание их заново
        Pentaminos.Clear();
        InitializePentaminos();
    }

    public bool IsBoardFull()
    {
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int column = 0; column < Board.Columns; column++)
            {
                if (Board.Grid[row, column] == Color.Transparent) // Проверка, что ячейка не пуста
                {
                    return false; // Если хотя бы одна ячейка пуста, поле не заполнено
                }
            }
        }
        return true; // Все ячейки заполнены
    }

    public void Draw(Graphics graphics)
    {
        // Рисует игровое поле
        Board.Draw(graphics);

        // Рисует все пентамино, доступные для размещения
        foreach (var pentamino in Pentaminos)
        {
            pentamino.Draw(graphics);
        }
    }

    public void PlacePentamino(Pentamino pentamino)
    {
        // Размещение выбранного пентамино на игровом поле
        foreach (var cell in pentamino.Cells)
        {
            int row = pentamino.Position.Y + cell.Y; // Координата строки
            int column = pentamino.Position.X + cell.X; // Координата столбца

            // Проверка, что ячейка находится в пределах игрового поля
            if (row >= 0 && row < Board.Rows && column >= 0 && column < Board.Columns)
            {
                // Установка цвета клетки на игровом поле
                Board.Grid[row, column] = pentamino.Color;
            }
        }

        // Добавление пентамино в список размещённых фигур
        Board.PlacedPentaminos.Add(pentamino);
    }
}
