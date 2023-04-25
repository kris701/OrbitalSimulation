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

        private double _scale = 1;
        private Point _offset = new Point();

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
                10000000000,
                25);
            _objects.Add(planet);

            SetupObjects();

            _isLoaded = true;

            //var orbiter1 = new OrbiterObject(
            //    false,
            //    new Point(400, 100),
            //    new Point(0, 0),
            //    10000,
            //    5);
            //orbiter1.VelocityVector = new Point(engine.GetHorisontalOrbitalSpeed(planet, orbiter1),0);
            //objects.Add(orbiter1);

            //var orbiter2 = new OrbiterObject(
            //    false,
            //    new Point(400, 240),
            //    new Point(0, 0),
            //    10000,
            //    5);
            //orbiter2.VelocityVector = new Point(engine.GetHorisontalOrbitalSpeed(planet, orbiter2), 0);
            //objects.Add(orbiter2);
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
                _engine.Update(_objects);
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
                _offset = new Point(_currentOffsetPoint.X - (_startOffsetPoint.X - thisLocation.X), _currentOffsetPoint.Y + (_startOffsetPoint.Y - thisLocation.Y));
                OffsetLabel.Content = $"Offset: ({_offset.X},{_offset.Y})";
                SetupObjects();
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                SpeedSliderLabel.Content = $"{Math.Round(e.NewValue,0)}";
        }

        private void DrawWeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                DrawWeightLabel.Content = $"{Math.Round(e.NewValue, 0)}";
        }

        private void DrawSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                DrawSizeLabel.Content = $"{Math.Round(e.NewValue, 0)}";
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            _objects.Clear();

            SetupObjects();
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _scale += ((double)e.Delta / 1000);
            _scale = Math.Round(_scale, 1);
            if (_scale <= 0)
                _scale = 0.1;
            ScaleLabel.Content = $"Scale: {_scale}x";
            SetupObjects();
        }
    }
}
