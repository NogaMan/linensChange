using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace LinensChange
{
    public class Manager
    {
        public const string ServerName = "ASUS-PC";
        public const string DBName = "Students";
        public const string Login = "Matron";
        public const string Password = "1";
        public const string DormitoryID = "8";
        public Student CurrentStudent { get; set; }

        public SqlConnection Connection { get; set; }

        public Manager()
        {
            Connection = new SqlConnection(String.Format("Server = {0}; Database = {1}; User = {2}; Password = {3};", ServerName, DBName, Login, Password));
            Connection.Open();
        }

        public List<Student> SearchStudentsByName(string searchName)
        {
            SqlCommand com = new SqlCommand("EXEC dbo.SearchStudentsByName @name, @dormitoryID", Connection);
            com.Parameters.AddWithValue("@name", searchName);
            com.Parameters.AddWithValue("@dormitoryID", DormitoryID);

            SqlDataReader reader = com.ExecuteReader();
            List<Student> _return = GetStudentsFromProcedure(reader);
            reader.Close();
            return _return;
        }

        public List<Student> SearchStudentsByDocumentNumber(string number)
        {
            SqlCommand com = new SqlCommand("EXEC dbo.SearchStudentsByDocumentNumber @number, @dormitoryID", Connection);
            com.Parameters.AddWithValue("@number", number);
            com.Parameters.AddWithValue("@dormitoryID", DormitoryID);

            SqlDataReader reader = com.ExecuteReader();
            List<Student> _return = GetStudentsFromProcedure(reader);
            reader.Close();
            return _return;
        }

        public List<Student> SearchStudentsByBirthDate(DateTime date)
        {
            SqlCommand com = new SqlCommand("EXEC dbo.SearchStudentsByBirthDate @date, @dormitoryID", Connection);
            com.Parameters.AddWithValue("@date", date);
            com.Parameters.AddWithValue("@dormitoryID", DormitoryID);

            SqlDataReader reader = com.ExecuteReader();
            List<Student> _return = GetStudentsFromProcedure(reader);
            reader.Close();
            return _return;
        }

        public Student ReadMagnetPass(string number)
        {
            SqlCommand com = new SqlCommand("EXEC dbo.GetStudentByMagnetPassNumber @number, @dormitoryID", Connection);
            com.Parameters.AddWithValue("@number", number);
            com.Parameters.AddWithValue("@dormitoryID", DormitoryID);
            SqlDataReader reader = com.ExecuteReader();
            List<Student> _return = GetStudentsFromProcedure(reader);
            reader.Close();
            return _return[0];
        }

        public void ChangeLinens(int typeID)
        {
            SqlCommand com = new SqlCommand("EXEC dbo.Change @studentID, @typeID", Connection);
            com.Parameters.AddWithValue("@studentID", CurrentStudent.ID);
            com.Parameters.AddWithValue("@typeID", typeID);

            SqlDataReader reader = com.ExecuteReader();
            List<Student> _return = GetStudentsFromProcedure(reader);
            reader.Close();
        }

        public List<Debt> GetDebts()
        {
            SqlCommand com = new SqlCommand("EXEC dbo.GetDebtsByStudentsID @studentID", Connection);
            com.Parameters.AddWithValue("@studentID", CurrentStudent.ID);

            SqlDataReader reader = com.ExecuteReader();
            List<Debt> _return = GetDebtsFromProcedure(reader);
            reader.Close();
            return _return;
        }

        public List<HistoryItem> GetHistory()
        {
            SqlCommand com = new SqlCommand("EXEC dbo.GetHistoryByStudentsID @studentID", Connection);
            com.Parameters.AddWithValue("@studentID", CurrentStudent.ID);

            SqlDataReader reader = com.ExecuteReader();
            List<HistoryItem> _return = GetHistoryFromProcedure(reader);
            reader.Close();
            return _return;
        }

        public void SinkDebt(int id)
        {
            SqlCommand com = new SqlCommand("EXEC dbo.SinkDebtByID @debtID", Connection);
            com.Parameters.AddWithValue("@debtID", id);

            com.ExecuteNonQuery();
        }

        private List<Student> GetStudentsFromProcedure(SqlDataReader reader)
        {
            List<Student> _return = new List<Student>();
            while (reader.Read())
            {
                if (_return == null)
                    _return = new List<Student>();
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string lastname = reader.GetString(2);
                string patronymic = "";
                if (!reader.IsDBNull(3))
                {
                    patronymic = reader.GetString(3);
                }
                char gender = reader.GetString(4)[0];
                DateTime birthdate = reader.GetDateTime(5);
                string passNumber = "";
                if (!reader.IsDBNull(6))
                {
                    passNumber = reader.GetString(6);
                }
                string otherDocumentNumber = "";
                if (!reader.IsDBNull(7))
                {
                    otherDocumentNumber = reader.GetString(7);
                }

                Student s = new Student(id, name, lastname, patronymic, gender, birthdate, passNumber, otherDocumentNumber);
                _return.Add(s);
            }
            return _return;
        }

        private List<Debt> GetDebtsFromProcedure(SqlDataReader reader)
        {
            List<Debt> _return = new List<Debt>();
            while (reader.Read())
            {
                if (_return == null)
                    _return = new List<Debt>();
                int id = reader.GetInt32(0);
                decimal value = reader.GetDecimal(2);
                DateTime date = reader.GetDateTime(3);
                Debt d = new Debt(id, CurrentStudent, value, date);
                _return.Add(d);
            }
            return _return;
        }

        private List<HistoryItem> GetHistoryFromProcedure(SqlDataReader reader)
        {
            List<HistoryItem> _return = new List<HistoryItem>();
            while (reader.Read())
            {
                if (_return == null)
                    _return = new List<HistoryItem>();
                int id = reader.GetInt32(0);
                DateTime date = reader.GetDateTime(1);
                string name = reader.GetString(2);
                decimal value = reader.GetDecimal(3);

                HistoryItem h = new HistoryItem(id, CurrentStudent, date, name, value);
                _return.Add(h);
            }
            return _return;
        }
    }
}
