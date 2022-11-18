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
    /// Logique d'interaction pour Save_name.xaml
    /// </summary>
    public partial class Save_name : Window
    {
        public string pattern;
        public Save_name()
        {
            InitializeComponent();
        }

        public void save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            if (name_of_form.Text != "")
            {
                mw.SaveOnDataBase(pattern, name_of_form.Text);
                Close();
            }
            
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
