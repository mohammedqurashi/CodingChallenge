using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Calculation
{
    public class FileWriter
    {
        /// <summary>
        /// Write CSV file
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="res"></param>
        /// <param name="opn"></param>
        /// <param name="costs"></param>
        /// <param name="costTrue"></param>
        /// <param name="fileName"></param>
        public void WriteCSV(List<Combination> combi, List<Resource> res, List<Openning> opn, string fileName)
        {
            string filePath = @"" + fileName;
            string delimiter = ",";
            StringBuilder sb = new StringBuilder();

            //header to csv
            sb.AppendLine("RequestID,EmployeeID");

            foreach (var item in combi)
            {

                var rs = res.Find(r => r.EmployeeId == item.EmpolyeeId);
                var op = opn.Find(o => o.RequestId == item.RequestId);
                var cst = item.Cost;
                double score = (1000 - cst) * 0.01;
          
                sb.AppendLine(string.Join(delimiter, op.RequestId,rs.EmployeeId));
            }




            File.WriteAllText(filePath, sb.ToString());

        }
    }
}
