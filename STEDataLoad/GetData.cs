using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

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
}
