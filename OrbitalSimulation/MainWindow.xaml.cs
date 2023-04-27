using OrbitalSimulation.Engines;
using OrbitalSimulation.Models;
using OrbitalSimulation.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

namespace OrbitalSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isLoaded = false;

        private double _scale = 0.00005;
        private double _minScale = 0.0000000001;
        private double _maxScale = 0.0001;

        private Point _offset = new Point();

        private double _MaxWeight = 10000000;

        private bool _run = false;
        private IPhysicsEngine _engine = new BasicEngine();
        private List<OrbiterObjectControl> _visualObjects = new List<OrbiterObjectControl>();
        private List<OrbiterObject> _objects = new List<OrbiterObject>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var planet = new OrbiterObject(
                true,
                new Point(400, 200),
                new Point(0, 0),
                500000,
                25);
            _objects.Add(planet);

            SetupObjects();

            _isLoaded = true;
        }

        private void SetupObjects()
        {
            _visualObjects = new List<OrbiterObjectControl>();
            foreach (var obj in _objects)
                _visualObjects.Add(new OrbiterObjectControl(obj));

            MainCanvas.Children.Clear();
            foreach (var control in _visualObjects)
                MainCanvas.Children.Add(control);

            foreach (var control in _visualObjects)
                control.Refresh(MainCanvas, _scale, _offset);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            SetupObjects();

            _run = true;
            while (_run)
            {
                if (_engine.Update(_objects))
                    SetupObjects();
                else
                    foreach (var control in _visualObjects)
                        control.Refresh(MainCanvas, _scale, _offset);

                await Task.Delay((int)SpeedSlider.Value);
            }

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _run = false;
        }

        private bool _isOffsetting = false;
        private Point _startOffsetPoint = new Point();
        private Point _currentOffsetPoint = new Point();
        private bool _isDrawing = false;
        private Point _startDrawPoint = new Point();
        private Line _line = new Line();
        private Label _velocityLabel = new Label();
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed && _isDrawing)
            {
                _isDrawing = false;
                MainCanvas.Children.Remove(_line);
                MainCanvas.Children.Remove(_velocityLabel);
                return;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                var invScale = 1 / _scale;
                _isOffsetting = true;
                _startOffsetPoint = e.GetPosition(MainCanvas);
                _currentOffsetPoint = _offset;
            } else if (e.LeftButton == MouseButtonState.Pressed && !_isOffsetting)
            {
                _startDrawPoint = e.GetPosition(MainCanvas);

                _line = new Line()
                {
                    Stroke = Brushes.Red,
                    X1 = _startDrawPoint.X,
                    Y1 = _startDrawPoint.Y
                };
                MainCanvas.Children.Add(_line);
                _velocityLabel = new Label()
                {
                    Content = "(0,0)",
                    Margin = new Thickness(_startDrawPoint.X, _startDrawPoint.Y, 0, 0)
                };
                MainCanvas.Children.Add(_velocityLabel);

                _isDrawing = true;
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                MainCanvas.Children.Remove(_line);
                MainCanvas.Children.Remove(_velocityLabel);
                var thisLocation = e.GetPosition(MainCanvas);

                var newVelocity = new Point((thisLocation.X - _startDrawPoint.X) / 10, -(thisLocation.Y - _startDrawPoint.Y) / 10);
                bool isStationary = false;
                if (DrawStationary.IsChecked == true)
                    isStationary = true;

                var invScale = 1 / _scale;
                var newObject = new OrbiterObject(
                    isStationary,
                    new Point(((_startDrawPoint.X) * invScale) - _offset.X, ((MainCanvas.ActualHeight - _startDrawPoint.Y) * invScale) - _offset.Y),
                    newVelocity,
                    DrawWeight.Value,
                    DrawSize.Value);
                _objects.Add(newObject);

                SetupObjects();
            } else if (_isOffsetting)
            {
                _isOffsetting = false;
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                var thisLocation = e.GetPosition(MainCanvas);
                _line.X2 = thisLocation.X;
                _line.Y2 = thisLocation.Y;

                var newVelocity = new Point((thisLocation.X - _startDrawPoint.X) / 10, -(thisLocation.Y - _startDrawPoint.Y) / 10);

                _velocityLabel.Content = $"({newVelocity.X},{newVelocity.Y})";
            } else if (_isOffsetting)
            {
                var thisLocation = e.GetPosition(MainCanvas);
                var invScale = 1 / _scale;
                _offset = new Point(_currentOffsetPoint.X - (_startOffsetPoint.X - thisLocation.X), _currentOffsetPoint.Y + (_startOffsetPoint.Y - thisLocation.Y));
                _offset.X *= invScale;
                _offset.Y *= invScale;
                OffsetLabel.Content = $"Offset: ({_offset.X},{_offset.Y})";
                SetupObjects();
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                SpeedSliderLabel.Content = $"Refresh Rate: {Math.Round(e.NewValue, 0)}ms";
        }

        private void DrawWeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                DrawWeightLabel.Content = $"Weight: {Math.Round(e.NewValue, 0)}kg";
        }

        private void DrawSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                DrawSizeLabel.Content = $"Size: {Math.Round(e.NewValue, 0)}";
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            _objects.Clear();
            _offset = new Point();
            _scale = 0.00005;

            ScaleLabel.Content = $"Scale: {_scale}x";
            OffsetLabel.Content = $"Offset: ({_offset.X},{_offset.Y})";

            SetupObjects();
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _scale += ((double)e.Delta / 100000000);
            _scale = Math.Round(_scale, 9);
            if (_scale <= _minScale)
                _scale = _minScale;
            if (_scale >= _maxScale)
                _scale = _maxScale;
            ScaleLabel.Content = $"Scale: {_scale}x";
            ScaleSlider.Value = _scale;
            SetupObjects();
        }

        private void EarthPresetButton_Click(object sender, RoutedEventArgs e)
        {
            DrawWeight.Value = 5.972 * Math.Pow(10,24);
            DrawSize.Value = 6371000;
            UpdateAllControlLabels();
        }

        private void MoonPresetButton_Click(object sender, RoutedEventArgs e)
        {
            DrawWeight.Value = 7.34767309 * Math.Pow(10, 22);
            DrawSize.Value = 1737400;
            UpdateAllControlLabels();
        }

        private void UpdateAllControlLabels()
        {
            ScaleLabel.Content = $"Scale: {_scale}x";
            OffsetLabel.Content = $"Offset: ({_offset.X},{_offset.Y})";
            SpeedSliderLabel.Content = $"Refresh Rate: {Math.Round(SpeedSlider.Value, 0)}ms";
            DrawWeightLabel.Content = $"Weight: {Math.Round(DrawWeight.Value, 0)}kg";
            DrawSizeLabel.Content = $"Size: {Math.Round(DrawSize.Value, 0)}";
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
            {
                _scale = e.NewValue;
                ScaleLabel.Content = $"Scale: {_scale}x";
                SetupObjects();
            }
        }
    }
}
