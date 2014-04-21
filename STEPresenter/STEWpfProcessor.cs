using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xaml;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
namespace STE
{
    public interface ISTEProcessor
    {
        List<string> CreatePages(List<string> xmlPages);
    }


    public  class STEWpfProcessor
    {
        static List<string> contentString = new List<string>();
        
        public struct URLTypePair
        {
            public string url;
            public string type;
        }

        private delegate WrapPanel DrawContentBlockAction(List<URLTypePair> urlTypePairs);

  

#region Dictionary definition

        Dictionary<string, DrawContentBlockAction> Layouts = new Dictionary<string, DrawContentBlockAction>
            {
                { "typeA", 
                    delegate(List<URLTypePair> pairs)
                    { 
                        //string result = " <WrapPanel Orientation='Vertical'> ";
                        WrapPanel result = new WrapPanel();
                        result.Orientation=Orientation.Vertical;
                        //result.Background = System.Windows.Media.Brushes.Azure;
                        //result.HorizontalAlignment = HorizontalAlignment.Left;
                        for (int i = 0; i < pairs.Count;i++ )
                        {
                            
                            switch (pairs[i].type)
                            {
                                case "text":
                                    TextBlock textElement = new TextBlock();
                                    string sElement = pairs[i].url;
                                    sElement = @"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' FontSize='25' Margin='5' TextWrapping='Wrap'>" + sElement + "</TextBlock>";
                                    StringReader reader = new StringReader(sElement);
                                    XmlReader xamlStream = XmlReader.Create(reader);
                                    textElement = (TextBlock)System.Windows.Markup.XamlReader.Load(xamlStream);
                                    
                                    result.Children.Add(textElement);
                                    break;
                                case "image":
                                    Image imageElement = new Image();
                                    Uri uri = new Uri("pack://siteoforigin:,,,/" + pairs[i].url);
                                    BitmapImage bitmap = new BitmapImage(uri);
                                    imageElement.Source = bitmap;
                                    imageElement.Height = Double.NaN;
                                    imageElement.Width = Double.NaN;
                                    imageElement.MaxWidth = 400;
                                    imageElement.MaxHeight = 400;
                                    //imageElement.Source = "pack://application:,,,/" + pairs[i].url;
                                    //imageElement.Source = new BitmapImage(new Uri("pack://application:,,,/"+pairs[i].url));
                                    //imageElement = @"<Image Height='Auto' Width='Auto' MaxWidth='400' MaxHeight='400' Source='pack://siteoforigin:,,,/" + pairs[i].url + "' />";
                                    result.Children.Add(imageElement);
                                    break;
                                case "video":
                                    MediaElement mediaElement = new MediaElement();
                                    mediaElement.Height = Double.NaN;
                                    mediaElement.Width = Double.NaN;
                                    mediaElement.MaxWidth = 600;
                                    mediaElement.MaxHeight = 400;
                                    mediaElement.Margin = new Thickness(2);
                                    mediaElement.Source = new Uri("pack://siteoforigin:,,,/" + pairs[i].url);
                                    //sElement = @"<MediaElement Height='Auto' MaxWidth='600' MaxHeight='400'  Margin='2' Width='Auto' Source='pack://siteoforigin:,,,/" + pairs[i].url + "' />";
                                    result.Children.Add(mediaElement);
                                    break;
                                case "audio":
                                    MediaElement audioElement = new MediaElement();
                                    audioElement.Height=Double.NaN;
                                    audioElement.Width=double.NaN;
                                    audioElement.Source = new Uri("pack://siteoforigin:,,,/" + pairs[i].url);
                                    //sElement = @"<MediaElement Height='Auto' Width='Auto' Source='pack://siteoforigin:,,,/" + pairs[i].url + "' />";
                                    result.Children.Add(audioElement);
                                    break;
                            }
                        }            
                        return result; 
                    } 
                },
                  
            };

#endregion

#region STEPWpfProcessor Part 


        public  STEController steController;

       
        /// <summary>
        /// Создание страниц с разметкой XAML
        /// </summary>
        /// <param name="xmlPages">Таски приходят в формате XML</param>
        /// <returns>Возвращает набор страниц с разметкой XAML</returns>
        public StackPanel CreatePages(string xmlPage)
        {
            //List<StackPanel> xamlPages = new List<StackPanel>();
            //List<XmlNode> xmlTaskResult=new List<XmlNode>();
            XmlDocument doc = new XmlDocument();
               
                doc.LoadXml(xmlPage);
                //xmlTaskResult.Add(CreateTestResult(xmlPage));
                XmlNode currentPage = doc.DocumentElement;
                //if (currentPage.Name == "task") 
            return (CreateTask(currentPage));
            
            //steController.storage.xmlTaskResults = xmlTaskResult;
            //steController.storage.wpfPage = xamlPages;
        }
        /// <summary>
        /// Создает одно задание
        /// </summary>
        /// <param name="currentPage"> Передаем текущую страницу в XML</param>
        /// <returns>Возвращаем страницу в XAML разметке</returns>
        private StackPanel CreateTask(XmlNode currentPage)
        {
            StackPanel page = new StackPanel();
            foreach (XmlNode child in currentPage.ChildNodes)
            {
                switch (child.Name)
                {
                    case "question":
                        page.Children.Add(CreateQuestionElement(child));
                        break;
                    case "single-answer":
                        page.Children.Add(CreateSingleAnswerElement(child));
                        break;
                    case "multiple-answer":
                        page.Children.Add(CreateMultipleAnswerElement(child));
                        break;
                    case "open-answer":
                        page.Children.Add(CreateOpenAnswerElement(child));
                        break;
                    case "single-semiopen-answer":
                        page.Children.Add(CreateSingleSemiopenAnswerElement(child));
                        break;
                    case "multiple-semiopen-answer":
                        //page += CreateMultipleSemiopenAnswerElement(child);
                        break;
                    case "matching-answer":
                          page.Children.Add(CreateMatchingAnswerElement(child));
                        break;
                }
            }
            return page;
        }

        /// <summary>
        /// Создаем элемент вопрос в XAML разметке
        /// </summary>
        /// <param name="question">Узел в XML с соответствующим типом</param>
        /// <returns>Возвращает элемент вопрос в XAML разметке</returns>
        public StackPanel CreateQuestionElement(XmlNode question)
        {
            StackPanel result = new StackPanel();
            WrapPanel res = CreateContentBlock(question);
            result.Children.Add(res);
            return result;

        }

        /// <summary>
        /// Создаем вопрос с единичным выбором в XAML разметке
        /// </summary>
        /// <param name="node">Узел в XML с соответствующим типом</param>
        /// <returns></returns>
        public StackPanel CreateSingleAnswerElement(XmlNode node)
        {
           // List<Radiob> sOptionElements = new List<string>();
            GroupBox group = new GroupBox();
            StackPanel myPanel = new StackPanel();
            //string sSingleAnswerElement = "";
            string keyOption = "";
            foreach (XmlNode option in node.ChildNodes)
            {
                keyOption = option.Attributes.GetNamedItem("id").Value;
                WrapPanel buttonContent=CreateContentBlock(option);
                RadioButton radioButton = new RadioButton();
                radioButton.Name = keyOption;
                radioButton.Content = buttonContent;
                radioButton.Checked += Checked_Button;
                radioButton.Unchecked += Unchecked_Button;
                myPanel.Children.Add(radioButton);
                //sSingleAnswerElement += "<RadioButton Click ='Checked_Button' Unchecked='Unchecked_Button'  Name='" + keyOption + "'> " + sOptionElements.Last() + " </RadioButton>";
            }

            group.Content = myPanel;
            StackPanel result = new StackPanel();
            result.Children.Add(group);
            return result;
        }
        /// <summary>
        /// Создаем из XML вопрос типа множетсвенный выбор
        /// </summary>
        /// <param name="node">Узел в XML с соответствующим типом</param>
        /// <returns>Возвращаем вопрос типа множетсвенный выбор в XAML разметке</returns>
        public StackPanel CreateMultipleAnswerElement(XmlNode node)
        {

            //List<string> sOptionElements = new List<string>();

            GroupBox groupBox = new GroupBox();
            StackPanel myPanel = new StackPanel();
            string keyOption = "";
            foreach (XmlNode option in node.ChildNodes)
            {
                keyOption = option.Attributes.GetNamedItem("key").Value;
                //sOptionElements.Add(CreateContentBlock(option));
                WrapPanel buttonContent = CreateContentBlock(option);
                CheckBox checkBox = new CheckBox();
                checkBox.Name = keyOption;
                checkBox.Content = buttonContent;
                checkBox.Checked += Checked_Button;
                checkBox.Unchecked += Unchecked_Button;
                myPanel.Children.Add(checkBox);
                //sMultipleAnswerElement += "<CheckBox Click ='Checked_Button' Unchecked='Unchecked_Button' Name='" + keyOption + "'>" + sOptionElements.Last() + " </CheckBox>";
            }
            groupBox.Content = myPanel;
            StackPanel result = new StackPanel();
            result.Children.Add(groupBox);
            return result;
        }

        /// <summary>
        /// Создаем из XML вопрос с открытым типом ответа
        /// </summary>
        /// <param name="node">Узел в XML с соответствующим типом</param>
        /// <returns>Возвращаем вопрос с открытым типом ответа в разметке XAML</returns>
        public StackPanel CreateOpenAnswerElement(XmlNode node)
        {
            string sOpenAnswerElement = "Напишите ответ"; //заглушка
            string keyOption = node.ChildNodes[0].Attributes.GetNamedItem("id").Value;
            TextBox textBox = new TextBox();
            textBox.Text = sOpenAnswerElement;
            textBox.Name = keyOption;
            textBox.LostFocus += Text_Changed;
            StackPanel myPanel=new StackPanel();
            myPanel.Children.Add(textBox);
            return myPanel;
        }

        /// <summary>
        /// Создаем вопрос типа единичный выбор с возможонстью открытого ответа
        /// </summary>
        /// <param name="node">Узел в XML с соответствующим типом</param>
        /// <returns>Возвращаем вопрос типа единичный выбор с возможонстью открытого ответа в XAML разметке</returns>
        public StackPanel CreateSingleSemiopenAnswerElement(XmlNode node)
        {
            //string SingleSemiopenAnswerElement = "";
            string optionId = "";
            //string sOptionElements = "";
            StackPanel myPanel = new StackPanel();
            foreach (XmlNode optionNode in node.ChildNodes)
            {
                optionId = optionNode.Attributes.GetNamedItem("id").Value;
                if (optionNode.Name == "option")
                {
                    WrapPanel buttonContent = CreateContentBlock(optionNode);
                    RadioButton radioButton = new RadioButton();
                    radioButton.Name = optionId;
                    radioButton.Content = buttonContent;
                    myPanel.Children.Add(radioButton);
                    radioButton.Checked += Checked_Button;
                    radioButton.Unchecked += Unchecked_Button;
                    //SingleSemiopenAnswerElement += "<RadioButton Click ='Checked_Button' Unchecked='Unchecked_Button'  Margin='5' Name='" + keyOption + "'>" + sOptionElements + " </RadioButton>";
                }
                else
                {
                    TextBox textBox = new TextBox();
                    textBox.Name = optionId;
                    myPanel.Children.Add(textBox);
                    //SingleSemiopenAnswerElement += " <TextBox LostFocus='Unchecked_Button' Name='" + keyOption + "'>" + sOptionElements + "</TextBox>";
                }
            }

            return myPanel;
        }

        /// <summary>
        /// Создаем вопрос типа множественный выбор с возможонстью открытого ответа
        /// </summary>
        /// <param name="node">Узел в XML с соответствующим типом</param>
        /// <returns>Возвращаем вопрос типа множественный выбор с возможонстью открытого ответа в XAML разметке</returns>
        public string CreateMultipleSemiopenAnswerElement(XmlNode node)
        {
            string MultipleSemiopenAnswerElement = "";
            return MultipleSemiopenAnswerElement;
        }

        /// <summary>
        /// Создаем вопрос на совпадение из XML
        /// </summary>
        /// <param name="node">Узел в XML с соответствующим типом</param>
        /// <returns>Возвращаем описание вопроса в XAML</returns>
        public StackPanel CreateMatchingAnswerElement(XmlNode node)
        {
            StackPanel MatchingAnswerElement=new StackPanel();
            Grid gridPanel = new Grid();
            Panel currentPanel;
            SteDragDrop dragDrop=new SteDragDrop();
            dragDrop.steController = steController;
            double d = 0;
            double w = SystemParameters.PrimaryScreenWidth;
            Viewbox box = new Viewbox();
            box.Child = gridPanel;
            string slotName="";
            string matchName = "";
            foreach (XmlNode slot in node.LastChild.ChildNodes)
            {
                slotName = slot.Attributes.GetNamedItem("id").Value;
                currentPanel=CreateSlotElement(slot);
                currentPanel.Name = slotName;
                double top = currentPanel.Margin.Top;
                currentPanel.Margin = new Thickness(w-400, top + d, 0, 0);
                dragDrop.slots.Add(currentPanel);
                gridPanel.Children.Add(currentPanel);
                d = d+105;
            }

            d = 0;
            foreach (XmlNode match in node.FirstChild.ChildNodes)
            {
                matchName = match.Attributes.GetNamedItem("id").Value;
                currentPanel = CreateMatchElement(match, dragDrop);
                currentPanel.Name = matchName;
                double top = currentPanel.Margin.Top;
                currentPanel.Margin = new Thickness(0, top + d, 0, 0);
                gridPanel.Children.Add(currentPanel);
                d += 105;
               
            }
            gridPanel.MouseMove += dragDrop.MatchElementdMouseMove;
           
            MatchingAnswerElement.Children.Add(box);
            
            return MatchingAnswerElement;
        }

        private Panel CreateMatchElement(XmlNode match,SteDragDrop dragDrop)
        {
            Grid matchGrid = new Grid();
            matchGrid.Height = 100;
            matchGrid.Width = 200;
            matchGrid.Background = Brushes.AliceBlue;
            matchGrid.PreviewMouseDown += dragDrop.MatchElementMouseDown;
            matchGrid.MouseUp += dragDrop.MatchElementMouseUp;
            matchGrid.MouseMove += dragDrop.MatchElementdMouseMove;
            
            ScrollViewer scroll=new ScrollViewer();
            matchGrid.Children.Add(scroll);
            
            WrapPanel panel;
            panel = CreateContentBlock(match);
            matchGrid.HorizontalAlignment = HorizontalAlignment.Left;
            matchGrid.VerticalAlignment = VerticalAlignment.Top;
            matchGrid.Children.Add(panel);
            
            return matchGrid;
        }

        private Panel CreateSlotElement(XmlNode slot)
        {
            Grid slotElement = new Grid();
            slotElement.Background = Brushes.Aquamarine;
            slotElement.Height = 100;
            slotElement.Width = 400;
            slotElement.ShowGridLines = true;
            ColumnDefinition col1 = new ColumnDefinition();
            ColumnDefinition col2 = new ColumnDefinition();
            slotElement.ColumnDefinitions.Add(col1);
            slotElement.ColumnDefinitions.Add(col2);
           
            StackPanel slotArea = new StackPanel();
            slotArea.Background = Brushes.AntiqueWhite;
            slotArea.Width = 200;
            slotArea.Height = 100;
            WrapPanel contentArea = new WrapPanel();
           
            contentArea=CreateContentBlock(slot);
            contentArea.Width = 200;
            contentArea.Height = 100;
            Grid.SetColumn(slotArea, 0);
            Grid.SetColumn(contentArea, 1);
            slotElement.Children.Add(slotArea);
            slotElement.Children.Add(contentArea);
            //slotElement.HorizontalAlignment = HorizontalAlignment.Left;
            slotElement.VerticalAlignment = VerticalAlignment.Top;

            return slotElement;
        }

        private void CreateRadioButton()
        {}
    
        private void CreateCheckBox()
        { }

        private void CreateOpenTextBox()
        { }
        /// <summary>
        /// Создаем контент блок
        /// </summary>
        /// <param name="question">Узел в XML, потомком которого является ContentBlock</param>
        /// <returns> Возвращаем XAML разметку содержащегося контента</returns>
        public WrapPanel CreateContentBlock(XmlNode node)
        {
            XmlNode contentBlock = node.FirstChild;
            WrapPanel result = new WrapPanel();
            List<URLTypePair> pairs;
            DrawContentBlockAction drawer;
            pairs = GetUrlTypePairs(contentBlock);
            string typeLayout = contentBlock.Attributes.GetNamedItem("layout").Value;
            Layouts.TryGetValue(typeLayout, out drawer);
            return result = drawer(pairs);
        }

  
        public List<URLTypePair> GetUrlTypePairs(XmlNode contentBlock)
        {
            List<URLTypePair> myUrlTypePairs = new List<URLTypePair>();
            URLTypePair pair;
            foreach (XmlNode contentItem in contentBlock.ChildNodes)
            {
                pair.type = contentItem.Name;
                if (contentItem.Name != "text")
                    pair.url = contentItem.Attributes.GetNamedItem("url").Value;
                else 
                    pair.url = contentItem.InnerXml;

                myUrlTypePairs.Add(pair);
            }
            return myUrlTypePairs;
        }

        /// <summary>
        /// Событие, происходящее на изменение состояния RadioButton и CheckBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Checked_Button(object sender, RoutedEventArgs e)
        {
            int k = steController.currentPage;
            XmlNode currentTaskResult = steController.GetTaskResult(k);
            XmlNode one = currentTaskResult.SelectSingleNode(String.Format("//*//*[@id='{0}']|//*//*[@match-id='{0}']", (sender as ToggleButton).Name));
            one.Attributes["selected"].Value = "true";
        }
        /// <summary>
        /// В основном нужно для RadioButton, так как она имеет свойство выключаться при переключении.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Unchecked_Button(object sender, RoutedEventArgs e)
        {
            int k = steController.currentPage;
            XmlNode currentTaskResult = steController.GetTaskResult(k);
            XmlNode one = currentTaskResult.SelectSingleNode(String.Format("//*//*[@id='{0}']|//*//*[@match-id='{0}']", (sender as ToggleButton).Name));
            one.Attributes["selected"].Value = "false";

        }
        /// <summary>
        /// Для текста, берем содержимое
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_Changed(object sender, RoutedEventArgs e)
        {
            int k = steController.currentPage;
            XmlNode currentTaskResult = steController.GetTaskResult(k);
            XmlNode one = currentTaskResult.SelectSingleNode(String.Format("//*//*[@id='{0}']|//*//*[@match-id='{0}']", (sender as TextBox).Name));
            one.Attributes["value"].Value = (sender as TextBox).Text;
            //MessageBox.Show((sender as TextBox).Name);
        }

     
        
#endregion
#region Create Task Results
        
#endregion

}
    #region SteDragDrop

    class SteDragDrop
    {
        bool moving = false;
        Panel currentPanel;
        public List<Panel> slots = new List<Panel>();
        Point startPosition;
        public STEController steController;

        /// <summary>
        ///  Отслеживание нажатий на внутрений элемент
        /// </summary>
        /// <param name="sender">Наш матч, который мы двигаем</param>
        /// <param name="e"></param>
        public void MatchElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            moving = true;
            currentPanel = (Panel)sender;
            Match_Up(currentPanel.Name);
            startPosition = e.GetPosition(currentPanel);
            currentPanel.CaptureMouse();
        }

        /// <summary>
        /// Отслеживание движения мыши из внешнего контейнера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MatchElementdMouseMove(object sender, MouseEventArgs e)
        {
            if (moving == true)
            {
                Point currentPoint = e.GetPosition((Panel)sender);
                double x = currentPoint.X - startPosition.X;
                double y = currentPoint.Y - startPosition.Y;
                currentPanel.Margin = new Thickness(x, y, 0, 0);
            }
        }

        /// <summary>
        /// Отпускаем кнопку мыши. Нужно проверить принадлежность текущего объекта к набору слотов, хранящихся в Map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MatchElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            moving = false;
            if (currentPanel != null)
            {
                currentPanel.ReleaseMouseCapture();
                foreach (Panel slot in slots)
                {
                    if (IsSlotMatched(slot))
                    {
                        Match_Drop(currentPanel.Name, slot.Name);
                        currentPanel.Margin = new Thickness(slot.Margin.Left, slot.Margin.Top, 0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// При отпускании мыши помещаем матч в определенный слот 
        /// </summary>
        /// <param name="match_id"></param>
        /// <param name="slot_id"></param>
        public void Match_Drop(string match_id, string slot_id)
        {
            int k = steController.currentPage;
            XmlNode currentTaskResult = steController.GetTaskResult(k);
            XmlNode one = currentTaskResult.SelectSingleNode(String.Format("//*//*[@match-id='{0}']", match_id));
            one.Attributes["slot-id"].Value = slot_id;

        }

        /// <summary>
        /// При подъеме матча мы должны удалить запись о нём
        /// </summary>
        /// <param name="match_id"></param>
        public void Match_Up(string match_id)
        {
            int k = steController.currentPage;
            XmlNode currentTaskResult = steController.GetTaskResult(k);
            XmlNode one = currentTaskResult.SelectSingleNode(String.Format("//*//*[@match-id='{0}']", match_id));
            one.Attributes["slot-id"].Value = "empty";
        }

        /// <summary>
        /// Срабатывает при выходе мыши за границы внешнего элемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MathingFieldMouseLeave(object sender, MouseEventArgs e)
        {
            moving = false;
        }

        /// <summary>
        /// Определяет, лежит ли правый верхний угол нашего объекта внутри данного слота
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        private bool IsSlotMatched(Panel panel)
        {
            double x = currentPanel.Margin.Left;
            double y = currentPanel.Margin.Top;
            double x1 = panel.Margin.Left;
            double y1 = panel.Margin.Top;
            double h1 = panel.Height;
            double w1 = panel.Width;
            Point p1 = Mouse.GetPosition(panel);
            Point p2 = Mouse.GetPosition(currentPanel);
            //double r = Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2));
            return (x >= x1 && (x <= x1 + w1) && y >= y1 && y <= y1 + h1);
            // return r < 100;
        }

       

    }
#endregion
}
