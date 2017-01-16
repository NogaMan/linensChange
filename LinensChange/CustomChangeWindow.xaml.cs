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

namespace LinensChange
{
    /// <summary>
    /// Логика взаимодействия для CustomChangeWindow.xaml
    /// </summary>
    public partial class CustomChangeWindow : Window
    {
        List<int> ids;
        public CustomChangeWindow()
        {
            InitializeComponent();
            ids = new List<int>();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            ids.Clear();
            if (checkBoxNavo.IsChecked == true)
                ids.Add(4);
            if (checkBoxPodo.IsChecked == true)
                ids.Add(5);
            if (checkBoxPros.IsChecked == true)
                ids.Add(6);

            if (checkBoxSmallTowel.IsChecked == true)
                ids.Add(7);
            if (checkBoxLargeTowel.IsChecked == true)
                ids.Add(8);

            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public List<int> GetIDs
        {
            get { return ids; }
        }
    }
}
