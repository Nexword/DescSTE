using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace STE
{
    public class STEXmlProcessor
    {
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

        private XmlNode CreateResultNode(XmlNode root, XmlDocument doc)
        {
            XmlNode answerNode = doc.CreateElement(root.LastChild.Name);
            if (root.LastChild.Name == "matching-answer") CreateMatchingAnswerNode(root, answerNode, doc);
            else
                CreateButtonAnswerNode(root, answerNode, doc);

            return answerNode;
        }

        private XmlNode CreateButtonAnswerNode(XmlNode root, XmlNode answerNode, XmlDocument doc)
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

        private XmlNode CreateMatchingAnswerNode(XmlNode root, XmlNode answerNode, XmlDocument doc)
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
    }
}
