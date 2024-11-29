using System.Collections.Generic;
using System.Drawing;

public class Board
{
    public static int CellSize { get; set; } = 50;
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public Color[,] Grid { get; private set; }
    public List<Pentamino> PlacedPentaminos { get; private set; }

    public Board(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        Grid = new Color[Rows, Columns];
        PlacedPentaminos = new List<Pentamino>();

        // Инициализация сетки значения, которое будет обозначать пустые ячейки
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Grid[row, col] = Color.Transparent; // Использование Color.Transparent для пустых ячеек
            }
        }
    }

    // Рисует поле и пентамино на нем
    public void Draw(Graphics graphics)
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Rectangle cellRect = new Rectangle(col * CellSize, row * CellSize, CellSize, CellSize);
                graphics.DrawRectangle(Pens.Black, cellRect);
                if (Grid[row, col] != Color.Transparent) // Проверяем, не пустая ли ячейка
                {
                    using (SolidBrush brush = new SolidBrush(Grid[row, col]))
                    {
                        graphics.FillRectangle(brush, cellRect);
                    }
                }
            }
        }

        // Рисует размещенные пентамино на поле
        foreach (var pentamino in PlacedPentaminos)
        {
            pentamino.Draw(graphics);
        }
    }

    // Проверка, что поле заполнено
    public bool IsFull()
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (Grid[y, x] == Color.Transparent) // Если клетка пуста
                {
                    return false;
                }
            }
        }
        return true;
    }

// Метод для пометки клетки как занятый
public void MarkCellAsOccupied(int x, int y, Color color)
    {
        if (x >= 0 && x < Columns && y >= 0 && y < Rows)
        {
            Grid[y, x] = color;  // Помечает клетку как занятую цветом пентамино
        }
    }
}
