using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static database database = new database();
        MySqlConnection mySqlConnection = new MySqlConnection(database.Initialize());
        public MainWindow()
        {
            InitializeComponent();
        }
        #region Variables <----
        const int BoardSize = 102;
        public const int smlBoardSize = 12;
        const int RectSize = 10;
        double[] speeds = {0.5, 1, 2, 4 };
        int select_speed = 1;
        int CreationDelay = 100;
        bool IsGameStarted = false;
        bool isLocked = false;
        bool dragged = false;
        DispatcherTimer dTimer;
        Rectangle[,] BoardRef;
        public Rectangle[,] smlBoardRef;
        CellModel[,] temp_table;
        
        int LiveCells = 0;
        #endregion
        #region Methods <----
        void CreateBoard()
        {
            cBoard.Children.Clear();
            BoardRef = new Rectangle[BoardSize, BoardSize];
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (i == 0 || i == BoardSize - 1 || j == 0 || j == BoardSize - 1)
                    {
                        Rectangle r = new Rectangle
                        {
                            Width = RectSize,
                            Height = RectSize,
                            Stroke = Brushes.White,
                            StrokeThickness = 0.5,
                            Fill = Brushes.White,
                        };
                        BoardRef[i, j] = r;
                        Canvas.SetLeft(r, j * RectSize);
                        Canvas.SetTop(r, i * RectSize);
                        cBoard.Children.Add(r);
                    }
                    else
                    {
                        CellModel Cell = new CellModel { State = false, Col = i, Ren = j };
                        Rectangle r = new Rectangle
                        {
                            Width = RectSize,
                            Height = RectSize,
                            Stroke = Brushes.Black,
                            StrokeThickness = 0.5,
                            Fill = Brushes.White,
                            Tag = Cell
                        };
                        r.MouseDown += R_MouseDown;
                        r.MouseEnter += R_MouseEnter;
                        BoardRef[i, j] = r;
                        Canvas.SetLeft(r, j * RectSize);
                        Canvas.SetTop(r, i * RectSize);
                        cBoard.Children.Add(r);
                    }
                }
            }
        }

        void CreateSmlBoard()
        {
            smlBoard.Children.Clear();
            smlBoardRef = new Rectangle[smlBoardSize, smlBoardSize];
            for (int i = 0; i < smlBoardSize; i++)
            {
                for (int j = 0; j < smlBoardSize; j++)
                {
                    if (i == 0 || i == smlBoardSize - 1 || j == 0 || j == smlBoardSize - 1)
                    {
                        Rectangle r = new Rectangle
                        {
                            Width = RectSize,
                            Height = RectSize,
                            Stroke = Brushes.White,
                            StrokeThickness = 0.5,
                            Fill = Brushes.White,
                        };
                        smlBoardRef[i, j] = r;
                        Canvas.SetLeft(r, j * RectSize);
                        Canvas.SetTop(r, i * RectSize);
                        smlBoard.Children.Add(r);
                    }
                    else
                    {
                        CellModel Cell = new CellModel { State = false, Col = i, Ren = j };
                        Rectangle r = new Rectangle
                        {
                            Width = RectSize,
                            Height = RectSize,
                            Stroke = Brushes.Black,
                            StrokeThickness = 0.5,
                            Fill = Brushes.White,
                            Tag = Cell
                        };
                        r.MouseDown += R_smlMouseDown;
                        r.MouseEnter += R_smlMouseEnter;
                        smlBoardRef[i, j] = r;
                        Canvas.SetLeft(r, j * RectSize);
                        Canvas.SetTop(r, i * RectSize);
                        smlBoard.Children.Add(r);
                    }
                }
            }
        }

        void StartGame()
        {
            dTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, CreationDelay) };
            dTimer.Tick += DispatcherTimer_Tick;
            dTimer.Start();
            IsGameStarted = true;
            btnStart.IsEnabled = btnReset.IsEnabled = random.IsEnabled = false;
            btnStop.IsEnabled = true;
        }
        void StopGame()
        {
            dTimer.Stop();
            IsGameStarted = false;
            btnStart.IsEnabled = btnReset.IsEnabled = random.IsEnabled = true;
            btnStop.IsEnabled = false;
        }
        void ResetGame()
        {
            foreach (var carre in BoardRef)
            {
                if (carre.Tag != null)
                {
                    var cellule = (CellModel)carre.Tag;
                    if (cellule.State)
                        ChangeCellState(cellule);
                }
            }
        }
        void ChangeCellState(CellModel cell)
        {
            if (!cell.State)
            {
                cell.State = true;
                BoardRef[cell.Col, cell.Ren].Fill = Brushes.Black;
                LiveCells++;
            }
            else
            {
                cell.State = false;
                BoardRef[cell.Col, cell.Ren].Fill = Brushes.White;
                LiveCells--;
                if (LiveCells == 0 && IsGameStarted)
                    StopGame();
            }
            lblLiveCells.Text = LiveCells.ToString();
        }
        public void ChangesmlCellState(CellModel cell)
        {
            if (!cell.State)
            {
                cell.State = true;
                smlBoardRef[cell.Col, cell.Ren].Fill = Brushes.Black;
            }
            else
            {
                cell.State = false;
                smlBoardRef[cell.Col, cell.Ren].Fill = Brushes.White;
            }
        }
        void ApplyRules()
        {
            List<CellModel> CellsToChange = new List<CellModel>();
            foreach (var cellRectref in BoardRef)
            {
                List<CellModel> neighbors;
                if (cellRectref.Tag != null)
                {
                    var tempCell = (CellModel)cellRectref.Tag;
                    neighbors = GetNeighbors(tempCell);
                    int neighborsCount = neighbors.Count(x => x.State == true);

                    if (tempCell.State) //Si la cellule est morte
                    {
                        if (neighborsCount < 2 || neighborsCount > 3)
                            CellsToChange.Add(tempCell);
                    }

                    else //Si la cellule est vivante
                    {
                        if (neighborsCount == 3)
                            CellsToChange.Add(tempCell);
                    }

                }
            }
            if (CellsToChange.Count != 0)
                ChangeCells(CellsToChange);
        }
        void ChangeCells(List<CellModel> cells)
        {
            foreach (var cell in cells)
            {
                ChangeCellState(cell);
            }
        }
        List<CellModel> GetNeighbors(CellModel cell)
        {
            List<CellModel> NeighborsList = new List<CellModel>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    var neighbour = BoardRef[cell.Col + i, cell.Ren + j];
                    if (neighbour.Tag != null)
                    {
                        var temp = (CellModel)neighbour.Tag;
                        if (temp.Col != cell.Col || temp.Ren != cell.Ren)
                            NeighborsList.Add(temp);
                    }
                }
            }
            return NeighborsList;
        }
        #endregion
        #region Events <----
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!IsGameStarted)
                StartGame();
        }
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (LiveCells > 0)
                ApplyRules();
            else
                dTimer.Stop();
        }
        private void R_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsGameStarted)
            {
                var cell = (CellModel)(sender as Rectangle).Tag;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        if ((bool)(((sender as Rectangle).Tag as CellModel).State) == false)
                        {
                            ChangeCellState(cell);
                        }
                        break;
                    case MouseButton.Right:
                        if ((bool)(((sender as Rectangle).Tag as CellModel).State) == true)
                        {
                            ChangeCellState(cell);
                        }
                        break;
                }
                
                
            }
        }

        private void R_smlMouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsGameStarted && Mouse.LeftButton == MouseButtonState.Pressed && (bool)(((sender as Rectangle).Tag as CellModel).State) == false || !IsGameStarted && Mouse.RightButton == MouseButtonState.Pressed && (bool)(((sender as Rectangle).Tag as CellModel).State) == true)
            {
                var cell = (CellModel)(sender as Rectangle).Tag;
                ChangesmlCellState(cell);
            }
        }

        private void R_smlMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsGameStarted)
            {
                var cell = (CellModel)(sender as Rectangle).Tag;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        if ((bool)(((sender as Rectangle).Tag as CellModel).State) == false)
                        {
                            ChangesmlCellState(cell);
                        }
                        break;
                    case MouseButton.Right:
                        if ((bool)(((sender as Rectangle).Tag as CellModel).State) == true)
                        {
                            ChangesmlCellState(cell);
                        }
                        break;
                }


            }
        }

        
       

        private void R_drag(object sender, MouseButtonEventArgs e)
        {
            temp_table = new CellModel[smlBoardSize, smlBoardSize];
            for (int i = 0; i < smlBoardSize; i++)
            {
                for (int j = 0; j < smlBoardSize; j++)
                {
                    if (smlBoardRef[i,j].Tag != null)
                    {
                        temp_table[i, j] = new CellModel();
                        temp_table[i, j].State = (smlBoardRef[i, j].Tag as CellModel).State;
                    }
                    
                }
            }
            dragged = true;
        }

        private void R_drop(object sender, MouseButtonEventArgs e)
        {
            if (dragged && !IsGameStarted)
            {
                
                int senderIndexI = 0;
                int senderIndexJ = 0;
                for (int i = BoardRef.GetLowerBound(0); i < BoardRef.GetUpperBound(0); i++)
                {
                    for (int j = BoardRef.GetLowerBound(1); j < BoardRef.GetUpperBound(1); j++)
                    {
                        if (BoardRef[i, j] == (sender as Rectangle))
                        {
                            senderIndexI = i;
                            senderIndexJ = j;
                            break;
                        }
                    }
                }
                for (int i = 0; i < temp_table.GetLength(0); i++)
                {
                    for (int j = 0; j < temp_table.GetLength(1); j++)
                    {
                        if (i == 0 || j == 0 || i == temp_table.GetLength(0) - 1 || j == temp_table.GetLength(1) - 1)
                        {

                        }
                        else
                        {
                            try
                            {
                                if (temp_table[i, j].State && (BoardRef[senderIndexI + i, senderIndexJ + j].Tag as CellModel).State != temp_table[i, j].State)
                                {
                                    ChangeCellState((BoardRef[senderIndexI + i, senderIndexJ + j].Tag as CellModel));
                                }
                            }
                            catch
                            {

                                break;
                            }
                        }
                        
                    }
                }
                dragged = false;
            }
        }

        private void R_MouseEnterGrid(object sender, MouseEventArgs e)
        {
            if (dragged && !IsGameStarted)
            {
                int senderIndexI = 0;
                int senderIndexJ = 0;
                for (int i = BoardRef.GetLowerBound(0); i < BoardRef.GetUpperBound(0); i++)
                {
                    for (int j = BoardRef.GetLowerBound(1); j < BoardRef.GetUpperBound(1); j++)
                    {
                        if (BoardRef[i, j] == (sender as Rectangle))
                        {
                            senderIndexI = i;
                            senderIndexJ = j;
                            break;
                        }
                    }
                }
                for (int i = 0; i < temp_table.GetLength(0); i++)
                {
                    for (int j = 0; j < temp_table.GetLength(1); j++)
                    {
                        if (i == 0 || j == 0 || i == temp_table.GetLength(0) - 1 || j == temp_table.GetLength(1) - 1)
                        {

                        }
                        else
                        {
                            try {
                                if (!(BoardRef[senderIndexI + i, senderIndexJ + j].Tag as CellModel).State && (BoardRef[senderIndexI + i, senderIndexJ + j].Tag as CellModel).State != temp_table[i, j].State)
                                {
                                    BoardRef[senderIndexI + i, senderIndexJ + j].Fill = Brushes.Gray;
                                }
                            }
                            catch { break; }
                            
                        }

                    }
                }
            }
        }

        private void R_MouseLeaveGrid(object sender, MouseEventArgs e)
        {
            if (dragged && !IsGameStarted)
            {
                int senderIndexI = 0;
                int senderIndexJ = 0;
                for (int i = BoardRef.GetLowerBound(0); i < BoardRef.GetUpperBound(0); i++)
                {
                    for (int j = BoardRef.GetLowerBound(1); j < BoardRef.GetUpperBound(1); j++)
                    {
                        if (BoardRef[i, j] == (sender as Rectangle))
                        {
                            senderIndexI = i;
                            senderIndexJ = j;
                            break;
                        }
                    }
                }
                for (int i = 0; i < temp_table.GetLength(0); i++)
                {
                    for (int j = 0; j < temp_table.GetLength(1); j++)
                    {
                        if (i == 0 || j == 0 || i == temp_table.GetLength(0) - 1 || j == temp_table.GetLength(1) - 1)
                        {

                        }
                        else
                        {
                            try
                            {
                                if (!(BoardRef[senderIndexI + i, senderIndexJ + j].Tag as CellModel).State && (BoardRef[senderIndexI + i, senderIndexJ + j].Tag as CellModel).State != temp_table[i, j].State)
                                {
                                    BoardRef[senderIndexI + i, senderIndexJ + j].Fill = Brushes.White;
                                }
                            }
                            catch { break; }

                        }

                    }
                }
            }
        }

        private void R_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsGameStarted && Mouse.LeftButton == MouseButtonState.Pressed && (bool)(((sender as Rectangle).Tag as CellModel).State) == false || !IsGameStarted && Mouse.RightButton == MouseButtonState.Pressed && (bool)(((sender as Rectangle).Tag as CellModel).State) == true)
            {
                var cell = (CellModel)(sender as Rectangle).Tag;
                ChangeCellState(cell);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateBoard();
            CreateSmlBoard();
        }


        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopGame();
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (!IsGameStarted)
                ResetGame();
        }
        private void btnSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (select_speed == 3)
            {
                select_speed = 0;
            }
            else
            {
                select_speed++;
            }
            CurrentSpeed.Text = speeds[select_speed].ToString();
            CreationDelay = ((int)(100 / speeds[select_speed]));
            if (IsGameStarted)
                dTimer.Interval = new TimeSpan(0, 0, 0, 0, CreationDelay);
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            Random rnd = new Random();
            foreach (var carre in BoardRef)
            {
                int var = rnd.Next(0, 2);
                if (carre.Tag != null)
                {

                    var cellule = (CellModel)carre.Tag;
                    if (var == 1)
                    {
                        ChangeCellState(cellule);
                    }
                    
                }
            }
        }
        private void OnLock_Click(object sender, RoutedEventArgs e)
        {
            if (!isLocked)
            {
                smlBoardLock.Content = "Unlock";
                for (int i = 0; i < smlBoardSize; i++)
                {
                    for (int j = 0; j < smlBoardSize; j++)
                    {
                        if (i == 0 || i == smlBoardSize - 1 || j == 0 || j == smlBoardSize - 1) { }
                        else
                        {
                            smlBoardRef[i, j].MouseDown -= R_smlMouseDown;
                            smlBoardRef[i, j].MouseEnter -= R_smlMouseEnter;
                            smlBoardRef[i, j].MouseDown += R_drag;
                        }
                    }
                }
                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        if (i == 0 || i == BoardSize - 1 || j == 0 || j == BoardSize - 1) { }
                        else
                        {
                            BoardRef[i, j].MouseDown -= R_MouseDown;
                            BoardRef[i, j].MouseEnter -= R_MouseEnter;
                            BoardRef[i, j].MouseEnter += R_MouseEnterGrid;
                            BoardRef[i, j].MouseUp += R_drop;
                            BoardRef[i, j].MouseLeave += R_MouseLeaveGrid;
                            
                        }
                    }
                }

            }
            else
            {
                smlBoardLock.Content = "Lock";
                for (int i = 0; i < smlBoardSize; i++)
                {
                    for (int j = 0; j < smlBoardSize; j++)
                    {
                        if (i == 0 || i == smlBoardSize - 1 || j == 0 || j == smlBoardSize - 1) { }
                        else
                        {
                            smlBoardRef[i, j].MouseDown += R_smlMouseDown;
                            smlBoardRef[i, j].MouseEnter += R_smlMouseEnter;
                            smlBoardRef[i, j].MouseDown -= R_drag;
                        }
                            
                    }
                }
                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        if (i == 0 || i == BoardSize - 1 || j == 0 || j == BoardSize - 1) { }
                        else
                        {
                            BoardRef[i, j].MouseDown += R_MouseDown;
                            BoardRef[i, j].MouseEnter += R_MouseEnter;
                            BoardRef[i, j].MouseEnter -= R_MouseEnterGrid;
                            BoardRef[i, j].MouseUp -= R_drop;
                            BoardRef[i, j].MouseLeave -= R_MouseLeaveGrid;

                        }
                    }
                }
            }
            isLocked = !isLocked;

        }

        private void btnsmlReset_Click(object sender, RoutedEventArgs e)
        {
            foreach (var carre in smlBoardRef)
            {
                if (carre.Tag != null)
                {
                    var cellule = (CellModel)carre.Tag;
                    if (cellule.State)
                        ChangesmlCellState(cellule);
                }
            }
        }

        

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string sb = "";
            for (int i = 0; i < smlBoardRef.GetLength(0); i++)
            {   
                for (int j = 0; j < smlBoardRef.GetLength(1); j++)
                {
                    if (smlBoardRef[i, j].Tag != null)
                    {
                        sb += (((smlBoardRef[i, j].Tag as CellModel).State) ? "1" : "0");
                    }
                    
                }
            }

            Save_name sn = new Save_name();
            sn.pattern = sb;
            sn.Show();

            Test_sb.Content = sb;
            


        }

        public void SaveOnDataBase(string sb, string name)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(database.Insert_pannel(), mySqlConnection);
            mySqlCommand.Parameters.AddWithValue("@form", sb);
            mySqlCommand.Parameters.AddWithValue("@name", name);
            mySqlConnection.Open();

            mySqlCommand.ExecuteNonQuery();
            mySqlConnection.Close();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            
            forms_database forms_Database = new forms_database();
            forms_Database.Show();

        }

        

        #endregion
    }
}
