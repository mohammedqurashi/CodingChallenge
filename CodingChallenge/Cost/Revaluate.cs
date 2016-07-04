using System;
using System.Collections.Generic;
using System.Linq;

namespace Calculation
{
    public class Revaluate
    {

        List<Combination> blockCombination = new List<Combination>();

        /// <summary>
        /// populate employee, opening and cost collection
        /// </summary>
        /// <param name="rowSol"></param>
        /// <param name="resources"></param>
        /// <param name="openings"></param>
        /// <param name="costs"></param>
        /// <returns>list of combination</returns>
        public List<Combination> PopulateResultCombinations(int[] rowSol, List<Resource> resources, List<Openning> openings, int[,] costs)
        {

            List<Combination> combinations = new List<Combination>();

            for (int i = 0; i < rowSol.Length; i++)
            {
                if (rowSol[i] > -1 && costs[i, rowSol[i]] < 1000)
                {
                    combinations.Add(new Combination
                    {
                        EmpolyeeId = resources.ElementAt(i).EmployeeId,
                        RequestId = openings.ElementAt(rowSol[i]).RequestId,
                        ProjectKey = openings.ElementAt(rowSol[i]).ProjectKey,
                        Cost = costs[i, rowSol[i]]
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
        public bool ProjectAverageScore(List<Combination> results)
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
                                         ResourceId = result.OrderBy(a => a.Cost).First().EmpolyeeId,
                                         OpeningId = result.OrderBy(a => a.Cost).First().RequestId
                                     };


            foreach (var item in projectAverageCost)
            {
                if (item.Avg > 1.5)
                {

                    foreach (var item2 in projectWiseMinCost.Where(pwc => pwc.ProjectKey == item.ProjectKey))
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
