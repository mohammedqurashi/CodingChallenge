using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillMatching
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<string> skill = new List<string>();

            skill.Add("salesforce-expert");
            skill.Add("salesforce-intermediate");
            skill.Add("salesforce-beginner");

            skill.Add("python-expert");
            skill.Add("python-intermediate");
            skill.Add("python-beginner");

            skill.Add("iot-expert");
            skill.Add("iot-intermediate");
            skill.Add("iot-beginner");

            skill.Add("linux-expert");
            skill.Add("linux-intermediate");
            skill.Add("linux-beginner");

            skill.Add("c-expert");
            skill.Add("c-intermediate");
            skill.Add("c-beginner");


            List<string> requiredskill = new List<string>();
            requiredskill.Add("iot-beginner");
            requiredskill.Add("c-expert");


            if(!requiredskill.Except(skill).Any())
                Console.WriteLine("PASS");
            else
                Console.WriteLine("FAIL");

            Console.ReadKey();

        }
    }
}
