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
    /// Логика взаимодействия для SinkDebtsWindow.xaml
    /// </summary>
    public partial class SinkDebtsWindow : Window
    {
        private List<Debt> Debts;
        private List<int> ids;

        public SinkDebtsWindow(List<Debt> _debts)
        {
            InitializeComponent();
            Debts = _debts;
            dataGridDebts.ItemsSource = Debts;
            ids = new List<int>();
        }

        public List<int> GetIDs
        {
            get { return ids; }
        }

        private void btnSink_Click(object sender, RoutedEventArgs e)
        {
            foreach (Debt d in Debts)
                if (d.ChosenForSinking)
                    ids.Add(d.DebtID);
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
