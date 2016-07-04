using System;
using System.Collections.Generic;

namespace Calculation
{
    public class Resource
    {
        public string EmployeeId { get; set; }
        public DateTime DOJ { get; set; }
        public List<string> Skills { get; set; }
        public string Rating { get; set; }
        public string CommunicationRating { get; set; }
        public bool NAGP { get; set; }
        public double YearsOfExperiance { get; set; }
        public string CurrentRole { get; set; }
        public List<string> PreviousCustomerExperiance { get; set; }
        public DateTime AvlDate { get; set; }
        public List<string> DomainExperiance { get; set; }
    }
}
