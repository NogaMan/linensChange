using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinensChange
{
    public class HistoryItem
    {
        public int ID { get; set; }
        public Student HistoryStudent { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }

        public HistoryItem(int id, Student student, DateTime date, string name, decimal value)
        {
            ID = id;
            HistoryStudent = student;
            Date = date;
            Name = name.TrimEnd();
            Value = value;
        }

        public string GetShortDate
        {
            get { return Date.ToShortDateString(); }
        }
    }
}
