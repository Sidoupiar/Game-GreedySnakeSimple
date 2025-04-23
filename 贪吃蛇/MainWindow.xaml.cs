using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace 贪吃蛇
{
	public partial class MainWindow : Window
	{
		private const int ScreenActualWidth = 400;
		private const int ScreenActualHeight = 400;
		private const int TileSize = 20;

		private int _frame;
		private bool _canMove;
		private Point _snakeHead;
		private Vector _direction;
		private readonly List<Point> _snakeBody = [];
		private Point _food;
		private bool _isGameOver;
		private int _score;

		public MainWindow()
		{
			this.InitializeComponent();
			this.InitializeGame();
			CompositionTarget.Rendering += this.GameLoop; // 游戏循环
		}

		private void InitializeGame()
		{
			this.Restart(); // 重新开始游戏
			this.Draw(); // 绘制初始状态
		}

		private void GameLoop( object? sender , EventArgs eventArgs )
		{
			if( this._isGameOver )
			{
				return;
			}
			if( this._canMove )
			{
				this.MoveSnake(); // 移动蛇
				this.CheckCollision(); // 检查碰撞
			}
			this.Draw(); // 更新画面
		}

		private void MoveSnake()
		{
			// 每秒移动一次
			this._frame = ( this._frame + 1 ) % 10;
			if( this._frame != 0 )
			{
				return;
			}
			// 移动身体 (从尾部开始更新位置)
			for( int index = this._snakeBody.Count - 1 ; index > 0 ; --index )
			{
				this._snakeBody[index] = this._snakeBody[index - 1];
			}
			if( this._snakeBody.Count > 0 )
			{
				this._snakeBody[0] = this._snakeHead;
			}
			// 移动头部
			this._snakeHead.X += this._direction.X;
			this._snakeHead.Y += this._direction.Y;
		}

		private void CheckCollision()
		{
			// 边界检测
			if( this._snakeHead.X < 0 || this._snakeHead.X >= MainWindow.ScreenActualWidth || this._snakeHead.Y < 0 || this._snakeHead.Y >= MainWindow.ScreenActualHeight )
			{
				this.GameOver();
			}
			// 吃到食物
			if( Math.Abs( this._snakeHead.X - this._food.X ) < MainWindow.TileSize && Math.Abs( this._snakeHead.Y - this._food.Y ) < MainWindow.TileSize )
			{
				this._score += 10;
				this._snakeBody.Add( new Point( -MainWindow.TileSize , -MainWindow.TileSize ) ); // 扩展身体
				this.GenerateFood();
			}
			// 自撞检测
			foreach( Point bodyPart in this._snakeBody )
			{
				if( Math.Abs( this._snakeHead.X - bodyPart.X ) < MainWindow.TileSize && Math.Abs( this._snakeHead.Y - bodyPart.Y ) < MainWindow.TileSize )
				{
					this.GameOver();
				}
			}
		}

		private void GenerateFood()
		{
			Random random = new();
			while( true )
			{
				this._food = new Point(
					random.Next( 0 , MainWindow.ScreenActualWidth / MainWindow.TileSize ) * MainWindow.TileSize ,
					random.Next( 0 , MainWindow.ScreenActualHeight / MainWindow.TileSize ) * MainWindow.TileSize
				);
				if( !this.CollideSnake( this._food ) )
				{
					break;
				}
			}
		}

		private bool CollideSnake( Point point )
		{
			if( point == this._snakeHead )
			{
				return true;
			}
			foreach( Point bodyPart in this._snakeBody )
			{
				if( point == bodyPart )
				{
					return true;
				}
			}
			return false;
		}

		private void Restart()
		{
			this._isGameOver = false;
			this._canMove = false;
			this._frame = 1;
			// 初始化蛇头位置
			this._snakeHead = new Point( 10 * MainWindow.TileSize , 10 * MainWindow.TileSize );
			this._direction = new Vector( MainWindow.TileSize , 0 );
			this._snakeBody.Clear();
			this._score = 0;
			this.GenerateFood(); // 生成第一个食物
		}

		private void GameOver()
		{
			this._isGameOver = true;
			CompositionTarget.Rendering -= this.GameLoop;
			_ = MessageBox.Show( $"游戏结束！得分: {this._score}" );
			this.Restart();
		}

		private void Draw()
		{
			this.GameCanvas.Children.Clear();
			// 绘制食物
			Rectangle foodRect = new()
			{
				Width = MainWindow.TileSize ,
				Height = MainWindow.TileSize ,
				Fill = Brushes.Red
			};
			Canvas.SetLeft( foodRect , this._food.X );
			Canvas.SetTop( foodRect , this._food.Y );
			_ = this.GameCanvas.Children.Add( foodRect );
			// 绘制蛇
			Rectangle headRect = new()
			{
				Width = MainWindow.TileSize ,
				Height = MainWindow.TileSize ,
				Fill = Brushes.Green
			};
			Canvas.SetLeft( headRect , this._snakeHead.X );
			Canvas.SetTop( headRect , this._snakeHead.Y );
			_ = this.GameCanvas.Children.Add( headRect );
			foreach( Point body in this._snakeBody )
			{
				Rectangle bodyRect = new()
				{
					Width = MainWindow.TileSize ,
					Height = MainWindow.TileSize ,
					Fill = Brushes.LightGreen
				};
				Canvas.SetLeft( bodyRect , body.X );
				Canvas.SetTop( bodyRect , body.Y );
				_ = this.GameCanvas.Children.Add( bodyRect );
			}
		}

		protected override void OnKeyDown( KeyEventArgs eventArgs )
		{
			base.OnKeyDown( eventArgs );
			switch( eventArgs.Key )
			{
				case Key.Left when this._direction.X != MainWindow.TileSize:
					this._direction = new Vector( -MainWindow.TileSize , 0 );
					break;
				case Key.Right when this._direction.X != -MainWindow.TileSize:
					this._direction = new Vector( MainWindow.TileSize , 0 );
					break;
				case Key.Up when this._direction.Y != MainWindow.TileSize:
					this._direction = new Vector( 0 , -MainWindow.TileSize );
					break;
				case Key.Down when this._direction.Y != -MainWindow.TileSize:
					this._direction = new Vector( 0 , MainWindow.TileSize );
					break;
				default:
					this._canMove = !this._canMove;
					break;
			}
		}
	}
}