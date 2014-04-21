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


namespace STE
{
    class STEDragDrop
    {
        bool moving = false;
        Panel currentPanel;
        public List<Panel> slots = new List<Panel>();
        Point startPosition;
        
        /// <summary>
        ///  Отслеживание нажатий на внутрений элемент
        /// </summary>
        /// <param name="sender">Наш матч, который мы двигаем</param>
        /// <param name="e"></param>
        public void MatchElementMouseDown(object sender,MouseButtonEventArgs e)
        {
            moving = true;
            currentPanel = (Panel)sender;
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
            if (currentPanel!=null)
            {
                currentPanel.ReleaseMouseCapture();
                foreach(Panel slot in slots)
                {
                    if (IsSlotMatched(slot))
                    {
                        
                        currentPanel.Margin = new Thickness(slot.Margin.Left, slot.Margin.Top, 0, 0);
                        
                    }
                }
            }
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
}
