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
        public Grid userControlGrid;
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
    <Grid Name='UserControlGrid' Margin='0' ShowGridLines='True' Grid.Row='1'>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width='400*'></ColumnDefinition>
                <ColumnDefinition Width='80*'></ColumnDefinition>
            </Grid.ColumnDefinitions>
        <ScrollViewer Grid.Column='1' VerticalScrollBarVisibility='Auto'>
                <WrapPanel Orientation='Vertical'>
                    <TextBox Name='TimeTextBox' TextAlignment='Center' Width='Auto'>
                        14:55:22
                    </TextBox>
                    <WrapPanel Orientation='Horizontal'>
                        <Button Name='PreviousButton' Click='PreviousButtonClick'>Предыдущее</Button>
                        <Button Name='NextButton' Click='NextButtonClick'>Следующее</Button>
                        <Button Name='EndTest' Click='EndTestClick'>Закончить</Button>
                    </WrapPanel>
                </WrapPanel>
            </ScrollViewer>
		<ScrollViewer Grid.Column='0' Grid.Row='1' VerticalScrollBarVisibility='Auto' >
			<WrapPanel Name='ButtonWrapPanel' Grid.Column='0' Grid.Row='1' Margin='5' >
			</WrapPanel>
		</ScrollViewer>
	</Grid>
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

        public void CreateUserControlElements()
        {
            Button myEndTestButton = new Button();
            Button myNextButton = new Button();
            Button myPreviousButton = new Button();
            myEndTestButton.Content = "Закночить тестирование";
            myEndTestButton.SetValue(Grid.ColumnProperty, 1);
            userControlGrid.Children.Add(myEndTestButton);
            
        }

        public void CreateMainElements()
        {
            mainStackPanel = (StackPanel)this.FindName("MainStackPanel");
            buttonWrapPanel = (WrapPanel)this.FindName("ButtonWrapPanel");
            CreateWrapPanelButtons();
            
        }

        private void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentPage = CurrentPage - 1;
        }
        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentPage = CurrentPage + 1;
        }
        private void EndTestClick(object sender, RoutedEventArgs e)
        {
 
        }

        public int CurrentPage
        {
            get
            {
                return currentPage;
            }

            set
            {
                if (value >= 0 && value < testPages.Count)
                {
                    currentPage = value;
                    mainStackPanel.Children.Clear();
                    mainStackPanel.Children.Add(testPages[value]);
                }
            }
        }
    }
}
