using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Controls;
namespace STE
{
     public class STEStorage
     {
        private STEWpfProcessor wpfProcessor;
        private STEXmlProcessor xmlProcessor;
        private List<XmlNode> taskResultsPages = new List<XmlNode>();
        private List<StackPanel> wpfPages = new List<StackPanel>();

        private void SetWpfProcessor(STEWpfProcessor proc)
        {
            wpfProcessor = proc;
        }

        private void SetXmlProcessor(STEXmlProcessor proc)
        {
            xmlProcessor = proc;
        }

        internal StackPanel GetWpf(int index)
        {
            return wpfPages[index];
        }

        internal XmlNode GetTaskResult(int index)
        {
            return taskResultsPages[index];
        }


        public void AddTask(string task)
        {
            wpfPages.Add(wpfProcessor.CreatePages(task));
            taskResultsPages.Add(xmlProcessor.CreateTestResult(task));
        }

        internal void Clear()
        {
            wpfPages.Clear();
            taskResultsPages.Clear();
        }
            
        

        public int GetPageCount()
        {
            return wpfPages.Count;
        }

        public STEStorage(STEWpfProcessor wpfProc, STEXmlProcessor xmlProc)
        {
            SetWpfProcessor(wpfProc);
            SetXmlProcessor(xmlProc);
        }

      
    }
}
