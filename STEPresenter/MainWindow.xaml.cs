using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;
using System.IO;
namespace STE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            GetData data = new GetData();
            XmlDocument xmlTestSet, xmlTaskSet, xmlContentSet;
            xmlTestSet = data.GetXMLTest();
            xmlTaskSet = data.GetXMLTask();
            xmlContentSet = data.GetXMLContent();
            ISTEProcessor myWPFProcessor = new STEProcessor();
            STEStackPanel.xmlTaskSet = xmlTaskSet; 
            List<List<string>> xamlPages = new List<List<string>>();
            XmlNodeList tests = xmlTestSet.GetElementsByTagName("test");
            foreach (XmlNode test in tests)
                xamlPages.Add(myWPFProcessor.CreatePages(test,xmlTaskSet,xmlContentSet));
            MakeAnswerSet(xmlTaskSet);









           List<STEStackPanel> wpfPages = new List<STEStackPanel>();
           STEWindow window = STEWindow.LoadWindowFromXaml();
           window.Show();
           foreach(List<string> xmlPage in xamlPages)
           {
               wpfPages = CreateWPFPages(xmlPage);
           }
           window.testPages = wpfPages;
          
           window.CreateMainElements();
           window.CurrentPage = 0;
           
        }

        public List<STEStackPanel> CreateWPFPages(List<string> xamlPages)
        {
            List<STEStackPanel> wpfElements = new List<STEStackPanel>();
            
            foreach (string xamlPage in xamlPages)
            {
                string xamlTestPage =
                     @"<local:STEStackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
            xmlns:local='clr-namespace:STE;assembly=STEPresenter' 
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
           >"

                    + xamlPage + "</local:STEStackPanel>";
                StringReader reader = new StringReader(xamlTestPage);
                XmlReader xamlStream=XmlReader.Create(reader);
                wpfElements.Add((STEStackPanel)XamlReader.Load(xamlStream));
            }

            return wpfElements;
        }

        public void MakeAnswerSet(XmlDocument xmlTaskSet)
        {
            XmlNodeList xMap = xmlTaskSet.SelectNodes("//task-set//task");
            foreach(XmlNode node in xMap)
            {
                XmlAttribute xKey = xmlTaskSet.CreateAttribute("id");
                
                node.Attributes.RemoveNamedItem("");
                node.Attributes.Append(xKey);
              
            }

        }

       



    }
}
