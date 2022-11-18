using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameOfLife
{
    /// <summary>
    /// Logique d'interaction pour forms_database.xaml
    /// </summary>
    public partial class forms_database : Window
    {
        #region <-- Variables
        public const int smlBoardSize = 12;
        const int RectSize = 10;
        public Rectangle[,] smlBoardRef;
        #endregion

        static database database = new database();
        MySqlConnection mySqlConnection = new MySqlConnection(database.Initialize());
        public forms_database()
        {
            InitializeComponent();
            MySqlCommand mySqlCommand = new MySqlCommand(database.GetNumberForm(), mySqlConnection);
            MySqlDataReader reader;
            mySqlConnection.Open();
            reader = mySqlCommand.ExecuteReader();
            reader.Read();
            int nb_forms = reader.GetInt32(0);
            mySqlConnection.Close();
            for (int i = 0; i < nb_forms; i++)
            {
                CreateSmlBoard();
                LoadElement(i);
            }
        }

        private void binary_to_smlBoard(string binary_number)
        {
            char[] binary_numbers = binary_number.ToCharArray();
            for (int i = 0; i < smlBoardSize; i++)
            {

                for (int j = 0; j < smlBoardSize; j++)
                {
                    if (smlBoardRef[i, j].Tag != null)
                    {

                        char current_binary_number = binary_numbers[(smlBoardSize - 2) * (i - 1) + (j - 1)];
                        if (current_binary_number == '1' && (smlBoardRef[i, j].Tag as CellModel).State == false || current_binary_number == '0' && (smlBoardRef[i, j].Tag as CellModel).State == true)
                        {
                            ChangesmlCellState(smlBoardRef[i, j].Tag as CellModel);
                        }
                    }

                }
            }
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



        private void LoadElement(int id)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(database.GetFormValue(id), mySqlConnection);
            MySqlDataReader reader;
            string decimal_number = "";
            mySqlConnection.Open();
            reader = mySqlCommand.ExecuteReader();
            reader.Read();
            decimal_number = reader["form"].ToString();
            
            mySqlConnection.Close();
            
            binary_to_smlBoard(decimal_number);
        }

        void CreateSmlBoard()
        {
            Canvas smlBoard = new Canvas();
            smlBoard.Margin = new Thickness(0, 10, 0, 0);
            forms.Content = smlBoard;
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
                        smlBoardRef[i, j] = r;
                        Canvas.SetLeft(r, j * RectSize);
                        Canvas.SetTop(r, i * RectSize);
                        smlBoard.Children.Add(r);
                    }
                }
            }
        }
    }
}
