using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinensChange
{
    public class Student
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public char Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string PassportNumber { get; set; }
        public string OtherDocumentNumber { get; set; }

        public Student(int id, string name, string lastName, string patronymic, char gender, DateTime birthDate, string passportNumber, string otherDocumentNumber)
        {
            ID = id;
            FirstName = name.TrimEnd();
            LastName = lastName.TrimEnd();
            Patronymic = patronymic.TrimEnd();
            Gender = gender;
            BirthDate = birthDate;
            PassportNumber = passportNumber.TrimEnd();
            OtherDocumentNumber = otherDocumentNumber.TrimEnd();
        }

        public string GetShortBirthDate
        {
            get { return BirthDate.ToShortDateString(); }
        }

        public string GetDocumentNumber
        {
            get 
            {
                if (PassportNumber != null)
                    return PassportNumber;
                else
                    if (OtherDocumentNumber != null)
                        return OtherDocumentNumber;
                    else
                        return "";
            }
        }
    }
}
