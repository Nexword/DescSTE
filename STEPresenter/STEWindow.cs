using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Markup;

namespace STE
{
    public class STEWindow : Window
    {
        public List<StackPanel> testPages;
        public StackPanel mainStackPanel;
        public WrapPanel buttonWrapPanel;
        private int currentPage;
        public static STEWindow LoadWindowFromXaml()
        {
            string xaml =
            @"<local:STEWindow xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
            xmlns:local='clr-namespace:STE;assembly=STEPresenter' 
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            xmlns:sys='clr-namespace:System;assembly=mscorlib' WindowState='Maximized'>
            
    <Grid Name='grid1' ShowGridLines='True' Margin='0' >
        <Grid.RowDefinitions>
            <RowDefinition Height='400*'>
            </RowDefinition>
            <RowDefinition Height='40*'>
            </RowDefinition>
        </Grid.RowDefinitions>
		<ScrollViewer Grid.Column='1' Grid.Row='1' VerticalScrollBarVisibility='Auto' >
			<WrapPanel Name='ButtonWrapPanel' Grid.Column='1' Grid.Row='1' Margin='5' >
			</WrapPanel>
		</ScrollViewer>
	
		<ScrollViewer VerticalScrollBarVisibility='Auto' >
            <StackPanel Name='MainStackPanel' Width='Auto' >
			</StackPanel>
		</ScrollViewer>
   </Grid>
</local:STEWindow> ";
            StringReader reader = new StringReader(xaml);
            XmlReader xamlStream = XmlReader.Create(reader);
            STEWindow window = (STEWindow)XamlReader.Load(xamlStream);
            return window;
        }

        public void CreateWrapPanelButtons()
        {
            for (int i = 0; i < testPages.Count; i++)
            {
                Button myButton = new Button();
                myButton.Content = i + 1;
                myButton.MinWidth = 75;
                //myButton.Click += Button_Click1;
                int pageNumber = i;
                myButton.Click +=  new RoutedEventHandler(
                    delegate(object sender, RoutedEventArgs e) 
                    { 
                        this.CurrentPage = pageNumber; 
                    }
                );
                myButton.Margin = new Thickness(3);
                buttonWrapPanel.Children.Add(myButton);
            }
        }

        public void CreateMainElements()
        {
            mainStackPanel = (StackPanel)this.FindName("MainStackPanel");
            buttonWrapPanel = (WrapPanel)this.FindName("ButtonWrapPanel");
            CreateWrapPanelButtons();
        }



        public int CurrentPage
        {
            get
            {
                return currentPage;
            }

            set
            {
                currentPage = value;
                mainStackPanel.Children.Clear();
                mainStackPanel.Children.Add(testPages[value]);
            }
        }
    }
}
