using System;
using System.Collections.Generic;

namespace Calculation
{
    public class Openning
    {
       
        public int RequestId { get; set; }
        public string ClientKey { get; set; }
        public string ProjectKey { get; set; }
        public string CustomerName { get; set; }
        public string ProjectName { get; set; }
        public bool IsKeyProject { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public string Role { get; set; }
        public bool IsKeyPosition { get; set; }
        public double YearsOfExperiance { get; set; }
        public List<string> MandotaroySkilss { get; set; }
        public bool ClientCommunication { get; set; }
        public DateTime RequestStartDate { get; set; }
        public DateTime AllocationEndDate { get; set; }
        public List<string> ProjectDomain { get; set; }

    }
}
