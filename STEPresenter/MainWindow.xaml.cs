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
            DataLoader getData = new DataLoader();
            STEWindow window = STEWindow.LoadWindowFromXaml();
            STEXmlProcessor myXmlProcessor = new STEXmlProcessor();
            STEWpfProcessor myWpfProcessor = new STEWpfProcessor();
            STEStorage storage = new STEStorage(myWpfProcessor, myXmlProcessor);
            STEController ste = new STEController(window,storage,myWpfProcessor);
          
            List<string> xamlPages = new List<string>();

            //Мы говорим, что давай загрузи нам линейный тест с id T_001_001 из файла TestSet.xml, который расположен по стандартному адресу
            List<string> xmlPages = getData.GetTasks(Environment.CurrentDirectory + "\\Tests\\TestSet.xml", "T_001_001");
            List<XmlNode> xmlResultPages = new  List<XmlNode>();      
            foreach (string xmlPage in xmlPages)
            {
                storage.AddTask(xmlPage);
                storage.AddTaskResult(xmlPage);
            }
       
            window.Show();
            window.CreateMainElements();
            window.CurrentPage = 0;
           
        }

       
    }
}
