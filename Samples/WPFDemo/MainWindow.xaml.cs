using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleCAD.Core;
using SimpleCAD.Core.Interfaces;
using SimpleCAD.Core.Types;
using SimpleCAD.Tracers;
using Svg;
using Svg.Transforms;
using static SimpleCAD.Tracers.LeeTracer;
using Point = System.Windows.Point;

namespace WPFDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private Ellipse _elipseSvgAngle = new();

        private TracingScheme _inputScheme;
        private TracingScheme InputScheme
        {
            get { return _inputScheme; }
            set
            {
                _inputScheme = value;
                _currentSvgIndex = 0;
                schSnapshots.Clear();
                schSnapshots.Add(_inputScheme);
                svgSnapshots.Clear();
                var svg = SchToSvg(_inputScheme);
                svgSnapshots.Add(svg);
                _currentSvg = svg;

                StartTraceBtn.IsEnabled = true;
                StartTraceImg.IsEnabled = true;
                Tabs.IsEnabled = true;

                UpdateWireItems();
                RenderCurrentSvg();
            }
        }


        public ObservableCollection<WireItem> WireItems = new();
        public List<TracingScheme> schSnapshots = new();
        public List<SvgDocument> svgSnapshots = new();

        public int _currentSvgIndex = 0;
        SvgDocument _currentSvg;

        Point PointMousePressed;
        bool workspaceDragged = false;

        SvgScale _scaleTransform = new(1, 1);
        SvgTranslate _shearTransform = new(0, 0);
        public MainWindow()
        {
            InitializeComponent();
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        public void UpdateWireItems()
        {
            var selectedIndex = lvWires.SelectedIndex;

            WireItems.Clear();
            foreach (var wire in InputScheme.Wires)
            {
                var str = "";
                ObservableCollection<Cell> cells = new();
                foreach (var cell in wire)
                {
                    str += $"X:{cell.RowIndex},Y:{cell.ColumnIndex}; ";
                    cells.Add((Cell)cell);
                }
                WireItems.Add(new() { Label = str, cells = cells });
            }
            lvCells.ItemsSource = null;
            lvWires.SelectedIndex = selectedIndex;
        }
        public void UpdateLabelsWireItems()
        {
            var selectedIndex = lvWires.SelectedIndex;
            lvWires.ItemsSource = null;
            foreach (var wire in WireItems)
            {
                var str = "";
                foreach (var cell in wire.cells)
                {
                    str += $"X:{cell.RowIndex},Y:{cell.ColumnIndex}; ";
                }
                wire.Label = str;
            }
            lvWires.ItemsSource = WireItems;
            lvWires.SelectedIndex = selectedIndex;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Point point = new Point(30, 30);
            //Ellipse elipse = new Ellipse();

            //_elipseSvgAngle.Width = 4;
            //_elipseSvgAngle.Height = 4;

            //_elipseSvgAngle.StrokeThickness = 2;
            //_elipseSvgAngle.Stroke = System.Windows.Media.Brushes.Black;


            //DragArena.Children.Add(_elipseSvgAngle);
            StartTraceImg.IsEnabled = false;
            StopTraceImg.IsEnabled = false;

            lvWires.ItemsSource = WireItems;
            //BackTraceImg.IsEnabled = false;

            //SnapshotsText.Text = $"{_currentSvgIndex + 1}/{svgSnapshots.Count}";
            //RenderCurrentSvg();
        }
        private void TraceCurrentSch()
        {
            schSnapshots.Clear();
            svgSnapshots.Clear();

            var cad = new SimpleCADWorker();
            LeeTracer tracer;
            switch(cbMethod.SelectedIndex)
            {
                case 0:
                    tracer = cad.Tracers.Lee(InputScheme);
                    break;
                case 1:
                    tracer = cad.Tracers.Akkers(InputScheme);
                    break;
                case 2:
                    tracer = cad.Tracers.Complex(InputScheme);
                    break;
                case 3:
                    tracer = cad.Tracers.Mod3(InputScheme);
                    break;
                default:
                    tracer = cad.Tracers.Lee(InputScheme);
                    break;

            }

             
            while (tracer.CurrentState != TracingStateEnum.Finished && tracer.CurrentState
                != TracingStateEnum.CannotTraceWire)
            {
                var sch = tracer.TraceStep();

                schSnapshots.Add(sch);
                svgSnapshots.Add(SchToSvg(sch));
            }
            if (tracer.CurrentState == TracingStateEnum.CannotTraceWire)
                MessageBox.Show("Cannot trace a wire.", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static SvgDocument SchToSvg(TracingScheme sch)
        {
            var cad = new SimpleCADWorker();
            var svg = cad.Converters.TracingSchemeToSvgConverter(sch).Convert();

            var memStream = new MemoryStream();
            svg.Write(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            var newSvg = SvgDocument.Open<SvgDocument>((Stream)memStream, null);
            return newSvg;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _currentSvgIndex++;
            if (_currentSvgIndex == svgSnapshots.Count)
                _currentSvgIndex--;

            SnapshotsText.Text = $"{_currentSvgIndex + 1}/{svgSnapshots.Count}";
            RenderCurrentSvg();
        }
        private void Button_Click_First(object sender, RoutedEventArgs e)
        {
            _currentSvgIndex = 0;

            SnapshotsText.Text = $"{_currentSvgIndex + 1}/{svgSnapshots.Count}";
            RenderCurrentSvg();
        }
        private void Button_Click_Last(object sender, RoutedEventArgs e)
        {
            _currentSvgIndex = svgSnapshots.Count - 1;

            RenderCurrentSvg();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _currentSvgIndex--;
            if (_currentSvgIndex < 0)
                _currentSvgIndex++;


            RenderCurrentSvg();
        }
        private void ImageContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) _scaleTransform.X = (float)(_scaleTransform.X * 1.1);
            if (e.Delta < 0) _scaleTransform.X = (float)(_scaleTransform.X / 1.1);
            _scaleTransform.Y = _scaleTransform.X;

            RenderCurrentSvg();
        }
        private void RenderCurrentSvg()
        {
            if (_currentSvg == null)
                return;
            var svg = svgSnapshots[_currentSvgIndex];

            if (svg.Transforms == null)
            {
                svg.Transforms = new();
                svg.Transforms.Add(_scaleTransform);
                svg.Transforms.Add(_shearTransform);
            }

            WorkspaceImage.Source = ToBitmapImage(svg.Draw(
                (int)Math.Max(ImageContainer.ActualWidth, ImageContainer.ActualHeight),
                (int)Math.Max(ImageContainer.ActualWidth, ImageContainer.ActualHeight)));
            _scaleTransform = (SvgScale)svg.Transforms[0];
            _currentSvg = svg;

            SnapshotsText.Text = $"{_currentSvgIndex + 1}/{svgSnapshots.Count}";
        }
        private void ImageContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            workspaceDragged = WorkspaceImage.CaptureMouse();
            PointMousePressed = e.GetPosition(WorkspaceImage);

            if (_currentSvg == null || Tabs.IsEnabled == false)
                return;

            
            if (elPicker.ChoosenElement != null && Tabs.SelectedIndex == 0)
            {
                var widthRatio = _currentSvg.Width / _scaleTransform.X
              / Math.Max(ImageContainer.ActualWidth, ImageContainer.ActualHeight);
                var heightRatio = _currentSvg.Height / _scaleTransform.Y
                    / Math.Max(ImageContainer.ActualWidth, ImageContainer.ActualHeight);

                //_elipseSvgAngle.Margin = new Thickness(
                //    _shearTransform.X / widthRatio - 2,
                //    _shearTransform.Y / heightRatio - 2, 0, 0);

                var schemeX = PointMousePressed.X - _shearTransform.X / widthRatio;
                var schemeY = PointMousePressed.Y - _shearTransform.Y / heightRatio;

                var cellX = (schemeX / (100 / widthRatio));
                var cellY = (schemeY / (100 / heightRatio));

                CellCoords.Text = $"X:{cellX},Y:{cellY}";

                if (cellX < 0 || cellX > _inputScheme.Graph.Width ||
                    cellY < 0 || cellY > _inputScheme.Graph.Height)
                    return;

                var cell = _inputScheme.Graph.GetCell((uint)cellY, (uint)cellX);
                if (cell.Element == null)
                {
                    cell.Element = (Element)elPicker.ChoosenElement.Clone();
                }
                else
                    cell.Element = null;
                //_inputScheme = _inputScheme;
                
                svgSnapshots.Clear();
                _currentSvg = SchToSvg(_inputScheme);
                _currentSvgIndex = 0;
                svgSnapshots.Add(_currentSvg);
                RenderCurrentSvg();
            }
            else if (Tabs.SelectedIndex == 1)
            {
                var cell = lvCells.SelectedItem as Cell;
                var wires = lvWires.SelectedItem as WireItem;
                if (cell == null || wires == null)
                    return;

                var widthRatio = _currentSvg.Width / _scaleTransform.X
              / Math.Max(ImageContainer.ActualWidth, ImageContainer.ActualHeight);
                var heightRatio = _currentSvg.Height / _scaleTransform.Y
                    / Math.Max(ImageContainer.ActualWidth, ImageContainer.ActualHeight);

                //_elipseSvgAngle.Margin = new Thickness(
                //    _shearTransform.X / widthRatio - 2,
                //    _shearTransform.Y / heightRatio - 2, 0, 0);

                var schemeX = PointMousePressed.X - _shearTransform.X / widthRatio;
                var schemeY = PointMousePressed.Y - _shearTransform.Y / heightRatio;

                var cellX = (schemeX / (100 / widthRatio));
                var cellY = (schemeY / (100 / heightRatio));

                CellCoords.Text = $"X:{cellX},Y:{cellY}";

                if (cellX < 0 || cellX > _inputScheme.Graph.Width ||
                    cellY < 0 || cellY > _inputScheme.Graph.Height)
                    return;

                var newCell = _inputScheme.Graph.GetCell((uint)cellY, (uint)cellX);
                var index = (lvWires.SelectedItem as WireItem).cells.IndexOf(cell);
                (lvWires.SelectedItem as WireItem).cells[index] = newCell;
                lvCells.SelectedIndex = index;
                RenderCellWireStuff();
            }


        }
        private void ImageContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            workspaceDragged = false;
            WorkspaceImage.ReleaseMouseCapture();
        }
        private void ImageContainer_MouseMove(object sender, MouseEventArgs e)
        {
            Point PointMouseMoved = e.GetPosition(ImageContainer);

            if (!workspaceDragged) return;
            if (e.LeftButton == MouseButtonState.Released) return;

            var deltaX = 2 * (float)(PointMouseMoved.X - PointMousePressed.X);
            var deltaY = 2 * (float)(PointMouseMoved.Y - PointMousePressed.Y);

            _shearTransform.X += deltaX;
            _shearTransform.Y += deltaY;

            PointMousePressed = PointMouseMoved;
            RenderCurrentSvg();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputScheme = Examples.GetExample10x10();
        }
        private void StartTraceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (lvWires.Items.Count == 0)
                return;

            InputScheme.ClearStuff();
            InputScheme.Wires.Clear();
            foreach (var wire in WireItems)
                InputScheme.Wires.Add(wire.cells.ToList());

            TraceCurrentSch();
            InputScheme.Wires.Clear();
            UpdateWireItems();

            FirstSnapBtn.IsEnabled = true;
            PrevSnapBtn.IsEnabled = true;
            NextSnapBtn.IsEnabled = true;
            LastSnapBtn.IsEnabled = true;

            StartTraceBtn.IsEnabled = false;
            StopTraceBtn.IsEnabled = true;
            //BackTraceBtn.IsEnabled = true;

            StartTraceImg.IsEnabled = false;
            StopTraceImg.IsEnabled = true;
            //BackTraceImg.IsEnabled = true;            
            Tabs.IsEnabled = false;



            RenderCurrentSvg();
        }
        private void StopTraceBtn_Click(object sender, RoutedEventArgs e)
        {
            InputScheme = schSnapshots.Last();
            InputScheme.Wires.Clear();

            FirstSnapBtn.IsEnabled = false;
            PrevSnapBtn.IsEnabled = false;
            NextSnapBtn.IsEnabled = false;
            LastSnapBtn.IsEnabled = false;

            StartTraceBtn.IsEnabled = true;
            StopTraceBtn.IsEnabled = false;
            //BackTraceBtn.IsEnabled = false;

            StartTraceImg.IsEnabled = true;
            StopTraceImg.IsEnabled = false;
            //BackTraceImg.IsEnabled = false;

            Tabs.IsEnabled = true;
        }
        private void BackTraceBtn_Click(object sender, RoutedEventArgs e)
        {
            InputScheme = _inputScheme;

            FirstSnapBtn.IsEnabled = false;
            PrevSnapBtn.IsEnabled = false;
            NextSnapBtn.IsEnabled = false;
            LastSnapBtn.IsEnabled = false;

            StartTraceBtn.IsEnabled = true;
            StopTraceBtn.IsEnabled = false;
            //BackTraceBtn.IsEnabled = false;

            StartTraceImg.IsEnabled = true;
            StopTraceImg.IsEnabled = false;
            //BackTraceImg.IsEnabled = false;
        }

        public class WireItem
        {
            public string Label { get; set; }
            public ObservableCollection<Cell> cells { get; set; }
        }

        private void lvWires_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvWires.SelectedIndex < 0)
                return;
            RenderCellWireStuff();
            lvCells.ItemsSource = (lvWires.SelectedItem as WireItem)?.cells;
        }
        public void RenderCellWireStuff()
        {
            InputScheme.ClearStuff();
            foreach (var cellBehind in (lvWires.SelectedItem as WireItem)?.cells)
                cellBehind.StuffElement = new ColorStuffElement() { Color = System.Drawing.Color.FromArgb(102, System.Drawing.Color.Aqua) };

            var cell = lvCells.SelectedItem as Cell;
            if (cell != null)
                cell.StuffElement = new ColorStuffElement() { Color = System.Drawing.Color.FromArgb(162, System.Drawing.Color.LightGreen) };
            svgSnapshots[0] = SchToSvg(InputScheme);
            RenderCurrentSvg();
        }
        private void lvCells_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCells.SelectedIndex >= 0)
                RenderCellWireStuff();
        }

        private int PrevTabIndex;
        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InputScheme == null || Tabs.SelectedIndex == PrevTabIndex)
                return;
            //if (Tabs.SelectedIndex == 1)
            //    elPicker.ChoosenElement = null;

            PrevTabIndex = Tabs.SelectedIndex;
            InputScheme.ClearStuff();
            svgSnapshots[0] = SchToSvg(InputScheme);
            RenderCurrentSvg();
        }
        private void btnAddCellToWire_Click(object sender, RoutedEventArgs e)
        {
            (lvWires.SelectedItem as WireItem)?.cells.Add(InputScheme.Graph.GetCell(0, 0));
            RenderCellWireStuff();
            UpdateLabelsWireItems();
        }
        private void btnRemoveCellFromWire_Click(object sender, RoutedEventArgs e)
        {
            (lvWires.SelectedItem as WireItem)?.cells.Remove(lvCells.SelectedItem as Cell);
            RenderCellWireStuff();
            UpdateLabelsWireItems();
        }
        private void btnAddWire_Click(object sender, RoutedEventArgs e)
        {
            WireItems.Add(new()
            {
                cells = new()
                {
                    InputScheme.Graph.GetCell(0, 0),
                    InputScheme.Graph.GetCell(0, 0)
                }
            });
            UpdateLabelsWireItems();
        }
        private void btnRemoveWire_Click(object sender, RoutedEventArgs e)
        {
            var wire = lvWires.SelectedItem as WireItem;
            if (wire == null)
                return;

            WireItems.Remove(wire);
            UpdateLabelsWireItems();
        }
    }
}
