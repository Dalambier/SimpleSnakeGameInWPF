using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FirstGame
{

    //Лютая змеище для Артёма Викторовича

    public partial class MainWindow : Window
    {
        private int snakeLength; //Длина змеи
        private int currentScore = 0; //Очки

        private UIElement snakeFood = null; //
        private SolidColorBrush foodBrush = Brushes.Yellow; //Цвет еды (кукурузы)

        const int SnakeSquareSize = 20; //Размер блока змеи
        const int SnakeStartLength = 3; //Начальное значение блоков змеи
        const int SnakeStartSpeed = 400; //Начальная скорость змеи
        const int SnakeSpeedThreshold = 100; //Порог скорости

        private Random rnd = new Random(); //Объявление рандома

        private Point GetNextFoodPosition() //Появление новой еды на экране
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize); //Вычисление максимального x-пространства
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize); //Вычисление максимального y-пространства
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize; //Место для спавна по оси X
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize; //Место для спавна по оси Y

            foreach (SnakePart snakePart in snakeParts) //Проверка, чтобы еда не появилась в змее
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition(); //Снова вызов функции спавна еды
            }

            return new Point(foodX, foodY); //Возвращение объекта
        }

        private SolidColorBrush snakeBodyBrush = Brushes.Gray; //Цвет тела змеи
        private SolidColorBrush snakeHeadBrush = Brushes.Black; //ЦВет головы змеи
        private List<SnakePart> snakeParts = new List<SnakePart>(); //Список элементов змеи

        public enum SnakeDirection { Left, Right, Up, Down }; //Напрвления змеи
        private SnakeDirection snakeDirection = SnakeDirection.Right; //Начальное направление змеи направо

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer(); //Объявление таймера
        public MainWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick; //Обновление всего происходящего на экране с новым тиком
        }

        private void StartNewGame()
        {
            // Удаление ненужных частей змеи и остатки еды
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();
            if (snakeFood != null)
                GameArea.Children.Remove(snakeFood);

            // Сбрасывает всякое
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            //Рисует змею и еду
            DrawSnake();
            DrawSnakeFood();

            // Обновляет игровой статус
            UpdateGameStatus();

            //Запускает таймер
            gameTickTimer.IsEnabled = true;
        }

        private void GameTickTimer_Tick(object sender, EventArgs e) //С каждым тиком, змея движется
        {
            MoveSnake();
        } 

        private void Window_ContentRendered(object sender, EventArgs e) //Отрисовка области
        {
            DrawGameArea();
            StartNewGame();
        }

        private void DrawGameArea() //Рисовка игровой зоны
        {
            bool doneDrawingBackground = false; //Рисовка заднего фона игровой зоны
            int nextX = 0, nextY = 0; //Следующий x и y игровой зоны
            int rowCounter = 0; //Число строк
            bool nextIsOdd = false; //Чётность

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.DarkGreen : Brushes.Green //Если чётно, то один цвет, если нечётно, то другой
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }

        private void UpdateGameStatus() //Обновление игрвого статуса в заголовке окна
        {
            this.Title = "Лютая змея - Счёт: " + currentScore + " - Скорость игры: " + gameTickTimer.Interval.TotalMilliseconds;
        }

        private void EndGame() //Конец игры
        {
            gameTickTimer.IsEnabled = false; //Отключение таймера
            MessageBox.Show("Змея померла\n\nНажмите на пробел, чтобы начать заново", "Лютая змея");
        }

        private void DrawSnake() //Отрисовка змеи
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void DrawSnakeFood() //Отрисовка еды (кукурузы)
        {
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        private void EatSnakeFood() //Съедение еды
        {
            snakeLength++; //Увеличение 
            currentScore++; //Увеличение очков
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }

        private void MoveSnake()
        {
            // Удаление последней зачти змеи и подготова новой части
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }
            // Добавляем новый эемент змеи (голову) 
            // Все существующие элементы помечаем как тело  
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            // Определение, в какую сторону развернуть змею
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1]; //Определение головы
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            //Добавление голову в список элементов змеи
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            DrawSnake(); //Рисование змеи
            DoCollisionCheck(); //Проверка столкновения с объектами 
        } //Обработка движения змеи

        private void DoCollisionCheck() //Обработка столкновения
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            //Проверка столкновения с едой
            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            //Проверка столкновения с концом карты
            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
            (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            //Проверка столкновения с змеёй
            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e) //Обработка нажатий клавиш
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            if (snakeDirection != originalSnakeDirection) //Если ничего не нажато, змея просто двигается
                MoveSnake();
        }
    }
}
