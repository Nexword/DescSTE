using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
namespace STE
{
    public class STEController
    {
        STEStorage storage;
        public int currentPage = 0;
       
        public STEController(STEWindow myWindow, STEStorage myStorage, STEWpfProcessor myWpfProcessor)
        {
            myWindow.controller = this;
            myWpfProcessor.steController = this;
            storage = myStorage;
        }

        public int PageCount()
        {
            return storage.GetPageCount();
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


      /*  StackPanel AddTask(XmlNode xml)
        { }

        StackPanel GetWpf(int index)
        { }*/
    }


}
