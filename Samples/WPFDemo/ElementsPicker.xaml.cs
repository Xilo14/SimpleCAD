using SimpleCAD.Core.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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

namespace WPFDemo.UserControls
{
    /// <summary>
    /// Логика взаимодействия для Elements.xaml
    /// </summary>
    public partial class ElementsPicker : UserControl
    {
        public class ListViewItemElement
        {
            public BitmapImage Image { get; set; }
            public string Label { get; set; }
        }
        public Element ChoosenElement;
        private ObservableCollection<ListViewItemElement> lvItems;
        private List<Element> elements = new()
        {
            new ChipElement(),
            new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Left } }
            },
        };
        public ElementsPicker()
        {
            InitializeComponent();
            InitElements();
        }

        private void InitElements()
        {
            lvItems = new();
            lvItems.Add(new()
            {
                Image = MainWindow.ToBitmapImage((Bitmap)Bitmap.FromFile("start.png")),
                Label = "Cursor",
            });
            foreach (var el in elements)
            {
                var sch = new TracingScheme()
                {
                    Graph = new(1, 1),
                };
                sch.Graph.GetCell(0, 0).Element = el;
                var svg = WPFDemo.MainWindow.SchToSvg(sch);
                lvItems.Add(new() {
                    Image = MainWindow.ToBitmapImage(svg.Draw()),
                    Label = el.GetType().Name,
                });
            }
            lvElements.ItemsSource = lvItems;
        }

        private void lvElements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvElements.SelectedIndex < 1)
            {
                ChoosenElement = null;
                return;
            }
                var value = (ListViewItemElement)lvElements.SelectedValue;
            if (value == null)
                return;
            if (lvElements.SelectedIndex == 2 )
            {
                CheckBoxes.IsEnabled = true;
            }
            else
            {
                CheckBoxes.IsEnabled = false;
            }
            ChoosenElement = elements[lvElements.SelectedIndex - 1];
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            
            if (lvElements.SelectedIndex < 0 || !(elements[lvElements.SelectedIndex - 1] is ConductorElement))
                return;

            var el = new ConductorElement();
            //var el = (ConductorElement)elements[lvElements.SelectedIndex];

            el.ConductorWires = new() {
                new(),    
            };


            if ((bool)LeftCheckbox.IsChecked)
                el.ConductorWires[0].Add(ConductorElement.ConductorSide.Left);

            if ((bool)TopCheckbox.IsChecked)
                el.ConductorWires[0].Add(ConductorElement.ConductorSide.Top);

            if ((bool)RightCheckbox.IsChecked)
                el.ConductorWires[0].Add(ConductorElement.ConductorSide.Right);

            if ((bool)BottomCheckbox.IsChecked)
                el.ConductorWires[0].Add(ConductorElement.ConductorSide.Bottom);


            //var sch = new TracingScheme()
            //{
            //    Graph = new(1, 1),
            //};
            //sch.Graph.GetCell(0, 0).Element = el;
            //var svg = WPFDemo.MainWindow.SchToSvg(sch);
            //var img = MainWindow.ToBitmapImage(svg.Draw());
            //lvItems[lvElements.SelectedIndex].Image = img;

            elements[lvElements.SelectedIndex - 1] = el;
            lvElements.ItemsSource = null;
            InitElements();
            lvElements.SelectedIndex = 2;
        }
    }
}
