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
        //global variables
        private static char[] charSeparators = new char[] { ',' };
        private static IFormatProvider culture = new System.Globalization.CultureInfo("hi-IN", true);

        static void Main(string[] args)
        {
           
            Console.Title = "Coding Challenge";
            Stopwatch sw = new Stopwatch();
            sw.Start(); Console.WriteLine("Start:" + sw.Elapsed);

            var resources = ResourceMapping(); //Load and map resouces from resource input xml
            var openings = OpeningMapping();   //Load amd map opening from opening input xml

            
            var costs = CostCalculation(resources, openings, true);
            var newcost = CostCalculation(resources, openings, false);

            List<Resource> originalResouces = new List<Resource>(resources);
            List<Openning> originalOpenings = new List<Openning>(openings);
            
            //PASS 1 Run
            var rowSol = new int[resources.Count()];
            var output = LAPJV.FindAssignments(ref rowSol, costs);
            var resourceAssignments = rowSol;
            var pass1Combi = PopulateResultCombinations(rowSol, resources, openings, costs);
            
            //PASS 2 Run
            resourceAssignments = NextResourcePoolAssignment(rowSol, resources, openings, newcost);
            var costPass2 = CostCalculation(resources, openings, true); 
            var pass2Combi = PopulateResultCombinations(resourceAssignments, resources, openings, costPass2);


            var result = pass1Combi.Union(pass2Combi);
            WriteCSV(result.ToList(), originalResouces, originalOpenings, "FinalOutput-" + DateTime.Now.ToString("ddMMyyyhhmmss") + ".csv");
            sw.Stop(); Console.WriteLine("End Calculation :" + sw.Elapsed); Console.ReadKey();
        }

        /// <summary>
        /// Run second PASS to find another resouce for remaning openings
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns>assignment list</returns>
        private static int[] NextResourcePoolAssignment(int[] resourceAssignments, List<Resource> resources, List<Openning> openings, int[,] costs)
        {

            //Calculate remaining resource-opening mapping
            var unmatchOpenings = RemainingOpenings(resourceAssignments,ref resources, ref openings, costs);

            var cost = CostCalculation(resources, openings, false);

            var rowSol = new int[resources.Count()];
            var output = LAPJV.FindAssignments(ref rowSol, cost);

            return rowSol;
        }

        /// <summary>
        /// Remaining opening identification and assignments
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns>unmatch opening count</returns>
        private static int RemainingOpenings(int[] resourceAssignments, ref List<Resource> resources, ref List<Openning> openings, int[,] costs)
        {
            int k = 0,p = 0;
            int[] removeOpeningIndex = new int[openings.Count()];
            foreach (var resource in resources)
            {
                if (costs[k, resourceAssignments[k]] < 1000)
                {
                    var opn = openings.ElementAt(resourceAssignments[k]);

                    if (opn != null)
                    {
                        resource.AvlDate = opn.AllocationEndDate.AddDays(1);
                        removeOpeningIndex[p++] = opn.RequestId;
                    }
                }
                k++;
            }

            //remove matched opening for next PASS
            for (int i = 0; i < p; i++)
            {
                openings.Remove(openings.Find(o => o.RequestId == removeOpeningIndex[i]));
            }

            return openings.Count();
        }

        /// <summary>
        /// Claculate cost
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="IsPenaltyRequired"></param>
        /// <returns>cost materics</returns>
        private static int[,] CostCalculation(List<Resource> resources, List<Openning> openings, bool IsPenaltyRequired)
        {
            int[,] cost = new int[resources.Count(), openings.Count()];
            int j, i = j = 0;
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
        /// Write CSV file
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="res"></param>
        /// <param name="opn"></param>
        /// <param name="costs"></param>
        /// <param name="costTrue"></param>
        /// <param name="fileName"></param>
        private static void WriteCSV(List<Combination> combi, List<Resource> res, List<Openning> opn, string fileName) 
        {
            string filePath = @"xml\"+ fileName;
            string delimiter = ",";
            StringBuilder sb = new StringBuilder();

            //header to csv
            sb.AppendLine("EmployeeId,RequestId,Skills,MandotaroySkilss,AvlDate,RequestStartDate,AllocationEndDate,PreviousCustomerExperiance,CustomerName,DomainExperiance,ProjectDomain,ProjectKey,NAGP,Rating,IsKeyProject,IsKeyPosition,cost1,cost2,score");

                    foreach (var item in combi)
                    {
    
                        var rs = res.Find(r=>r.EmployeeId == item.EmpolyeeId);
                        var op = opn.Find(o=> o.RequestId == item.RequestId);
                        var cst = item.Cost;
                        double score = (1000 - cst) *0.01;
                        sb.AppendLine(string.Join(delimiter, rs.EmployeeId,
                                                             op.RequestId,
                                                             String.Join("+", rs.Skills.ToArray()),
                                                             op.MandotaroySkilss.Count() > 0 ? String.Join("+", op.MandotaroySkilss.ToArray()) : "",
                                                             rs.AvlDate,
                                                             op.RequestStartDate,
                                                             op.AllocationEndDate,
                                                             rs.PreviousCustomerExperiance.Count() > 0 ? String.Join("+", rs.PreviousCustomerExperiance.ToArray()) : "" ,
                                                             op.CustomerName,
                                                             rs.DomainExperiance.Count() > 0 ? String.Join("+", rs.DomainExperiance.ToArray()) : "",
                                                             op.ProjectDomain.Count() > 0 ? String.Join("+", op.ProjectDomain.ToArray()): "",
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

        /// <summary>
        /// Load data from xml to datatable
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>data table</returns>
        private static DataTable LoadXML(string filePath)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(filePath, XmlReadMode.Auto);
            return ds.Tables[0];
        }

        /// <summary>
        /// Load and Map resource from xml
        /// </summary>
        /// <returns>list of resources</returns>
        private static List<Resource> ResourceMapping()
        {
            List<Resource> resources = new List<Resource>();

            //  Loading resource
            foreach (var item in LoadXML("xml/resources.xml").AsEnumerable())
            {
                Resource res = new Resource();
                res.EmployeeId                 = Convert.ToInt32(item["EmployeeID"]);
                res.DOJ                        = Convert.ToDateTime(item["DOJ"], culture);
                res.Skills                     = PrepareSkillList(Convert.ToString(item["Skills"]).ToLower().Trim());
                res.DomainExperiance           = Convert.ToString(item["DomainExperience"]).ToLower().Split(charSeparators,StringSplitOptions.RemoveEmptyEntries).ToList<string>(); 
                res.Rating                     = Convert.ToString(item["Rating"]).ToLower();
                res.CommunicationRating        = Convert.ToString(item["CommunicationsRating"]).ToLower();
                res.NAGP                       = Convert.ToString(item["NAGP"]).ToLower() == "y" ? true : false;
                res.YearsOfExperiance          = Convert.ToDouble(item["YearsOfExperience"]);
                res.CurrentRole                = Convert.ToString(item["CurrentRole"]).ToLower();
                res.PreviousCustomerExperiance = Convert.ToString(item["PreviousCustomerExperience"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>(); 
                res.AvlDate                    = Convert.ToDateTime(item["AvailableFromDate"], culture);
                resources.Add(res);
            }

            return resources;
        }

        /// <summary>
        /// Load and Map opening from xml
        /// </summary>
        /// <returns>list of openings</returns>
        private static List<Openning> OpeningMapping()
        {
            List<Openning> openings = new List<Openning>();
            
            //Loading openings
            foreach (var item in LoadXML("xml/openings.xml").AsEnumerable())
            {
                Openning opnn = new Openning();
                opnn.RequestId           = Convert.ToInt32(item["RequestID"]);
                opnn.ClientKey           = Convert.ToString(item["ClientKey"]).ToLower();
                opnn.ProjectKey          = Convert.ToString(item["ProjectKey"]).ToLower();
                opnn.CustomerName        = Convert.ToString(item["CustomerName"]).ToLower();
                opnn.ProjectName         = Convert.ToString(item["ProjectName"]).ToLower();
                opnn.ProjectDomain       = Convert.ToString(item["ProjectDomain"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>(); 
                opnn.IsKeyProject        = Convert.ToString(item["IsKeyProject"]).ToLower() == "y" ? true : false;
                opnn.ProjectStartDate    = Convert.ToDateTime(item["ProjectStartDate"], culture);
                opnn.ProjectEndDate      = Convert.ToDateTime(item["ProjectEndDate"], culture);
                opnn.Role                = Convert.ToString(item["Role"]).ToLower();
                opnn.IsKeyPosition       = Convert.ToString(item["IsKeyPosition"]).ToLower() == "y" ? true : false;
                opnn.YearsOfExperiance   = Convert.ToDouble(item["YearsOfExperience"]);
                opnn.MandotaroySkilss    = Convert.ToString(item["MandatorySkills"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>(); 
                opnn.ClientCommunication = Convert.ToString(item["ClientCommunication"]).ToLower() == "y" ? true : false;
                opnn.RequestStartDate    = Convert.ToDateTime(item["RequestStartDate"], culture);
                opnn.AllocationEndDate   = Convert.ToDateTime(item["AllocationEndDate"], culture);
                openings.Add(opnn);
            }

            return openings;
        }

        /// <summary>
        /// Prepare skill list
        /// </summary>
        /// <param name="skill"></param>
        /// <returns>list of skills</returns>
        private static List<string> PrepareSkillList(string skill)
        {
            var skillList = skill.ToLower().Split(',');
            var mainTech = "";
            foreach (var item in skillList)
            {
                if (item.Contains("expert"))
                {
                    Array.Resize(ref skillList, skillList.Length + 2);
                    mainTech = item.Substring(0, item.IndexOf('-')).ToLower();
                    skillList[skillList.Length - 2] = mainTech + "-intermediate";
                    skillList[skillList.Length - 1] = mainTech + "-beginner";
                }
                else if (item.Contains("intermediate"))
                {
                    Array.Resize(ref skillList, skillList.Length + 1);
                    mainTech = item.Substring(0, item.IndexOf('-')).ToLower();
                    skillList[skillList.Length - 1] = mainTech + "-beginner";
                }
                mainTech = "";
            }

            return skillList.ToList<string>();
        }

        /// <summary>
        /// populate employee, opening and cost collection
        /// </summary>
        /// <param name="rowSol"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns>list of combination</returns>
        private static List<Combination> PopulateResultCombinations(int[] rowSol, List<Resource> resources, List<Openning> openings, int[,] costs) {

            List<Combination> combinations = new List<Combination>();

            //var combi = new int[resources.Count(), 3];

            for (int i = 0; i < rowSol.Length; i++)
            {
                if (rowSol[i] > -1 && costs[i, rowSol[i]]< 1000)
                {
                    combinations.Add(new Combination
                    {
                        EmpolyeeId = resources.ElementAt(i).EmployeeId,
                        RequestId = openings.ElementAt(rowSol[i]).RequestId,
                        Cost = costs[i, rowSol[i]]
                    });

                }
            }

            return combinations;

        }

    }

}
