using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PhotoMosaic
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            InitBoard();
        }

        private readonly Canvas[] _canvas = new Canvas[16];
        private readonly Image[] _image = new Image[16];
        private readonly int[] _board = new int[16];

        void InitBoard()
        {
            var uri = new Uri("Images/Lviv.png", UriKind.Relative);

            for (var ix = 0; ix < 4; ix++)
            {
                for (var iy = 0; iy < 4; iy++)
                {
                    var nx = (ix * 4) + iy;

                    _image[nx] = new Image
                                {
                                    Height = 400,
                                    Width = 400,
                                    Stretch = Stretch.UniformToFill
                                };

                    var r = new RectangleGeometry
                                {
                                    Rect = new Rect((ix * 100), (iy * 100), 100, 100)
                                };

                    _image[nx].Clip = r;
                    _image[nx].Source = new BitmapImage(uri);
                    _image[nx].SetValue(Canvas.TopProperty, Convert.ToDouble(iy * 100 * -1));
                    _image[nx].SetValue(Canvas.LeftProperty, Convert.ToDouble(ix * 100 * -1));

                    _canvas[nx] = new Canvas
                                 {
                                     Width = 100,
                                     Height = 100
                                 };

                    _canvas[nx].Children.Add(_image[nx]);
                    _canvas[nx].SetValue(NameProperty, "C" + nx);
                    _canvas[nx].MouseLeftButtonDown += Page_MouseLeftButtonDown;

                    if (nx < 15)
                    {
                        GameContainer.Children.Add(_canvas[nx]);
                    }
                }
            }

            Shuffle();
            DrawBoard();
        }

        void Shuffle()
        {
            for (var n = 0; n < 15; n++)
            {
                _board[n] = n;
            }

            var rand = new Random(System.DateTime.Now.Second);

            for (var n = 0; n < 100; n++)
            {
                var n1 = rand.Next(15);
                var n2 = rand.Next(15);

                if (n1 != n2)
                {
                    var tmp = _board[n1];
                    _board[n1] = _board[n2];
                    _board[n2] = tmp;
                }
            }

            _board[15] = -1;
        }

        void DrawBoard()
        {
            for (var n = 0; n < 15; n++)
            {
                var nx = n / 4;
                var ny = n % 4;

                if (_board[n] >= 0)
                {
                    _canvas[_board[n]].SetValue(Canvas.TopProperty, Convert.ToDouble(ny * 100));
                    _canvas[_board[n]].SetValue(Canvas.LeftProperty, Convert.ToDouble(nx * 100));
                }
            }
        }

        void Page_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var c = sender as Canvas;

            var nCanvasId = -1;
            var nBoardLoc = -1;
            var nEmptyLoc = -1;

            for (var i = 0; i < 16; i++)
            {
                if (c == _canvas[i])
                {
                    nCanvasId = i;
                    break;
                }
            }

            for (var i = 0; i < 16; i++)
            {
                if (_board[i] == nCanvasId)
                {
                    nBoardLoc = i;
                }
                else if (_board[i] == -1)
                {
                    nEmptyLoc = i;
                }
            }

            if ((nBoardLoc == nEmptyLoc + 1) || (nBoardLoc == nEmptyLoc - 1) ||
                (nBoardLoc == nEmptyLoc + 4) || (nBoardLoc == nEmptyLoc - 4))
            {
                var nx = nEmptyLoc / 4;
                var ny = nEmptyLoc % 4;

                var animation = new MovingAnimation(_canvas[_board[nBoardLoc]])
                                    {
                                        MoveTo = new Point(nx*100, ny*100)
                                    };
                animation.Begin();

                //_canvas[_board[nBoardLoc]].SetValue(Canvas.TopProperty, Convert.ToDouble(ny * 100));
                //_canvas[_board[nBoardLoc]].SetValue(Canvas.LeftProperty, Convert.ToDouble(nx * 100));

                _board[nEmptyLoc] = nCanvasId;
                _board[nBoardLoc] = -1;
                CheckWinner();
            }
        }

        void CheckWinner()
        {
            var bCompleted = true;
            for (var n = 0; n < 15; n++)
            {
                if (n != _board[n])
                {
                    bCompleted = false;
                    break;
                }
            }
            if (bCompleted)
            {
                // Game Over
            }
        }
    }

    public class MovingAnimation
    {
        #region Fields

        private readonly UIElement _control;
        private readonly Storyboard _storyBoard;

        private readonly DoubleAnimation _doubleAnimationX = new DoubleAnimation()
                                                                 {
                                                                     EasingFunction = new BackEase()
                                                                                          {
                                                                                              EasingMode =
                                                                                                  EasingMode.EaseInOut,
                                                                                              Amplitude = 0.3
                                                                                          }
                                                                 };

        private readonly DoubleAnimation _doubleAnimationY = new DoubleAnimation()
                                                                 {
                                                                     EasingFunction =
                                                                         new BackEase()
                                                                             {
                                                                                 EasingMode = EasingMode.EaseInOut,
                                                                                 Amplitude = 0.3
                                                                             }
                                                                 };

        private Point _moveTo;

        #endregion

        #region Properties

        public Point MoveTo
        {
            get { return _moveTo; }
            set
            {
                _moveTo = value;

                _storyBoard.Duration =
                    TimeSpan.FromSeconds(
                        Math.Sqrt(Math.Pow(_moveTo.X - Canvas.GetLeft(_control), 2) +
                                  Math.Pow(_moveTo.Y - Canvas.GetTop(_control), 2)));

                _doubleAnimationX.To = _moveTo.X;
                _doubleAnimationY.To = _moveTo.Y;
            }
        }

        #endregion

        #region Constructors

        public MovingAnimation(UIElement element)
        {
            _storyBoard = Create(_control = element);
        }

        #endregion

        #region Methods

        private Storyboard Create(UIElement element)
        {
            var sb = new Storyboard();

            sb.Children.Add(_doubleAnimationX);
            sb.Children.Add(_doubleAnimationY);

            Storyboard.SetTarget(_doubleAnimationX, element);
            Storyboard.SetTarget(_doubleAnimationY, element);

            Storyboard.SetTargetProperty(_doubleAnimationX, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTargetProperty(_doubleAnimationY, new PropertyPath("(Canvas.Top)"));

            return sb;
        }

        public void Begin()
        {
            _storyBoard.Begin();
        }

        #endregion
    }

}
