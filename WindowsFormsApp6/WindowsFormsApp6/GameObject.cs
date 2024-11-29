using System.Drawing;

// Основной базовый класс для всех игровых объектов
public abstract class GameObject
{
    public Point Position { get; set; }
    public Color Color { get; set; }

    public GameObject(Point position, Color color)
    {
        Position = position;
        Color = color;
    }

    public abstract void Draw(Graphics graphics);
}