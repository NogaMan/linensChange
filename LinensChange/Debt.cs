using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinensChange
{
    public class Debt
    {
        public int DebtID { get; set; }
        public Student Debtor { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public bool ChosenForSinking { get; set; }

        public Debt (int id, Student debtor, decimal value, DateTime date)
        {
            DebtID = id;
            Debtor = debtor;
            Value = value;
            Date = date;
            ChosenForSinking = true;
        }

        public string GetShortDate
        {
            get { return Date.ToShortDateString(); }
        }
    }
}
