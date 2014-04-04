using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Controls;
namespace STE
{
    public class STEController
    {
        public int currentPage = 0;
        public STEStorage storage;   
        public STEController()
        {
            storage = new STEStorage();
        }      
    }

    public class STEStorage
    {
        public List<StackPanel> wpfPage;
        public List<XmlNode> xmlTaskResults;
    }
}
