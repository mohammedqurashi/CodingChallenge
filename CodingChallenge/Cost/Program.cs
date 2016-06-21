using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Calculation
{
    class Program
    {
        static void Main(string[] args)
        {
        

            
            Console.Title = "Coding Challenge";
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Start:" + sw.Elapsed);
            List<Resource> resources = new List<Resource>();
            List<Openning> openings = new List<Openning>();
            IFormatProvider culture = new System.Globalization.CultureInfo("hi-IN", true);

          //  Console.WriteLine("Start Resource Load:" + sw.Elapsed);
            foreach (var item in LoadXML("xml/resources.xml").AsEnumerable())
            {
                Resource res = new Resource();
                res.EmployeeId = Convert.ToInt32(item["EmployeeID"]);
                res.DOJ = Convert.ToDateTime(item["DOJ"], culture);
                res.Skills = PrepareSkillList(item["Skills"].ToString().ToLower());
                res.DomainExperiance = item["DomainExperience"].ToString().ToLower().Split(',').ToList<string>();
                res.Rating = item["Rating"].ToString();
                res.CommunicationRating = item["CommunicationsRating"].ToString();
                res.NAGP = item["NAGP"].ToString() == "Y" ? true : false;
                res.YearsOfExperiance = Convert.ToDouble(item["YearsOfExperience"]);
                res.CurrentRole = item["CurrentRole"].ToString();
                res.PreviousCustomerExperiance = item["PreviousCustomerExperience"].ToString().Split(',').ToList<string>();
                res.AvlDate = Convert.ToDateTime(item["AvailableFromDate"], culture);
                resources.Add(res);
            }
            // Console.WriteLine("End Resource Load:" + sw.Elapsed);
            //Console.WriteLine("Start Opening Load:" + sw.Elapsed);
            foreach (var item in LoadXML("xml/openings.xml").AsEnumerable())
            {
                Openning opnn = new Openning();
                opnn.RequestId = Convert.ToInt32(item["RequestID"]);
                opnn.ClientKey = item["ClientKey"].ToString();
                opnn.ProjectKey = item["ProjectKey"].ToString();
                opnn.CustomerName = item["CustomerName"].ToString();
                opnn.ProjectName = item["ProjectName"].ToString();
                opnn.IsKeyProject = item["IsKeyProject"].ToString() == "Y" ? true : false;
                opnn.ProjectStartDate = Convert.ToDateTime(item["ProjectStartDate"], culture);
                opnn.ProjectEndDate = Convert.ToDateTime(item["ProjectEndDate"], culture);
                opnn.Role = item["Role"].ToString();
                opnn.IsKeyPosition = item["IsKeyPosition"].ToString() == "Y" ? true : false;
                opnn.YearsOfExperiance = Convert.ToDouble(item["YearsOfExperience"]);
                opnn.MandotaroySkilss = item["MandatorySkills"].ToString().ToLower().Split(',').ToList<string>();
                opnn.ClientCommunication = item["ClientCommunication"].ToString() == "Y" ? true : false;
                opnn.RequestStartDate = Convert.ToDateTime(item["RequestStartDate"], culture);
                opnn.AllocationEndDate = Convert.ToDateTime(item["AllocationEndDate"], culture);
                openings.Add(opnn);
            }
            //Console.WriteLine("End Opening Load:" + sw.Elapsed);
            int[,] costs = new int[resources.Count(), openings.Count()];
            int[,] newcost = new int[resources.Count(), openings.Count()];
            int i = 0, j = 0;
            Scoring score = new Scoring();
            //Console.WriteLine("Start Matrix Preparation :" + sw.Elapsed);
            foreach (var res in resources)
            {

                foreach (var opnn in openings)
                {
                    costs[i, j] = newcost[i, j] = score.CalculatedIndivisualScore(res, opnn);
                    j++;
                }

                i++;
                j = 0;
            }

          
            //Console.WriteLine("End Matrix Preparation:" + sw.Elapsed);
            //Console.WriteLine("Start Calculation :" + sw.Elapsed);
            var resourceAssignments = HungarianAlgorithm.FindAssignments(costs);
            Console.WriteLine("End Calculation :" + sw.Elapsed);
            sw.Stop();

            Console.WriteLine("End Calculation :");

            WriteCSV(resourceAssignments, resources, openings, newcost);
            Console.WriteLine("End");
            Console.ReadKey();
        }

        /// <summary>
        /// Prepare skill list
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private static List<string> PrepareSkillList(string skill) {
            var skillList = skill.Split(',');
            var mainTech = "";
            foreach (var item in skillList)
            {
                if (item.Contains("expert"))
                {
                    Array.Resize(ref skillList, skillList.Length + 2);
                    mainTech = item.Substring(0, item.IndexOf('-'));
                    skillList[skillList.Length - 2] = mainTech + "-intermediate";
                    skillList[skillList.Length -1] = mainTech + "-beginner";
                }
                else if (item.Contains("intermediate"))
                {
                    Array.Resize(ref skillList, skillList.Length + 1);
                    mainTech = item.Substring(0, item.IndexOf('-'));
                    skillList[skillList.Length - 1] = mainTech + "-beginner";
                }
                mainTech = "";
            }

            return skillList.ToList<string>();
        }

        /// <summary>
        /// Write CSV file
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="res"></param>
        /// <param name="opn"></param>
        /// <param name="costs"></param>
        private static void WriteCSV(int[] resourceAssignments,List<Resource> res, List<Openning> opn, int[,] costs)
        {
            string filePath = @"xml\output.csv";
            string delimiter = ",";
            int indx = 0;
            StringBuilder sb = new StringBuilder();

                foreach (var assignmentID in resourceAssignments)
                {
                    var rs = res.ElementAt(indx);
                    var op = opn.ElementAt(assignmentID);
                    sb.AppendLine(string.Join(delimiter, rs.EmployeeId, 
                                                         String.Join("+", rs.Skills.ToArray()), 
                                                         rs.AvlDate,
                                                         String.Join("+", op.MandotaroySkilss.ToArray()), 
                                                         op.RequestId, 
                                                         op.ProjectKey,
                                                         op.ProjectName,
                                                         op.RequestStartDate,
                                                         op.AllocationEndDate,
                                                         assignmentID, 
                                                         costs[indx, assignmentID]));
                    indx++;
                }
                    
           
              File.WriteAllText(filePath, sb.ToString());

        }

        /// <summary>
        /// Load data from xml to datatable
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static DataTable LoadXML(string filePath)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(filePath, XmlReadMode.Auto);
            return ds.Tables[0];
        }
    }
}
