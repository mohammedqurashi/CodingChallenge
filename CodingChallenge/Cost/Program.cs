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
        private static int passCounter;
        static List<Combination> blockCombination = new List<Combination>();

        static void Main(string[] args)
        {
           
            Console.Title = "Coding Challenge";
            Stopwatch sw = new Stopwatch();
            sw.Start(); Console.WriteLine("Start:" + sw.Elapsed);


            FileWriter fileWriter = new FileWriter();
            XmlLoader xmlLoader = new XmlLoader();
            var resources = xmlLoader.ResourceMapping(); //Load and map resouces from resource input xml
            var openings = xmlLoader.OpeningMapping();   //Load amd map opening from opening input xml
            List<Resource> originalResouces =  new List<Resource>(resources);
            List<Openning> originalOpenings =  new List<Openning>(openings);

            var result = new List<Combination>();
            var costs = CostCalculation(resources, openings, blockCombination, true);
            bool isRevaluationRequired = false;


            var resourceAssignments = ResetResourceAssignment(resources);

            passCounter = 1;

            do
            {
                result = SolveItertiveLAP(ref resourceAssignments, resources, openings, costs);
                isRevaluationRequired = ProjectAverageScore(result); //Calculate Average Project Score
                resourceAssignments = ResetResourceAssignment(resources);
                resources = xmlLoader.ResourceMapping();
                openings  = xmlLoader.OpeningMapping();
            } while (isRevaluationRequired);


            
            fileWriter.WriteCSV(result, resources, openings, "2390_Mowgli_Output.csv");
            sw.Stop(); Console.WriteLine("End Calculation :" + sw.Elapsed);Console.WriteLine("Outfile: 2390_Mowgli_Output.csv is generated");Console.ReadKey();
        }

        /// <summary>
        /// reset resource assignment for multiple PASS evaluation
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        private static int[] ResetResourceAssignment(List<Resource> resources) {

            var resourceAssignments = new int[resources.Count()];
            for (int h = 0; h < resourceAssignments.Length; h++)
                resourceAssignments[h] = -1;

            return resourceAssignments;
        }

        /// <summary>
        /// Solve iterative LAP problem
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns></returns>
        private static List<Combination> SolveItertiveLAP(ref int[] resourceAssignments, List<Resource> resources, List<Openning> openings, int[,] costs) {
            int count = 0;
            var result = new List<Combination>();
            do
            {
                var passResultCombination = PassIteration(ref resourceAssignments, resources, openings, costs);
                count = passResultCombination.Count();
                costs = CostCalculation(resources, openings, blockCombination,true);
                result = result.Concat(passResultCombination).ToList();
            } while (count > 0);

            return result;
        }

        /// <summary>
        /// dynamics iteration behalf of found combination count
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns></returns>
        private static List<Combination> PassIteration(ref int[] resourceAssignments, List<Resource> resources, List<Openning> openings, int[,] costs)
        {
            Console.WriteLine("PASS " + passCounter.ToString() + " started.....");
            resourceAssignments = NextResourcePoolAssignment(ref resourceAssignments, resources, openings, costs);
            var newcosts = CostCalculation(resources, openings, blockCombination, true);
            var passResultCombination = PopulateResultCombinations(resourceAssignments, resources, openings, newcosts);
            Console.WriteLine("Pass "  + passCounter.ToString() + " completed");
            passCounter++;
            return passResultCombination;
        }

        /// <summary>
        /// Run second PASS to find another resouce for remaning openings
        /// </summary>
        /// <param name="resourceAssignments"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns>assignment list</returns>
        private static int[] NextResourcePoolAssignment(ref int[] resourceAssignments, List<Resource> resources, List<Openning> openings, int[,] costs)
        {

            //Calculate remaining resource-opening mapping
            var unmatchOpenings = RemainingOpenings(ref resourceAssignments,ref resources, ref openings, costs);
            var cost = CostCalculation(resources, openings, blockCombination, true);

            var rowSol = new int[resources.Count()];
            var output = LAPJV.FindAssignments(ref rowSol, cost);
            resourceAssignments = rowSol;
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
        private static int RemainingOpenings(ref int[] resourceAssignments, ref List<Resource> resources, ref List<Openning> openings, int[,] costs)
        {
            int k = 0,p = 0;
            int[] removeOpeningIndex = new int[openings.Count()];
            foreach (var resource in resources)
            {
                if (resourceAssignments[k] > -1 && costs[k, resourceAssignments[k]] < 1000)
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
        private static int[,] CostCalculation(List<Resource> resources, List<Openning> openings,List<Combination> blockCombination, bool IsPenaltyRequired)
        {
            int[,] cost = new int[resources.Count(), openings.Count()];
            int j, i = j = 0;
            Scoring score = new Scoring();

            foreach (var res in resources)
            {

                foreach (var opnn in openings)
                {
                    var m = from bc in blockCombination
                            where bc.EmpolyeeId == res.EmployeeId && bc.RequestId == opnn.RequestId
                            select bc;
                   
                    if (m.Any())
                    {
                       // Console.WriteLine(passCounter.ToString());
                            cost[i, j] = 1000;
                    }
                    else
                    {
                        cost[i, j] = score.CalculatedIndivisualScore(res, opnn, IsPenaltyRequired);
                    }

                    j++;
                }

                i++;
                j = 0;
            }

            return cost;
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
            Scoring score = new Scoring();
            for (int i = 0; i < rowSol.Length; i++)
            {
                if (rowSol[i] > -1 && costs[i, rowSol[i]]< 1000)
                {
                    combinations.Add(new Combination
                    {
                        EmpolyeeId = resources.ElementAt(i).EmployeeId,
                        RequestId = openings.ElementAt(rowSol[i]).RequestId,
                        ProjectKey = openings.ElementAt(rowSol[i]).ProjectKey,
                        Cost = score.CalculatedIndivisualScore(resources.ElementAt(i), openings.ElementAt(rowSol[i]), false) 
                    });

                }
            }

            return combinations;

        }

        /// <summary>
        /// Calculate project wise average cost
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static bool ProjectAverageScore(List<Combination> results)
        {
            bool isRevaluationRequired = false;
            var projectAverageCost = from combination in results
                                     group combination by new
                                         {
                                             combination.ProjectKey
                                         } into result
                                     select new
                                         {
                                            Avg = result.Average(pc => (1000 - pc.Cost) * 0.01),
                                            ProjectKey = result.Key.ProjectKey
                                         };

            var projectWiseMinCost = from combination in results
                                     group combination by new
                                     {
                                         combination.ProjectKey
                                     } into result
                                     select new
                                     {
                                         MinCost = result.Min(pc => pc.Cost),
                                         ProjectKey = result.Key.ProjectKey,
                                         ResourceId = result.OrderBy(a=> a.Cost).First().EmpolyeeId,
                                         OpeningId = result.OrderBy(a => a.Cost).First().RequestId
                                     };


            foreach (var item in projectAverageCost)
            {
                if (item.Avg > 1.5)
                {

                    foreach (var item2 in projectWiseMinCost.Where(pwc=> pwc.ProjectKey == item.ProjectKey))
                    {
                        blockCombination.Add(new Combination
                        {
                            ProjectKey = item2.ProjectKey,
                            EmpolyeeId = item2.ResourceId,
                            RequestId = item2.OpeningId,
                            Cost = item2.MinCost
                        });

                       
                    }
                    isRevaluationRequired = true;
                }

            }

            return isRevaluationRequired;

        }

    }

}
