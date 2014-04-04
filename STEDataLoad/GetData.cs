using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.IO;

namespace STE
{
    public  class GetData
    {

        public  XmlDocument GetXMLTest()
        {
            XmlDocument xmlTest = new XmlDocument();
            xmlTest.Load("TestSet.xml");
            return xmlTest;
        }

        public  XmlDocument GetXMLTask()
        {
            XmlDocument xmlTask = new XmlDocument();
            xmlTask.Load("TaskSet.xml");
            return xmlTask;

        }

        public  XmlDocument GetXMLContent()
        {
            XmlDocument xmlContent = new XmlDocument();
            xmlContent.Load("ContentSet.xml");
            return xmlContent;

        }

    }

    public class DataLoader
    {
        public List<string> GetTasks(string testSetName,string id)
        {
            XmlDocument testSet = new XmlDocument();
            testSet.Load(testSetName);

            List<string> xmlPages = new List<string>();
          
            XmlNode currentTest = testSet.SelectSingleNode(String.Format("//test-set//test[@id='{0}']", id));
            XmlNodeList childs = currentTest.ChildNodes;
           
            foreach (XmlNode node in childs)
            {

                if (node.Name == "note") xmlPages.Add(ReadFromFile(Environment.CurrentDirectory + "Tasks\\" + node.Attributes.GetNamedItem("id").Value + ".xml"));
                if (node.Name=="block")
                {
                    XmlNodeList tasks = node.ChildNodes;
                    foreach (XmlNode task in tasks)
                        xmlPages.Add(ReadFromFile(Environment.CurrentDirectory + "\\Tasks\\" + task.Attributes.GetNamedItem("key").Value + ".xml")); 
                }
            }

            return xmlPages;
        }

        public string ReadFromFile(string fileName)
        {
            string s = "";
            using (StreamReader sr = new StreamReader(fileName))
            {
                s = sr.ReadToEnd();
            }
            return s;
        }
    }
}
