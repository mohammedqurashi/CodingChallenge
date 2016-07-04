using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculation
{
    public class XmlLoader
    {
        //global variables
        private char[] charSeparators = new char[] { ',' };
        private IFormatProvider culture = new System.Globalization.CultureInfo("hi-IN", true);
       
        /// <summary>
        /// Load and Map resource from xml
        /// </summary>
        /// <returns>list of resources</returns>
        public List<Resource> ResourceMapping()
        {
                List<Resource> resources = new List<Resource>();
            
                //  Loading resource
                foreach (var item in LoadXML("Datasheet_Resources.xml").AsEnumerable())
                {
                    Resource res = new Resource();
                    res.EmployeeId = Convert.ToString(item["EmployeeID"]).ToLower().Trim();
                    res.DOJ = Convert.ToDateTime(item["DOJ"], culture);
                    res.Skills = PrepareSkillList(Convert.ToString(item["Skills"]).ToLower().Trim());
                    res.DomainExperiance = PrepareDomainExperianceList(Convert.ToString(item["DomainExperience"])); //Convert.ToString(item["DomainExperience"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    res.Rating = Convert.ToString(item["Rating"]).ToLower().Trim();
                    res.CommunicationRating = Convert.ToString(item["CommunicationsRating"]).ToLower().Trim();
                    res.NAGP = Convert.ToString(item["NAGP"]).ToLower().Trim() == "y" ? true : false;
                    res.YearsOfExperiance = Convert.ToDouble(item["YearsOfExperience"]);
                    res.CurrentRole = Convert.ToString(item["CurrentRole"]).ToLower().Trim();
                    res.PreviousCustomerExperiance = PrepareCustomerExperianceList(Convert.ToString(item["PreviousCustomerExperience"]));//   Convert.ToString(item["PreviousCustomerExperience"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    res.AvlDate = Convert.ToDateTime(item["AvailableFromDate"], culture);
                    resources.Add(res);
                }
           
           
            return resources;
        }

        /// <summary>
        /// Load and Map opening from xml
        /// </summary>
        /// <returns>list of openings</returns>
        public List<Openning> OpeningMapping()
        {
                List<Openning> openings = new List<Openning>();
           
                //Loading openings
                foreach (var item in LoadXML("Datasheet_Openings.xml").AsEnumerable())
                {
                    Openning opnn = new Openning();
                    opnn.RequestId = Convert.ToInt32(item["RequestID"]);
                    opnn.ClientKey = Convert.ToString(item["ClientKey"]).ToLower().Trim();
                    opnn.ProjectKey = Convert.ToString(item["ProjectKey"]).ToLower().Trim();
                    opnn.CustomerName = Convert.ToString(item["CustomerName"]).ToLower().Trim();
                    opnn.ProjectName = Convert.ToString(item["ProjectName"]).ToLower().Trim();
                    opnn.ProjectDomain = PrepareDomainExperianceList(Convert.ToString(item["ProjectDomain"])); // Convert.ToString(item["ProjectDomain"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    opnn.IsKeyProject = Convert.ToString(item["IsKeyProject"]).ToLower().Trim() == "y" ? true : false;
                    opnn.ProjectStartDate = Convert.ToDateTime(item["ProjectStartDate"], culture);
                    opnn.ProjectEndDate = Convert.ToDateTime(item["ProjectEndDate"], culture);
                    opnn.Role = Convert.ToString(item["Role"]).ToLower().Trim();
                    opnn.IsKeyPosition = Convert.ToString(item["IsKeyPosition"]).ToLower().Trim() == "y" ? true : false;
                    opnn.YearsOfExperiance = Convert.ToDouble(item["YearsOfExperience"]);
                    opnn.MandotaroySkilss = PrepareMandatorySkill(Convert.ToString(item["MandatorySkills"]));//Convert.ToString(item["MandatorySkills"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>(); //
                    opnn.ClientCommunication = Convert.ToString(item["ClientCommunication"]).ToLower().Trim() == "y" ? true : false;
                    opnn.RequestStartDate = Convert.ToDateTime(item["RequestStartDate"], culture);
                    opnn.AllocationEndDate = Convert.ToDateTime(item["AllocationEndDate"], culture);
                    openings.Add(opnn);
                }
            

            return openings;
        }

        /// <summary>
        /// Load data from xml to datatable
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>data table</returns>
        private DataTable LoadXML(string filePath)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(filePath, XmlReadMode.Auto);
            return ds.Tables[0];
        }

        /// <summary>
        /// Prepare skill list
        /// </summary>
        /// <param name="skill"></param>
        /// <returns>list of skills</returns>
        private List<string> PrepareSkillList(string skill)
        {
            var mainTech = "";
            var skillList = skill.ToLower().Split(',');
            List<string> finalSkillList = new List<string>();

            foreach (var item in skillList)
            {
                 mainTech = item.Substring(0, item.IndexOf('-')).ToLower();

                if (item.Contains("expert"))
                {
                    finalSkillList.Add(mainTech.Trim() + "-expert");
                    finalSkillList.Add(mainTech.Trim() + "-intermediate");
                    finalSkillList.Add(mainTech.Trim() + "-beginner");
                }
                else if (item.Contains("intermediate"))
                {
                    finalSkillList.Add(mainTech.Trim() + "-intermediate");
                    finalSkillList.Add(mainTech.Trim() + "-beginner");
                }
                else
                {
                    finalSkillList.Add(mainTech.Trim() + "-beginner");
                }

                mainTech = "";
            }

            return finalSkillList;
        }

        /// <summary>
        /// Prepare mandatory skill list
        /// </summary>
        /// <param name="mandatorySkill"></param>
        /// <returns></returns>
        private List<string> PrepareMandatorySkill(string mandatorySkill) {

            var mandatorySkillList = mandatorySkill.ToLower().Split(',');
            List<string> finalMandatorySkillList = new List<string>();
            foreach (var item in mandatorySkillList)
            {
                finalMandatorySkillList.Add(item.Trim().ToLower());
            }

            return finalMandatorySkillList;
        }

        /// <summary>
        /// Prepare mandatory skill list
        /// </summary>
        /// <param name="mandatorySkill"></param>
        /// <returns></returns>
        private List<string> PrepareDomainExperianceList(string domainExperiance)
        {

            var domainExperianceList = domainExperiance.ToLower().Split(',');
            List<string> finalDomainExperianceList = new List<string>();
            foreach (var item in domainExperianceList)
            {
                finalDomainExperianceList.Add(item.Trim().ToLower());
            }

            return finalDomainExperianceList;
        }

        /// <summary>
        /// Prepare customer experiance list
        /// </summary>
        /// <param name="customerExperiance"></param>
        /// <returns></returns>
        private List<string> PrepareCustomerExperianceList(string customerExperiance)
        {
            var customerExperianceList = customerExperiance.ToLower().Split(',');
            List<string> finalCustomerExperianceList = new List<string>();
            foreach (var item in customerExperianceList)
            {
                finalCustomerExperianceList.Add(item.Trim().ToLower());
            }

            return finalCustomerExperianceList;
        }
    }
}
