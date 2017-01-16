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
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private Manager manager;
        private Student student;

        public SearchWindow(Manager m)
        {
            InitializeComponent();
            manager = m;
        }

        public Student GetStudent
        {
            get { return student; }
        }

        private void btnByName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtBoxByName.Text;
                List<Student> _students = manager.SearchStudentsByName(name);
                dataGridSearchResults.ItemsSource = _students;
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnByDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string number = txtBoxByDocument.Text;
                List<Student> _students = manager.SearchStudentsByDocumentNumber(number);
                dataGridSearchResults.ItemsSource = _students;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnByBirthDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime date;
                if (datePickerByBirthDate.SelectedDate != null)
                {
                    date = (DateTime)datePickerByBirthDate.SelectedDate;
                }
                else
                {
                    throw new Exception("Дата не выбрана");
                }
                List<Student> _students = manager.SearchStudentsByBirthDate(date);
                dataGridSearchResults.ItemsSource = _students;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void dataGridSearchResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGridSearchResults.SelectedItem != null)
            {
                student = (Student)dataGridSearchResults.SelectedItem;
                this.DialogResult = true;
            }
        }
    }
}
