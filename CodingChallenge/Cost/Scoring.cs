﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Calculation
{
    public class Scoring
    {
        public Scoring()
        {
            //TODO: Something
        }

        public int CalculatedIndivisualScore(Resource res, Openning opn,bool IsWithPanelty = true) 
        {
            double score = 0;

           
                if (opn.RequestStartDate >= res.AvlDate && !opn.MandotaroySkilss.Except(res.Skills).Any())  
                {

                    score = score + 1;
                    score = res.NAGP ? score + 0.3 : score;
                    score = res.Rating == "a+" ? score + 0.2 : score;
                    score = res.Rating == "a" ? score + 0.1 : score;
                    score = opn.ProjectDomain != null ? (!res.DomainExperiance.Except(opn.ProjectDomain).Any() ? score + 0.2 : score) : score;
                    score = opn.CustomerName  != null ? (res.PreviousCustomerExperiance.Contains(opn.CustomerName) ? score + 0.3 : score) : score;
                    score = res.YearsOfExperiance - opn.YearsOfExperiance > 0 ? Math.Max(0, score - (0.05 * (res.YearsOfExperiance - opn.YearsOfExperiance))) : score;
                    score = opn.IsKeyProject && opn.IsKeyPosition ? Math.Min(2, score) : score;
                    score = opn.IsKeyProject && !opn.IsKeyPosition ? Math.Min(1.3, score) : score;
                    score = !opn.IsKeyProject && opn.IsKeyPosition ? Math.Min(1.5, score) : score;
                    score = !opn.IsKeyProject && !opn.IsKeyPosition ? Math.Min(1, score) : score;
                    

                    if(IsWithPanelty)
                    {
                        TimeSpan ts1 = opn.RequestStartDate - res.AvlDate;
                        TimeSpan ts2 = opn.AllocationEndDate - res.AvlDate;
                    
                        score = Math.Max(0.1, score - ((ts1.Add(ts2).Days) /36) * 0.02); // panelty factor assume 0.2
                    }

                }
          

            return 1000 - Convert.ToInt32(score * 100); 
        }


        public int PartialMatchCalculatedIndivisualScore(Resource res, Openning opn, bool IsWithPanelty = true)
        {
            double score = 0;

            if (opn.RequestStartDate > res.AvlDate && opn.MandotaroySkilss.Intersect(res.Skills).Any())
            {

                score = score + 1;
                score = res.NAGP ? score + 0.3 : score;
                score = res.Rating == "A+" ? score + 0.2 : score;
                score = res.Rating == "A" ? score + 0.1 : score;
                score = opn.ProjectDomain != null ? (!res.DomainExperiance.Except(opn.ProjectDomain).Any() ? score + 0.2 : score) : score;
                score = res.YearsOfExperiance - opn.YearsOfExperiance > 0 ? Math.Max(0, score - (0.05 * (res.YearsOfExperiance - opn.YearsOfExperiance))) : score;
                score = opn.IsKeyProject && opn.IsKeyPosition ? Math.Max(2, score) : score;
                score = opn.IsKeyProject && !opn.IsKeyPosition ? Math.Max(1.3, score) : score;
                score = !opn.IsKeyProject && opn.IsKeyPosition ? Math.Max(1.5, score) : score;
                score = !opn.IsKeyProject && !opn.IsKeyPosition ? Math.Max(1, score) : score;

                if (IsWithPanelty)
                {
                    score = Math.Max(0, score - (score) * 0.2); // panelty factor assume 0.2
                }

            }


            return 1000 - Convert.ToInt32(score * 100); ;
        }


    }
}
