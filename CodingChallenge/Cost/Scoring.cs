using System;
using System.Linq;

namespace Calculation
{
    public class Scoring
    {
        public Scoring()
        {
            //TODO: Something
        }

        public int CalculatedIndivisualScore(Resource res, Openning opn)
        {
            double score = 0;

           
                if (opn.RequestStartDate > res.AvlDate && !opn.MandotaroySkilss.Except(res.Skills).Any())  //&& res.Skills.Intersect(opn.MandotaroySkilss).Any())   //
                {

                    score = score + 1;
                    score = res.NAGP ? score + 0.3 : score;
                    score = res.Rating == "A+" ? score + 0.2 : score;
                    score = res.Rating == "A" ? score + 0.1 : score;
                    score = opn.ProjectDomain!=null ? ( !res.DomainExperiance.Except(opn.ProjectDomain).Any() ? score + 0.2 : score) : score;
                    //score = opn.ProjectDomain!=null ? (opn.ProjectDomain.Intersect(res.DomainExperiance).Any() ? score + 0.2 : score) : score;
                    score = res.YearsOfExperiance - opn.YearsOfExperiance > 0 ? Math.Max(0, score - (0.05 * (res.YearsOfExperiance - opn.YearsOfExperiance))) : score;
                    score = opn.IsKeyProject && opn.IsKeyPosition ? Math.Max(2, score) : score;
                    score = opn.IsKeyProject && !opn.IsKeyPosition ? Math.Max(1.3, score) : score;
                    score = !opn.IsKeyProject && opn.IsKeyPosition ? Math.Max(1.5, score) : score;
                    score = !opn.IsKeyProject && !opn.IsKeyPosition ? Math.Max(1, score) : score;
                }
          

            return 1000 - Convert.ToInt32(score * 100); ;
        }
    }
}
