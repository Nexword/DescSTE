using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.IO;
namespace STE
{
    public class STEController
    {
        STEWindow window;
        STEStorage storage;
        public int currentPage = 0;
       
        public STEController(STEWindow window, STEStorage storage)
        {
            window.Controller = this;

            this.window = window;
            this.storage = storage;
        }

        public void StartLinearTestLoading()
        {
            storage.Clear();
        }

        public void UploadTask(string xmlTask)
        {
            storage.AddTask(xmlTask);

        }

        public void EndLinearTestLoading()
        {
            window.ButtonsCount = storage.GetPageCount();
        }


        public void StartLinearTestExecuting()
        {
            window.UploadPage(storage.GetWpf(0));
        }

        public StackPanel GetWpf(int index)
        {
            currentPage = index;
            return storage.GetWpf(index);
        }

        public XmlNode GetTaskResult(int index)
        {
            return storage.GetTaskResult(index);
        }


        private void UpdateCurrentPage()
        {
            StackPanel wpf = storage.GetWpf(currentPage);
            window.UploadPage(wpf);
        }

        public void SwitchToNextPage()
        {
            if (currentPage + 1 < storage.GetPageCount())
            {
                currentPage++;
                UpdateCurrentPage();
            }
        }

        public void SwitchToPreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdateCurrentPage();
            }
        }

        public void SwitchPage(int pageIndex)
        {
            if (0 <= pageIndex && pageIndex < storage.GetPageCount())
            {
                currentPage = pageIndex;
                UpdateCurrentPage();
            }
        }

        public void SaveToFile()
        {
            var xdoc = new XmlDocument();
            for (int i = 0; i < storage.GetPageCount(); i++)
            {
                StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + "\\Results\\" + i + ".txt");
                XmlTextWriter writer = new XmlTextWriter(sw1);
                storage.GetTaskResult(i).WriteTo(writer);
                writer.Close();
            }
        }

        
        
      /*  StackPanel AddTask(XmlNode xml)
        { }

        StackPanel GetWpf(int index)
        { }*/
    }


}
