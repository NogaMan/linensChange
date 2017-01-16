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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LinensChange
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Manager manager;
        CardReader reader;
        bool manualAuth;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            manager = new Manager();
            reader = new CardReader();
            manualAuth = false;
            reader.CardConnected += (card) =>
            {
                try
                {
                    if (manualAuth)
                        throw new Exception("Открыто окно поиска");
                    if (manager.CurrentStudent != null)
                        throw new Exception("Студент уже выбран");

                    manager.CurrentStudent = null;
                    Dispatcher.Invoke(() => ClearInfo());
                    manager.CurrentStudent = manager.ReadMagnetPass(card.TextUId);
                    Dispatcher.Invoke(FillInfo);
                    Dispatcher.Invoke(EnableControls);
                    Dispatcher.Invoke(() => ShowMessage("Карта распознана", false));
                }
                catch (Exception exc)
                {
                    Dispatcher.Invoke(() => ShowMessage(exc.Message, true));
                }
            };
        }

        private void FillInfo()
        {
            txtBlockFirstName.Text = manager.CurrentStudent.FirstName;
            txtBlockLastName.Text = manager.CurrentStudent.LastName;
            txtBlockPatronymic.Text = manager.CurrentStudent.Patronymic;
            txtBlockBirthDate.Text = manager.CurrentStudent.BirthDate.ToShortDateString();
        }

        private void ClearInfo()
        {
            txtBlockFirstName.Text = "-";
            txtBlockLastName.Text = "-";
            txtBlockPatronymic.Text = "-";
            txtBlockBirthDate.Text = "-";
        }

        private void EnableControls()
        {
            gridInfoAndActions.IsEnabled = true;
            gridTypes.IsEnabled = true;
        }

        private void DisableControls()
        {
            gridInfoAndActions.IsEnabled = false;
            gridTypes.IsEnabled = false;
        }

        private void ShowMessage(string msg, bool isError)
        {
            gridMessage.SetValue(Grid.ZIndexProperty, 1);
            txtBlockMessage.Text = msg;

            if (isError)
                rectangleMessage.Fill = Brushes.LightPink;
            else
                rectangleMessage.Fill = Brushes.LightCyan;

            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            DoubleAnimation delayAnimation = new DoubleAnimation();
            DoubleAnimation fadeOutAnimation = new DoubleAnimation();

            fadeInAnimation.From = 0;
            fadeInAnimation.To = 1;
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(200);

            delayAnimation.From = 1;
            delayAnimation.To = 1;
            delayAnimation.Duration = TimeSpan.FromMilliseconds(1000);

            fadeOutAnimation.From = 1;
            fadeOutAnimation.To = 0;
            fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(200);

            fadeInAnimation.Completed += (sender, e) => gridMessage.BeginAnimation(Grid.OpacityProperty, delayAnimation);
            delayAnimation.Completed += (sender, e) => gridMessage.BeginAnimation(Grid.OpacityProperty, fadeOutAnimation);
            fadeOutAnimation.Completed += (sender, e) => gridMessage.SetValue(Grid.ZIndexProperty, -1);
            gridMessage.BeginAnimation(Grid.OpacityProperty, fadeInAnimation);
        }

        #region handlers
        private void btnSearchStudent_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow window = new SearchWindow(manager);
            manualAuth = true;
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                manager.CurrentStudent = window.GetStudent;
                FillInfo();
                EnableControls();
                ShowMessage("Выполнено", false);
            }
            manualAuth = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            manager.CurrentStudent = null;
            ClearInfo();
            DisableControls();
        }


        private void btnFull_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                manager.ChangeLinens(1);
                ShowMessage("Выполнено", false);
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void btnWithoutTowels_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                manager.ChangeLinens(2);
                ShowMessage("Выполнено", false);
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void btnTowelsOnly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                manager.ChangeLinens(3);
                ShowMessage("Выполнено", false);
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void btnTowelsCustom_Click(object sender, RoutedEventArgs e)
        {
            CustomChangeWindow window = new CustomChangeWindow();
            window.ShowDialog();

            try
            {
                if (window.DialogResult == true)
                    foreach (int id in window.GetIDs)
                        manager.ChangeLinens(id);
                ShowMessage("Выполнено", false);
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void btnDebt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DebtsWindow window = new DebtsWindow(manager.GetDebts());
                window.ShowDialog();
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void btnSinkDebt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SinkDebtsWindow window = new SinkDebtsWindow(manager.GetDebts());
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    foreach (int id in window.GetIDs)
                        manager.SinkDebt(id);
                }
                ShowMessage("Выполнено", false);
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HistoryWindow window = new HistoryWindow(manager.GetHistory());
                window.ShowDialog();
            }
            catch (Exception exc)
            {
                ShowMessage(exc.Message, true);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            reader.Dispose();
        }

        #endregion
    }
}
