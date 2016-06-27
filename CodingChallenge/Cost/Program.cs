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

            var resources = ResourceMapping(); //Load and map resouces from input xml
            var openings = OpeningMapping();   //Load amd map opening from input xml

            int[,] newcostTrue = new int[resources.Count(), openings.Count()];

            var costs = newcostTrue = CostCalculation(resources, openings, true);
            var newcost = CostCalculation(resources, openings, false);

            List<Resource> originalResouces = new List<Resource>(resources);
            List<Openning> originalOpenings = new List<Openning>(openings);
            
            var rowSol = new int[resources.Count()];
            var output = LAPJV.FindAssignments(ref rowSol, costs);

            var resourceAssignments = rowSol;
            WriteCSV(resourceAssignments, originalResouces, originalOpenings, newcost, newcostTrue, "FinalOutput-Paas1.csv");
            //run PASS 2
            resourceAssignments = NextResourcePoolAssignment(rowSol, resources, openings, newcost);
            WriteCSV(resourceAssignments, resources, openings, newcost, newcostTrue, "FinalOutput-Pass2.csv");

            sw.Stop();
            Console.WriteLine("End Calculation :" + sw.Elapsed);
            Console.ReadKey();
        }

        /// <summary>
        /// Run second PASS to find another resouce for remaning openings
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        private static int[] NextResourcePoolAssignment(int[] resourceAssignments, List<Resource> resources, List<Openning> openings, int[,] costs)
        {

            var resourcesCopy = resources;
            var openingsCopy = openings;

            //Calculate remaining resource-opening mapping
            RemainingOpenings(resourceAssignments,ref resources, ref openings, costs); 
        
            var cost = CostCalculation(resources, openings, false);

            var rowSol = new int[resources.Count()];
            var output = LAPJV.FindAssignments(ref rowSol, cost);
       

            //for (int i = 0; i < rowSol.Length; i++)
            //{
            //    if(rowSol[i] > -1)
            //    {
            //        var empId = resources.ElementAt(i).EmployeeId;
            //        var opnId = openings.ElementAt(rowSol[i]).RequestId;
            //        var resourceIndex = resourcesCopy.FindIndex(r => r.EmployeeId == empId);
            //        var openingIndex = openingsCopy.FindIndex(o => o.RequestId == opnId);

            //        resourceAssignments[resourceIndex] = openingIndex;
            //    }
            //}
            

            return rowSol;
        }

        /// <summary>
        /// Remaining opening identification and assignments
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        private static void RemainingOpenings(int[] resourceAssignments, ref List<Resource> resources, ref List<Openning> openings, int[,] costs)
        {
           List<Resource> newResourceCollection = new List<Resource>();
            List<Openning> newOpenningCollection = new List<Openning>();

            int k = 0;
            int p = 0;
            int[] removeOpeningIndex = new int[openings.Count()];
            foreach (var resource in resources)
            {
                if (costs[k, resourceAssignments[k]] < 1000)
                {
                    var opn = openings.ElementAt(resourceAssignments[k]);

                    if (opn != null)
                    {
                        resource.AvlDate = opn.AllocationEndDate;
                        removeOpeningIndex[p++] = opn.RequestId;
                        //openings.Remove(opn);
                    }
                }
                k++;
            }

            for (int i = 0; i < p; i++)
            {
                openings.Remove(openings.Find(o => o.RequestId == removeOpeningIndex[i]));
            }

            var newCost = CostCalculation(resources, openings, false);


            //int[] temp = new int[resources.Count()];
            //int tempVal = 0, inxarr = 0;
            //for (int m = 0; m < resources.Count(); m++)
            //{
            //    for (int n = 0; n < openings.Count(); n++)
            //    {
            //        tempVal += newCost[m, n];
            //    }

            //    temp[inxarr] = tempVal;
            //    tempVal = 0;
            //    inxarr++;
            //}


            //for (int h = 0; h < temp.Length; h++)
            //{
            //    if (temp[h] < 1000 * openings.Count())
            //    {
            //        newResourceCollection.Add(resources.ElementAt(h));
            //    }
            //}

            //resources = newResourceCollection;

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
        private static void WriteCSV(int[] resourceAssignments,List<Resource> res, List<Openning> opn, int[,] costs, int[,] costTrue, string fileName)
        {
            string filePath = @"xml\"+ fileName;
            string delimiter = ",";
            int indx = 0;
            StringBuilder sb = new StringBuilder();

                foreach (var assignmentID in resourceAssignments)
                {
                    if (assignmentID > -1)
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
                                                             costs[indx, assignmentID],
                                                             costTrue[indx, assignmentID]));
                    }
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

        /// <summary>
        /// Claculate cost
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="IsPenaltyRequired"></param>
        /// <returns></returns>
        private static int[,] CostCalculation(List<Resource> resources, List<Openning> openings,bool IsPenaltyRequired)
        {
            int[,] cost = new int[resources.Count(), openings.Count()];
            int j,i = j = 0;
            Scoring score = new Scoring();

            foreach (var res in resources)
            {

                foreach (var opnn in openings)
                {
                    cost[i, j] = score.CalculatedIndivisualScore(res, opnn, IsPenaltyRequired); 
                    j++;
                }

                i++;
                j = 0;
            }

            return cost;
        }

        /// <summary>
        /// Load and Map resource from xml
        /// </summary>
        /// <returns></returns>
        private static List<Resource> ResourceMapping()
        {
            List<Resource> resources = new List<Resource>();
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

            return resources;
        }

        /// <summary>
        /// Load and Map opening from xml
        /// </summary>
        /// <returns></returns>
        private static List<Openning> OpeningMapping()
        {
            List<Openning> openings = new List<Openning>();
            IFormatProvider culture = new System.Globalization.CultureInfo("hi-IN", true);

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

            return openings;
        }
    }

    
}
