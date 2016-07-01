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
            string filePath = @"xml\" + fileName;
            string delimiter = ",";
            StringBuilder sb = new StringBuilder();

            //header to csv
            sb.AppendLine("EmployeeId,RequestId,Skills,MandotaroySkilss,AvlDate,RequestStartDate,AllocationEndDate,PreviousCustomerExperiance,CustomerName,DomainExperiance,ProjectDomain,ProjectKey,NAGP,Rating,IsKeyProject,IsKeyPosition,cost1,cost2,score");

            foreach (var item in combi)
            {

                var rs = res.Find(r => r.EmployeeId == item.EmpolyeeId);
                var op = opn.Find(o => o.RequestId == item.RequestId);
                var cst = item.Cost;
                double score = (1000 - cst) * 0.01;
                sb.AppendLine(string.Join(delimiter, rs.EmployeeId,
                                                     op.RequestId,
                                                     String.Join("+", rs.Skills.ToArray()),
                                                     op.MandotaroySkilss.Count() > 0 ? String.Join("+", op.MandotaroySkilss.ToArray()) : "",
                                                     rs.AvlDate,
                                                     op.RequestStartDate,
                                                     op.AllocationEndDate,
                                                     rs.PreviousCustomerExperiance.Count() > 0 ? String.Join("+", rs.PreviousCustomerExperiance.ToArray()) : "",
                                                     op.CustomerName,
                                                     rs.DomainExperiance.Count() > 0 ? String.Join("+", rs.DomainExperiance.ToArray()) : "",
                                                     op.ProjectDomain.Count() > 0 ? String.Join("+", op.ProjectDomain.ToArray()) : "",
                                                     op.ProjectKey,
                                                     rs.NAGP,
                                                     rs.Rating,
                                                     op.IsKeyProject,
                                                     op.IsKeyPosition,
                                                     cst,
                                                     cst,
                                                     score));
            }




            File.WriteAllText(filePath, sb.ToString());

        }
    }
}
