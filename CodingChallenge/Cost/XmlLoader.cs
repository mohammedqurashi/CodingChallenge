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
                foreach (var item in LoadXML("xml/Datasheet_Resources.xml").AsEnumerable())
                {
                    Resource res = new Resource();
                    res.EmployeeId = Convert.ToInt32(item["EmployeeID"]);
                    res.DOJ = Convert.ToDateTime(item["DOJ"], culture);
                    res.Skills = PrepareSkillList(Convert.ToString(item["Skills"]).ToLower().Trim());
                    res.DomainExperiance = Convert.ToString(item["DomainExperience"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    res.Rating = Convert.ToString(item["Rating"]).ToLower();
                    res.CommunicationRating = Convert.ToString(item["CommunicationsRating"]).ToLower();
                    res.NAGP = Convert.ToString(item["NAGP"]).ToLower() == "y" ? true : false;
                    res.YearsOfExperiance = Convert.ToDouble(item["YearsOfExperience"]);
                    res.CurrentRole = Convert.ToString(item["CurrentRole"]).ToLower();
                    res.PreviousCustomerExperiance = Convert.ToString(item["PreviousCustomerExperience"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
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
                foreach (var item in LoadXML("xml/Datasheet_Openings.xml").AsEnumerable())
                {
                    Openning opnn = new Openning();
                    opnn.RequestId = Convert.ToInt32(item["RequestID"]);
                    opnn.ClientKey = Convert.ToString(item["ClientKey"]).ToLower();
                    opnn.ProjectKey = Convert.ToString(item["ProjectKey"]).ToLower();
                    opnn.CustomerName = Convert.ToString(item["CustomerName"]).ToLower();
                    opnn.ProjectName = Convert.ToString(item["ProjectName"]).ToLower();
                    opnn.ProjectDomain = Convert.ToString(item["ProjectDomain"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    opnn.IsKeyProject = Convert.ToString(item["IsKeyProject"]).ToLower() == "y" ? true : false;
                    opnn.ProjectStartDate = Convert.ToDateTime(item["ProjectStartDate"], culture);
                    opnn.ProjectEndDate = Convert.ToDateTime(item["ProjectEndDate"], culture);
                    opnn.Role = Convert.ToString(item["Role"]).ToLower();
                    opnn.IsKeyPosition = Convert.ToString(item["IsKeyPosition"]).ToLower() == "y" ? true : false;
                    opnn.YearsOfExperiance = Convert.ToDouble(item["YearsOfExperience"]);
                    opnn.MandotaroySkilss = Convert.ToString(item["MandatorySkills"]).ToLower().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    opnn.ClientCommunication = Convert.ToString(item["ClientCommunication"]).ToLower() == "y" ? true : false;
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
        private static DataTable LoadXML(string filePath)
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
    }
}
