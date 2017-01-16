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
    /// Логика взаимодействия для DebtsWindow.xaml
    /// </summary>
    public partial class DebtsWindow : Window
    {
        public DebtsWindow(List<Debt> _debts)
        {
            InitializeComponent();
            dataGridDebts.ItemsSource = _debts;

            decimal sum = 0;
            foreach (Debt d in _debts)
                sum += d.Value;
            txtBlockSummary.Text = "Всего: " + sum.ToString();
        }
    }
}
