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
       
        public List<string> contentStrings;//переменная для хранения контента. Обращение к контенту происходит в разных частях программы
        
       /// <summary>
       /// Ищем содержимое по нашему id во внешнем файле xmlContent
       /// </summary>
       /// <param name="id"></param>
       /// <param name="xmlContent"></param>
       /// <returns>Возвращает XAML описание содержащегося контента</returns>
        public List<string> FindContentById(string id, XmlDocument xmlContent)
        {
            List<string> cString = new List<string>();
            string itemUrl, itemId;
            XmlNode contentBlock = xmlContent.SelectSingleNode(String.Format("//content-set//content-block[@id='{0}']",id));
            foreach(XmlNode contentItem in contentBlock.ChildNodes)
            {
                itemId = contentItem.Attributes.GetNamedItem("id").Value;
                itemUrl = contentItem.Attributes.GetNamedItem("url").Value;
                //cString.Add(CreateBaseContentElement(url, itemName));
                ///Зачем это здесь ? rowPosition++;
                string sElement = "";
                switch (contentItem.Name)
                {
                    case "text": 
                        sElement = GetTextFromFile(itemUrl); 
                        sElement = @"<TextBlock FontSize='25' Margin='5' TextWrapping='Wrap'>" + sElement + "</TextBlock>"; 
                        
                        break;
                    case "image":
                        sElement = @"<Image Height='400' Width='400' Source='pack://siteoforigin:,,,/"+itemUrl+"' />";
                        break;
                    case "video":
                        sElement = @"<MediaElement Height='600' Width='800' Source='pack://siteoforigin:,,,/" + itemUrl + "' />";
                        break;
                    case "audio": 
                        sElement= @"<MediaElement Height='Auto' Width='Auto' Source='pack://siteoforigin:,,,/" + itemUrl + "' />"; 
                        break;

                }
                cString.Add(sElement);
            }

            return cString;
        }

        
       
        /// <summary>
        /// Метод создает Textblock с содержимым из нужной строки в файле
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetTextFromFile(string fileName)
        {
            int k = fileName.IndexOf('#');
            string fName = fileName.Substring(0, k) + ".txt";
            int strSkip = Convert.ToInt32(fileName.Substring(k + 1, fileName.Length - k - 1));
            string note = File.ReadLines(fName).Skip(strSkip-1).First();
            
            return note;
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

            foreach (XmlNode node in testNodes)
            {
                if (node.Name == "note") pages.Add(CreateTestNotePage(node, xmlContentSet));
                //  if (node.Name == "block") pages.Concat(CreateTestPages(node,xmlTask, xmlContent));
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
            contentStrings = FindContentById(note.Attributes.GetNamedItem("key").Value, xmlContentSet);
            string page = 
            @"<ScrollViewer VerticalScrollBarVisibility='Auto'>
            <StackPanel>";
           
            for (int i = 0; i <= contentStrings.Count-1; i++)
                page += contentStrings[i];            
            page += "</StackPanel> </ScrollViewer>";      
            
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

                    case "multiple-answer": page+=CreateMultipleAnswerElement(child,xmlContentSet); break;
                    case "open-answer": page+=CreateOpenAnswerElement(child,xmlContentSet); break;
                    case "single-semiopen-answer": page+=CreateSingleSemiopenAnswerElement(child,xmlContentSet); break;
                    case "multiple-semiopen-answer": page+=CreateMultipleSemiopenAnswerElement(child,xmlContentSet); break;
                    case "matching-answer": page+=CreateMatchingAnswerElement(child,xmlContentSet); break;
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
            List<string> sElements = new List<string>();
            sElements = FindContentById(contentId, xmlContent);
            ///Костыль для демонстрации возможностей. Убери.
            string mySuperString = "<WrapPanel Orientation='Vertical'>";
            for (int i = 0; i < sElements.Count;i++ )
            {
                mySuperString += sElements[i];
            }
                return mySuperString+"</WrapPanel>";
        }
       /// <summary>
       /// Не дописан гибкий вариант компоновки
       /// </summary>
       /// <param name="node"></param>
       /// <param name="xmlContent"></param>
       /// <returns></returns>
        public string CreateSingleAnswerElement(XmlNode node,XmlDocument xmlContent)
        {
            List<List<string>> sOptionElements = new List<List<string>>();
            string sSingleAnswerElement = "";
            foreach(XmlNode option in node.ChildNodes)
            {
                sOptionElements.Add(FindContentById(option.Attributes.GetNamedItem("key").Value, xmlContent));
                sSingleAnswerElement += "<RadioButton> "+ sOptionElements.Last()[0]+ " </RadioButton>";
            }

            return @"<GroupBox>
                    <WrapPanel Orientation='Vertical'> "+
                    sSingleAnswerElement+
                        " </WrapPanel> </GroupBox>";
        }
        /// <summary>
        /// Не дописан гибкий вариант компоновки!!!!!!!
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xmlContent"></param>
        /// <returns></returns>
        public string CreateMultipleAnswerElement(XmlNode node, XmlDocument xmlContent)
        {

            List<List<string>> sOptionElements = new List<List<string>>();
            string sMultipleAnswerElement = "";
            foreach (XmlNode option in node.ChildNodes)
            {
                sOptionElements.Add(FindContentById(option.Attributes.GetNamedItem("key").Value, xmlContent));
                sMultipleAnswerElement += "<CheckBox> " + sOptionElements.Last()[0] + " </CheckBox>";
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
            return @" <TextBox>"+
                    sOpenAnswerElement+
                        "</TextBox>";
        }

        public string CreateSingleSemiopenAnswerElement(XmlNode node, XmlDocument xmlContent)
        {
            string SingleSemiopenAnswerElement="";
            List<List<string>> sOptionElements = new List<List<string>>();
            foreach (XmlNode option in node.ChildNodes)
            {
                if (option.Name == "option")
                {
                    sOptionElements.Add(FindContentById(option.Attributes.GetNamedItem("key").Value, xmlContent));
                    SingleSemiopenAnswerElement += "<RadioButton Margin='5'> " + sOptionElements.Last()[0] + " </RadioButton>";
                } else
                {
                    SingleSemiopenAnswerElement += "<TextBox Margin='5'>" + "Введите ответ" + " </TextBox>";
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

        public void LeadToXAML(List<string> sElements)
        {

        }


    }
   
}
