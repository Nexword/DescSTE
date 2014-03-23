using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace STE
{
    public interface ISTEProcessor
    {
        List<string> CreatePages(XmlNode test, XmlDocument xmlTaskSet, XmlDocument xmlContentSet);
    }


    public  class STEProcessor:ISTEProcessor
    {
        static List<string> contentString = new List<string>();
        
        public struct URLTypePair
        {
            public string url;
            public string type;
        }

        private delegate string DrawContentBlockAction(List<URLTypePair> urlTypePairs);

        /// <summary>
        /// Считываем весь текст, относящийся к тесту
        /// </summary>
        /// <param name="fileName">Имя файла с текстом. По умолчанию text.txt</param>
        public void GetTextFromFile(string fileName)
        {
            StreamReader sStream = new StreamReader(fileName);
            while (!sStream.EndOfStream)
            {
                contentString.Add(sStream.ReadLine());
            }
            sStream.Close();
        }

#region Dictionary definition
       
        /*private static string GetTextFromFile(string fileName)
        {
            int k = fileName.IndexOf('#');
            string fName = fileName.Substring(0, k) + ".txt";
            int strSkip = Convert.ToInt32(fileName.Substring(k + 1, fileName.Length - k - 1));
            string note = File.ReadLines(fName).Skip(strSkip - 1).First();

            return note;
        }*/
        

        Dictionary<string, DrawContentBlockAction> Layouts = new Dictionary<string, DrawContentBlockAction>
            {
                { "typeA", 
                    delegate(List<URLTypePair> pairs)
                    { 
                        string result = " <WrapPanel Orientation='Vertical'> ";         
                        for (int i = 0; i < pairs.Count;i++ )
                        {
                            string sElement = "";
                            switch (pairs[i].type)
                            {
                   
                                case "text":
                                    int k = pairs[i].url.IndexOf('_');
                                    sElement = contentString[Convert.ToInt32(pairs[i].url.Substring(k + 1, pairs[i].url.Length - k - 1))-1];
                                    sElement = @"<TextBlock FontSize='25' Margin='5' TextWrapping='Wrap'>" + sElement + "</TextBlock>";
                                    break;
                                case "image":
                                    sElement = @"<Image Height='400' Width='400' Source='pack://siteoforigin:,,,/" + pairs[i].url + "' />";
                                    break;
                                case "video":
                                    sElement = @"<MediaElement Height='Auto' MaxWidth='800' MaxHeight='600'  Margin='5' Width='Auto' Source='pack://siteoforigin:,,,/" + pairs[i].url + "' />";
                                    break;
                                case "audio":
                                    sElement = @"<MediaElement Height='Auto' Width='Auto' Source='pack://siteoforigin:,,,/" + pairs[i].url + "' />";
                                    break;
                            }
                            result += sElement+" ";
                        }            
                        return result + " </WrapPanel>"; 
                    } 
                },
                  
            };

#endregion
        
        /// <summary>
        ///  Управляющий метод для создания блока контента. Содержит функцию формирования URL-ТИП,
        /// функцию отрисовки всего блока контента по парам URL-ТИП
        /// </summary>
        /// <param name="id"> идентификатор контент блока</param>
        /// <param name="xmlContent"> файл с описанием всех контент блоков</param>
        public string CreateContentBlock(string id,XmlDocument xmlContent)
        {
            XmlNode contentBlock = xmlContent.SelectSingleNode(String.Format("//content-set//content-block[@id='{0}']", id));
            string typeLayout = "";
            string result="";
            List<URLTypePair> pairs;
            DrawContentBlockAction drawer; 
            pairs=GetUrlTypePairs(contentBlock);
            typeLayout = contentBlock.Attributes.GetNamedItem("layout").Value;
            Layouts.TryGetValue(typeLayout, out drawer);
            return result = drawer(pairs);
        }

        /// <summary>
        ///  Формируем список пар URL-ТИП
        /// </summary>
        /// <param name="id">id content-block</param>
        /// <param name="xmlContent"> файл со всеми content-blocks</param>
        /// <returns>Функция возвращает список пар URL-ТИП</returns>
        public List<URLTypePair> GetUrlTypePairs(XmlNode contentBlock)
        {
            List<URLTypePair> myUrlTypePairs = new List<URLTypePair>();
            URLTypePair pair;
            foreach (XmlNode contentItem in contentBlock.ChildNodes)
            {
                pair.type=contentItem.Name;
                pair.url = contentItem.Attributes.GetNamedItem("url").Value;
                myUrlTypePairs.Add(pair);
            }
            return myUrlTypePairs;
        }



        /// <summary>
        /// Для каждого теста создается набор страниц, из которых он состоит. Позже страницы 
        /// будут отосланы на сервер
        /// </summary>
        /// <param name="test"></param>
        /// <param name="xmlTaskSet"> xml документ с описанием заданий</param>
        /// <param name="xmlContentSet"> xml документ с описанием содержимого</param>
        /// <returns>Список страниц для отдельного теста</returns>
        public List<string> CreatePages(XmlNode test, XmlDocument xmlTaskSet, XmlDocument xmlContentSet)
        {
            List<string> pages = new List<string>();
            XmlNodeList testNodes = test.ChildNodes;
            GetTextFromFile("text.txt");
            foreach (XmlNode node in testNodes)
            {
                if (node.Name == "note") pages.Add(CreateTestNotePage(node, xmlContentSet));
                if (node.Name == "block") pages.AddRange(CreateTestPages(node, xmlTaskSet, xmlContentSet));

            }

            return pages;
        }

        /// <summary>
        /// Создаем обычную страницу теста с заданиями
        /// </summary>
        /// <param name="node"> узел xml-документа Test со значением block</param>
        /// <param name="xmlTaskSet"> xml документ с описанием заданий </param>
        /// <param name="xmlContentSet"> xml документ с описанием содержимого</param>
        /// <returns> Возвращает набор страниц с заданиями для теста</returns>
        public List<string> CreateTestPages(XmlNode node, XmlDocument xmlTaskSet, XmlDocument xmlContentSet)
        {   
            List<string> pages = new List<string>();
            foreach(XmlNode child in node.ChildNodes)
            {
                if (child.Name == "note") pages.Add(CreateTestNotePage(child, xmlContentSet));
                if (child.Name == "task") pages.Add(CreateTask(child.Attributes.GetNamedItem("key").Value,xmlTaskSet, xmlContentSet));
            }
            
            return pages;
        }

        /// <summary>
        /// Создаем страницу с заметками о тесте
        /// </summary>
        /// <param name="note"></param>
        /// <param name="xmlContentSet"></param>
        /// <returns>Возвращает страницу с заметками к тесту</returns>
        public string CreateTestNotePage(XmlNode note, XmlDocument xmlContentSet)
        {
            string contentStrings;
            contentStrings = CreateContentBlock(note.Attributes.GetNamedItem("key").Value, xmlContentSet);
            string page = 
            @"<ScrollViewer VerticalScrollBarVisibility='Auto'>
            <StackPanel>";
            page += contentStrings + "</StackPanel> </ScrollViewer>";      
            
            return page;
        }

        /// <summary>
        /// Создаем страницу с заданием
        /// </summary>
        /// <param name="key"> по этому параметру будет производится поиск</param>
        /// <param name="xmlTaskSet"> xml документ с описанием заданий</param>
        /// <param name="xmlContentSet"> xml документ с описанием содержимого</param>
        /// <returns> возвращает страницу теста с заданием</returns>
        public string CreateTask(string key, XmlDocument xmlTaskSet, XmlDocument xmlContentSet)
        {
            string page = "";
            XmlNode task = xmlTaskSet.SelectSingleNode(String.Format("//task-set//task[@id='{0}']", key));
            foreach (XmlNode child in task.ChildNodes)
            {
                switch (child.Name)
                {
                    case "question": 
                        page += CreateQuestionElement(child.Attributes.GetNamedItem("key").Value, xmlContentSet); 
                        break;
                    case "single-answer": 
                        page+=CreateSingleAnswerElement(child,xmlContentSet); 
                        break;
                    case "multiple-answer": 
                        page+=CreateMultipleAnswerElement(child,xmlContentSet);
                        break;
                    case "open-answer": 
                        page+=CreateOpenAnswerElement(child,xmlContentSet); 
                        break;
                    case "single-semiopen-answer": 
                        page+=CreateSingleSemiopenAnswerElement(child,xmlContentSet); 
                        break;
                    case "multiple-semiopen-answer":
                        page+=CreateMultipleSemiopenAnswerElement(child,xmlContentSet); 
                        break;
                    case "matching-answer": 
                        page+=CreateMatchingAnswerElement(child,xmlContentSet); 
                        break;
                }
            }
            
            return page;
        }

        /// <summary>
        /// Формирование элемента вопрос. Не реализовано(!!!) формирование сложной структуры.
        /// </summary>
        /// <param name="contentId"> Идентификатор блока описания вопроса</param>
        /// <param name="xmlContent"> Xml документ с описаниями всех блоков</param>
        /// <returns></returns>
        public string CreateQuestionElement(string contentId,XmlDocument xmlContent)
        {
            return CreateContentBlock(contentId, xmlContent); ;
        }

       /// <summary>
       /// Минимум написан. Пока есть 1 вариант компоновки
       /// </summary>
       /// <param name="node"></param>
       /// <param name="xmlContent"></param>
       /// <returns></returns>
        public string CreateSingleAnswerElement(XmlNode node,XmlDocument xmlContent)
        {
            List<string> sOptionElements = new List<string>();
            string sSingleAnswerElement = "";
            string keyOption = "";
            foreach(XmlNode option in node.ChildNodes)
            {
                keyOption = option.Attributes.GetNamedItem("key").Value;
                sOptionElements.Add(CreateContentBlock(keyOption, xmlContent));
                sSingleAnswerElement += "<RadioButton Click ='Checked_Button' Unchecked='Unchecked_Button'  Name='" + keyOption + "'> " + sOptionElements.Last() + " </RadioButton>";
            }

            return @"<GroupBox>
                    <WrapPanel Orientation='Vertical'> "+
                    sSingleAnswerElement+
                        " </WrapPanel> </GroupBox>";
        }

        /// <summary>
        /// Минимум написан. Пока есть 1 вариант компоновки
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xmlContent"></param>
        /// <returns></returns>
        public string CreateMultipleAnswerElement(XmlNode node, XmlDocument xmlContent)
        {

            List<string> sOptionElements = new List<string>();
            string sMultipleAnswerElement = "";
            string keyOption = "";
            foreach (XmlNode option in node.ChildNodes)
            {
                keyOption = option.Attributes.GetNamedItem("key").Value;
                sOptionElements.Add(CreateContentBlock(keyOption, xmlContent));
                sMultipleAnswerElement += "<CheckBox  Name='" + keyOption + "'>" + sOptionElements.Last() + " </CheckBox>";
            }

            return @"<GroupBox>
                    <WrapPanel Orientation='Vertical'> " +
                    sMultipleAnswerElement +
                        " </WrapPanel> </GroupBox>";
        }

        /// <summary>
        /// Я тут схалтурил
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xmlContent"></param>
        /// <returns></returns>
        public string CreateOpenAnswerElement(XmlNode node, XmlDocument xmlContent)
        {
            string sOpenAnswerElement = "Напишите ответ"; //заглушка

            string keyOption = node.ChildNodes[0].Attributes.GetNamedItem("key").Value;
            return @" <TextBox "+"Name='"+keyOption+"'>"+
                    sOpenAnswerElement+
                        "</TextBox>";
        }

        public string CreateSingleSemiopenAnswerElement(XmlNode node, XmlDocument xmlContent)
        {
            string SingleSemiopenAnswerElement="";
            string keyOption = "";
            string sOptionElements = "";
            foreach (XmlNode option in node.ChildNodes)
            {
                   keyOption = option.Attributes.GetNamedItem("key").Value;
                if (option.Name == "option")
                {
                    sOptionElements=CreateContentBlock(option.Attributes.GetNamedItem("key").Value, xmlContent);
                    SingleSemiopenAnswerElement += "<RadioButton  Margin='5' Name='" + keyOption + "'>" + sOptionElements + " </RadioButton>";
                } else
                {
                    SingleSemiopenAnswerElement += " <TextBox Name='" + keyOption + "'>" + sOptionElements + "</TextBox>";
                }
            }

            return SingleSemiopenAnswerElement;
        }

        public string CreateMultipleSemiopenAnswerElement(XmlNode node, XmlDocument xmlContent)
        {
            string MultipleSemiopenAnswerElement = "";
            return MultipleSemiopenAnswerElement;
        }

        public string CreateMatchingAnswerElement(XmlNode node, XmlDocument xmlContent)
        {
            string MatchingAnswerElement = "";
            return MatchingAnswerElement;
        }
    }
   
}
