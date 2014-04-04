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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
namespace STE
{
    public interface ISTEProcessor
    {
        List<string> CreatePages(List<string> xmlPages);
    }


    public  class STEProcessor
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


        private STEController steController;

        public STEProcessor(STEController ste)
        {
            steController = ste;
        }
        /// <summary>
        /// Создание страниц с разметкой XAML
        /// </summary>
        /// <param name="xmlPages">Таски приходят в формате XML</param>
        /// <returns>Возвращает набор страниц с разметкой XAML</returns>
        public void CreatePages(List<string> xmlPages)
        {
            List<StackPanel> xamlPages = new List<StackPanel>();
            List<XmlNode> xmlTaskResult=new List<XmlNode>();
            XmlDocument doc = new XmlDocument();
            foreach(string xmlPage in xmlPages)
            {
                
                doc.LoadXml(xmlPage);
                xmlTaskResult.Add(CreateTestResult(xmlPage));
                XmlNode currentPage = doc.DocumentElement;
                if (currentPage.Name == "task") xamlPages.Add(CreateTask(currentPage));
            }
            steController.storage.xmlTaskResults = xmlTaskResult;
            steController.storage.wpfPage = xamlPages;
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
                       // page += CreateMatchingAnswerElement(child);
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
                optionId = optionNode.Attributes.GetNamedItem("key").Value;
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
        public string CreateMatchingAnswerElement(XmlNode node)
        {
            string MatchingAnswerElement = "";
            return MatchingAnswerElement;
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
            XmlNode currentTaskResult = steController.storage.xmlTaskResults[k];
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
            XmlNode currentTaskResult = steController.storage.xmlTaskResults[k];
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
            XmlNode currentTaskResult = steController.storage.xmlTaskResults[k];
            XmlNode one = currentTaskResult.SelectSingleNode(String.Format("//*//*[@id='{0}']|//*//*[@match-id='{0}']", (sender as TextBox).Name));
            one.Attributes["value"].Value = (sender as TextBox).Text;
            //MessageBox.Show((sender as TextBox).Name);
        }
        
#endregion
#region Create Task Results
        public XmlNode CreateTestResult(string xmlPage)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlPage);
            XmlNode task = doc.CreateElement("task");
            XmlNode root = doc.DocumentElement;
            XmlAttribute typeAttr = doc.CreateAttribute("id");
            typeAttr.Value = doc.DocumentElement.Attributes.GetNamedItem("id").Value;
            task.Attributes.Append(typeAttr);
            task.AppendChild(CreateResultNode(root, doc));

            return task;
        }

        public XmlNode CreateResultNode(XmlNode root, XmlDocument doc)
        {
            XmlNode answerNode = doc.CreateElement(root.LastChild.Name);
            if (root.LastChild.Name == "matching-answer") CreateMatchingAnswerNode(root, answerNode, doc);
            else
                CreateButtonAnswerNode(root, answerNode, doc);

            return answerNode;
        }

        public XmlNode CreateButtonAnswerNode(XmlNode root, XmlNode answerNode, XmlDocument doc)
        {
            foreach (XmlNode option in root.LastChild.ChildNodes)
            {
                XmlAttribute selected = doc.CreateAttribute("selected");
                selected.Value = "false";
                XmlAttribute id = doc.CreateAttribute("id");
                id.Value = option.Attributes.GetNamedItem("id").Value;
                XmlElement optionNode = doc.CreateElement(option.Name);
                optionNode.Attributes.Append(id);
                optionNode.Attributes.Append(selected);

                if (option.Name == "open-option")
                {
                    XmlAttribute textValue = doc.CreateAttribute("value");
                    textValue.Value = "some text";
                    optionNode.Attributes.Append(textValue);
                }
                answerNode.AppendChild(optionNode);
            }
            return answerNode;
        }

        public XmlNode CreateMatchingAnswerNode(XmlNode root, XmlNode answerNode, XmlDocument doc)
        {
            root = root.LastChild;
            foreach (XmlNode match in root.FirstChild.ChildNodes)
            {
                XmlAttribute matchId = doc.CreateAttribute("match-id");
                XmlAttribute slotId = doc.CreateAttribute("slot-id");
                matchId.Value = match.Attributes.GetNamedItem("id").Value;
                slotId.Value = "false";
                XmlElement matchNode = doc.CreateElement("matching");
                matchNode.Attributes.Append(matchId);
                matchNode.Attributes.Append(slotId);
                answerNode.AppendChild(matchNode);
            }
            return answerNode;
        }
#endregion

    }
   
}
