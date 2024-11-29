using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        private GameManager gameManager;
        private Pentamino selectedPentamino;
        private Point mouseOffset;
        private bool isDragging = false;
        private List<Pentamino> placedPentaminos = new List<Pentamino>();
        private List<Pentamino> unavailablePentaminos = new List<Pentamino>();
        private Point previousPosition;
        private Load loadManager;

        public Form1()
        {
            try
            {
                InitializeComponent();
                ShowSizeSelectionDialog();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в конструкторе Form1: {ex.Message}");
                MessageBox.Show("Произошла ошибка при инициализации формы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSizeSelectionDialog()
        {
            try
            {
                using (Form sizeSelectionForm = new Form())
                {
                    sizeSelectionForm.Text = "Выбор размера поля";
                    sizeSelectionForm.Size = new Size(300, 200);
                    sizeSelectionForm.StartPosition = FormStartPosition.CenterScreen;

                    Label label = new Label()
                    {
                        Text = "Выберите размер поля (от 3 до 6):",
                        Dock = DockStyle.Top,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    NumericUpDown numericUpDown = new NumericUpDown()
                    {
                        Minimum = 3,
                        Maximum = 6,
                        Value = 3,
                        Dock = DockStyle.Top,
                        TextAlign = HorizontalAlignment.Center
                    };

                    // Запрещает ввод любых символов, кроме цифр
                    numericUpDown.KeyPress += (sender, e) =>
                    {
                        // Разрешает ввод только цифр, использование Backspace
                        if (!Char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                        {
                            e.Handled = true; // Отмена ввода, если это не цифра или Backspace
                        }
                    };

                    Button okButton = new Button() { Text = "OK", Dock = DockStyle.Bottom };
                    bool sizeSelected = false; // Флаг, указывающий, был ли выбран размер поля

                    okButton.Click += (sender, e) =>
                    {
                        try
                        {
                            int selectedSize = (int)numericUpDown.Value;
                            InitializeGame(selectedSize);
                            sizeSelected = true; 
                            sizeSelectionForm.Close();
                        }
                        catch (Exception ex)
                        {
                            // Логирование ошибки
                            Logger.LogError($"Ошибка при выборе размера поля: {ex.Message}");
                            MessageBox.Show("Произошла ошибка при выборе размера поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    sizeSelectionForm.FormClosing += (sender, e) =>
                    {
                        // Если окно закрыто вручную и размер поля не был выбран, завершение программы
                        if (!sizeSelected)
                        {
                            Application.Exit();
                        }
                    };

                    sizeSelectionForm.Controls.Add(okButton);
                    sizeSelectionForm.Controls.Add(numericUpDown);
                    sizeSelectionForm.Controls.Add(label);
                    sizeSelectionForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в ShowSizeSelectionDialog: {ex.Message}");
                MessageBox.Show("Произошла ошибка при отображении диалога выбора размера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeGame(int size)
        {
            try
            {
                int rows = size;
                int columns = size;

                gameManager = new GameManager(rows, columns);

                // Очищение списков размещенных и недоступных пентаминов
                placedPentaminos.Clear();
                unavailablePentaminos.Clear();

                loadManager = new Load(gameManager, placedPentaminos, unavailablePentaminos);

                // Начальные позиции для всех пентаминов
                SetInitialPentaminoPositions();

                this.WindowState = FormWindowState.Maximized;
                this.DoubleBuffered = true;
                Invalidate();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в InitializeGame: {ex.Message}");
                MessageBox.Show("Произошла ошибка при инициализации игры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetInitialPentaminoPositions()
        {
            try
            {
                if (gameManager == null) return;

                // Установка начальных значений смещения и расстояния между пентаминами
                int offsetX = -23;
                int offsetY = -7;
                int horizontalSpacing = 6;
                int verticalSpacing = 4;

                int index = 0;
                foreach (var pentamino in gameManager.Pentaminos)
                {
                    if (!unavailablePentaminos.Contains(pentamino) && !placedPentaminos.Contains(pentamino))
                    {
                        pentamino.Position = new Point(offsetX + (index % 5) * horizontalSpacing, offsetY + (index / 5) * verticalSpacing);
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в SetInitialPentaminoPositions: {ex.Message}");
                MessageBox.Show("Произошла ошибка при установке начальных позиций пентаминов.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGameState(string filePath)
        {
            try
            {
                var loadedPentaminos = loadManager.LoadGameState(filePath);

                // Проверка, подходят ли загружаемые фигуры под текущее поле
                bool isOutOfBounds = loadedPentaminos.Any(p => p.Cells.Any(cell =>
                {
                    int cellX = p.Position.X + cell.X;
                    int cellY = p.Position.Y + cell.Y;
                    return cellX < 0 || cellY < 0 || cellX >= gameManager.Board.Columns || cellY >= gameManager.Board.Rows;
                }));

                if (isOutOfBounds)
                {
                    // Логирование ошибки
                    string errorMessage = "Размер поля не соответствует файлу загрузки. Загружаемые фигуры выходят за пределы текущего поля.";
                    Logger.LogError(errorMessage);
                    MessageBox.Show(errorMessage, "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                placedPentaminos = loadedPentaminos;
                unavailablePentaminos.AddRange(loadedPentaminos);

                // Обновление списка доступных пентаминов после загрузки
                gameManager.Pentaminos.RemoveAll(p => placedPentaminos.Any(up => up.Cells.SequenceEqual(p.Cells)));

                SetInitialPentaminoPositions();
                Invalidate();

                // Пометка клетки как занятой пентаминой
                foreach (var pentamino in placedPentaminos)
                {
                    foreach (var cell in pentamino.Cells)
                    {
                        int cellX = pentamino.Position.X + cell.X;
                        int cellY = pentamino.Position.Y + cell.Y;
                        // Пометка клетки как занятый элемент на поле
                        gameManager.Board.MarkCellAsOccupied(cellX, cellY, pentamino.Color);
                    }
                }

                // Перерисовывание поля, чтобы отобразить все изменения
                Invalidate();

                // Проверка, заполнено ли поле после загрузки
                if (gameManager.Board.IsFull())
                {
                    MessageBox.Show("Поздравляем! Вы успешно закрыли всё поле!", "Победа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetButton_Click(null, null); // Перезапуск игры
                }
            }
            catch (Exception ex)
            {

                //Логирование ошибки
                Logger.LogError($"Ошибка в LoadGameState: {ex.Message}");
                MessageBox.Show("Произошла ошибка при загрузке состояния игры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void SaveGameState(string filePath)
        {
            try
            {
                // Открытие файла
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(gameManager.Board.Rows);
                    writer.WriteLine(gameManager.Board.Columns);
                    foreach (var pentamino in placedPentaminos)
                    {
                        // Запись свойств пентамино в файл:
                        // 1. Цвет пентамино (ARGB) - используется метод ToArgb().
                        // 2. Позиция пентамино (X и Y).
                        // 3. Координаты всех ячеек пентамино.
                        string cells = string.Join(",", pentamino.Cells.Select(cell => $"{cell.X}:{cell.Y}"));
                        writer.WriteLine($"{pentamino.Color.ToArgb()}|{pentamino.Position.X}|{pentamino.Position.Y}|{cells}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в SaveGameState: {ex.Message}");
                MessageBox.Show("Произошла ошибка при сохранении состояния игры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Сбрасывание текущего состояния игры
                placedPentaminos.Clear();
                unavailablePentaminos.Clear();
                gameManager = new GameManager(gameManager.Board.Rows, gameManager.Board.Columns);
                SetInitialPentaminoPositions();
                Invalidate();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в ResetButton_Click: {ex.Message}");
                MessageBox.Show("Произошла ошибка при сбросе игры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveGameState("gameState.txt");
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в SaveButton_Click: {ex.Message}");
                MessageBox.Show("Произошла ошибка при сохранении игры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadGameState("gameState.txt");
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в LoadButton_Click: {ex.Message}");
                MessageBox.Show("Произошла ошибка при загрузке игры.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                if (gameManager != null)
                {
                    // Вычисление смещения и размеров игрового поля
                    int boardWidth = gameManager.Board.Columns * Board.CellSize;
                    int boardHeight = gameManager.Board.Rows * Board.CellSize;
                    int offsetX = ClientSize.Width - boardWidth - 50;
                    int offsetY = (ClientSize.Height - boardHeight) / 2 + 150;

                    // Применение смещения для отрисовки
                    e.Graphics.TranslateTransform(offsetX, offsetY);

                    // Отрисовка игрового поля
                    gameManager.Board.Draw(e.Graphics);

                    // Отрисовка размещенных пентаминов
                    foreach (var pentamino in placedPentaminos)
                    {
                        pentamino.Draw(e.Graphics);
                    }

                    // Отрисовка пентаминов, которые не размещены
                    foreach (var pentamino in gameManager.Pentaminos)
                    {
                        if (!placedPentaminos.Contains(pentamino) && !unavailablePentaminos.Contains(pentamino))
                        {
                            pentamino.Draw(e.Graphics);
                        }
                    }

                    // Отрисовка выделенного пентамино
                    if (selectedPentamino != null)
                    {
                        selectedPentamino.Draw(e.Graphics);
                    }

                    // Сброс смещения после отрисовки
                    e.Graphics.ResetTransform();
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Logger.LogError($"Ошибка в OnPaint: {ex.Message}");
                MessageBox.Show("Произошла ошибка при перерисовке экрана.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                base.OnMouseDown(e);

                if (gameManager == null) return;

                // Вычисляем смещение игрового поля относительно окна.
                int offsetX = ClientSize.Width - gameManager.Board.Columns * Board.CellSize - 50;
                int offsetY = (ClientSize.Height - gameManager.Board.Rows * Board.CellSize) / 2 + 150;

                // Корректируем позицию мыши с учетом смещения игрового поля.
                Point adjustedMousePosition = new Point(e.X - offsetX, e.Y - offsetY);

                foreach (var pentamino in gameManager.Pentaminos)
                {
                    // Пропуск размещенных, недоступных или уже используемых пентамин
                    if (placedPentaminos.Contains(pentamino) || unavailablePentaminos.Contains(pentamino) || pentamino.IsUsed)
                        continue;

                    // Проверка, попал ли клик в одну из ячеек текущего пентамино
                    foreach (var cell in pentamino.Cells)
                    {
                        Point cellPosition = new Point(
                            pentamino.Position.X + cell.X,
                            pentamino.Position.Y + cell.Y
                        );
                        Rectangle cellRect = new Rectangle(
                            cellPosition.X * Board.CellSize,
                            cellPosition.Y * Board.CellSize,
                            Board.CellSize,
                            Board.CellSize
                        );

                        if (cellRect.Contains(adjustedMousePosition))
                        {
                            // Выбор пентамино и начало его перемещения
                            selectedPentamino = pentamino;

                            if (selectedPentamino.IsUsed) return;

                            previousPosition = pentamino.Position;
                            mouseOffset = new Point(
                                adjustedMousePosition.X - pentamino.Position.X * Board.CellSize,
                                adjustedMousePosition.Y - pentamino.Position.Y * Board.CellSize
                            );
                            isDragging = true;

                            // Перемещение выбранного пентамино в конец списка, чтобы оно отрисовывалось поверх остальных
                            gameManager.Pentaminos.Remove(selectedPentamino);
                            gameManager.Pentaminos.Add(selectedPentamino);

                            Invalidate(); // Перерисовывание игрового поля
                            break;
                        }
                    }

                    // Если пентамино выбрано, завершение обработку
                    if (selectedPentamino != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибок
                Logger.LogError($"Ошибка в OnMouseDown: {ex.Message}");
                MessageBox.Show("Произошла ошибка при обработке нажатия мыши.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);

                // Если фигура выбрана и перемещается, обновляется её позиция
                if (isDragging && selectedPentamino != null)
                {
                    int offsetX = ClientSize.Width - gameManager.Board.Columns * Board.CellSize - 50;
                    int offsetY = (ClientSize.Height - gameManager.Board.Rows * Board.CellSize) / 2 + 150;
                    Point adjustedMousePosition = new Point(e.X - offsetX, e.Y - offsetY);

                    // Рассчет новой позиции фигуры в координатах игрового поля
                    int newX = (adjustedMousePosition.X - mouseOffset.X) / Board.CellSize;
                    int newY = (adjustedMousePosition.Y - mouseOffset.Y) / Board.CellSize;

                    selectedPentamino.Position = new Point(newX, newY);
                    Invalidate(); // Перерисовка экрана
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибок
                Logger.LogError($"Ошибка в OnMouseMove: {ex.Message}");
                MessageBox.Show("Произошла ошибка при обработке перемещения мыши.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                base.OnMouseUp(e);

                // Если фигура была перемещена, проверяется её корректное размещение
                if (isDragging)
                {
                    isDragging = false;

                    if (selectedPentamino != null)
                    {
                        bool isOutOfBounds = false;
                        foreach (var cell in selectedPentamino.Cells)
                        {
                            // Проверяется, чтобы фигура не выходила за границы поля
                            int cellX = selectedPentamino.Position.X + cell.X;
                            int cellY = selectedPentamino.Position.Y + cell.Y;

                            if (cellX < 0 || cellY < 0 || cellX >= gameManager.Board.Columns || cellY >= gameManager.Board.Rows)
                            {
                                isOutOfBounds = true;
                                break;
                            }
                        }

                        if (isOutOfBounds)
                        {
                            // Сообщение об ошибке и возврат фигуры в прежнюю позицию
                            string errorMessage = "Фигура выходит за пределы поля!";
                            Logger.LogError(errorMessage);
                            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            selectedPentamino.Position = previousPosition;
                            selectedPentamino = null;
                            Invalidate();
                            return;
                        }

                        bool isOverlap = false;
                        foreach (var placedPentamino in placedPentaminos)
                        {
                            // Проверка на перекрытие с другими фигурами
                            foreach (var cell in selectedPentamino.Cells)
                            {
                                Point cellPosition = new Point(selectedPentamino.Position.X + cell.X, selectedPentamino.Position.Y + cell.Y);

                                foreach (var placedCell in placedPentamino.Cells)
                                {
                                    Point placedCellPosition = new Point(placedPentamino.Position.X + placedCell.X, placedPentamino.Position.Y + placedCell.Y);

                                    if (cellPosition == placedCellPosition)
                                    {
                                        isOverlap = true;
                                        break;
                                    }
                                }

                                if (isOverlap)
                                    break;
                            }

                            if (isOverlap)
                                break;
                        }

                        if (isOverlap)
                        {
                            // Сообщение об ошибке перекрытия и возврат фигуру
                            string errorMessage = "Нельзя разместить фигуру на занятую клетку!";
                            Logger.LogError(errorMessage);
                            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            selectedPentamino.Position = previousPosition;
                            selectedPentamino = null;
                            Invalidate();
                            return;
                        }

                        // Добавление фигуры на поле
                        placedPentaminos.Add(selectedPentamino);
                        unavailablePentaminos.Add(selectedPentamino);
                        gameManager.PlacePentamino(selectedPentamino);
                        selectedPentamino = null;
                        Invalidate();

                        // Проверка, заполнено ли поле полностью после размещения фигуры
                        if (gameManager.Board.IsFull())
                        {
                            MessageBox.Show("Поздравляем! Вы успешно закрыли всё поле!", "Победа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ResetButton_Click(null, null); // Вызов сброса для перезапуска игры
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибок
                Logger.LogError($"Ошибка в OnMouseUp: {ex.Message}");
                MessageBox.Show("Произошла ошибка при отпускании кнопки мыши.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            try
            {
                base.OnResize(e);
                Invalidate();
            }
            catch (Exception ex)
            {
                // Логирование ошибок
                Logger.LogError($"Ошибка в OnResize: {ex.Message}");
                MessageBox.Show("Произошла ошибка при изменении размера окна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
