using System.Collections.Generic;
using System.Drawing;

public class Pentamino : GameObject
{
    private Point initialPosition; // Исходная позиция пентамино для сброса

    public bool IsUsed { get; set; } // Указывает, использована ли фигура

    public List<Point> Cells { get; private set; } // Список клеток, составляющих пентамино

    public Pentamino(Point position, Color color, List<Point> cells) : base(position, color)
    {
        initialPosition = position; // Установка начальной позиции
        Cells = cells; // Инициализация клеток
        IsUsed = false; // По умолчанию фигура не размещена
    }

    public void ResetToInitialPosition()
    {
        // Сбрасывает пентамино в исходную позицию
        Position = initialPosition;
    }

    public override void Draw(Graphics graphics)
    {
        // Отрисовывает пентамино на игровом поле
        using (SolidBrush brush = new SolidBrush(Color))
        {
            foreach (var cell in Cells)
            {
                Point drawPosition = new Point(Position.X + cell.X, Position.Y + cell.Y); // Позиция клетки
                graphics.FillRectangle(brush, drawPosition.X * Board.CellSize, drawPosition.Y * Board.CellSize, Board.CellSize, Board.CellSize); // Заполнение
                graphics.DrawRectangle(Pens.Black, drawPosition.X * Board.CellSize, drawPosition.Y * Board.CellSize, Board.CellSize, Board.CellSize); // Границы
            }
        }
    }

    public void Move(int deltaX, int deltaY)
    {
        // Перемещает пентамино на заданное расстояние
        Position = new Point(Position.X + deltaX, Position.Y + deltaY);
    }
}
