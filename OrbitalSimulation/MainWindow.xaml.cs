using OrbitalSimulation.Engines;
using OrbitalSimulation.Models;
using OrbitalSimulation.Presets;
using OrbitalSimulation.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static System.Formats.Asn1.AsnWriter;

namespace OrbitalSimulation
{
    public partial class MainWindow : Window
    {
        private bool _isLoaded = false;

        private double _refreshRate = 16.66666;

        private double _speedValue = 1;

        private double _scale = 1;
        private double _minScale = 0.00000001;
        private double _maxScale = 100;

        private Point _offset = new Point();

        private bool _run = false;
        private IPhysicsEngine _engine = new BasicEngine();
        private List<OrbiterObjectControl> _visualObjects = new List<OrbiterObjectControl>();
        private List<ExplosionControl> _visualExplosions = new List<ExplosionControl>();

        private bool _isOffsetting = false;
        private Point _startOffsetPoint = new Point();
        private Point _currentOffsetPoint = new Point();
        private bool _isDrawing = false;
        private Point _startDrawPoint = new Point();
        private Line _line = new Line();
        private Ellipse _ghostDraw = new Ellipse();
        private Label _velocityLabel = new Label();
        private List<Line> _drawPrediction = new List<Line>();
        private BuilderOptions _targetOption = BuilderOptions.None;
        private OrbitalBody _newBody = new OrbitalBody();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _engine.CollisionOccured += AddExplosion;
            _engine.BodyDeleted += DeleteBody;
            _engine.BodyAdded += AddBody;
            _isLoaded = true;
        }

        private void SetupObjects()
        {
            _visualObjects = new List<OrbiterObjectControl>();
            foreach (var obj in _engine.Bodies)
                _visualObjects.Add(new OrbiterObjectControl(obj));

            MainCanvas.Children.Clear();
            foreach (var control in _visualObjects)
                MainCanvas.Children.Add(control);
            foreach (var control in _visualExplosions)
                MainCanvas.Children.Add(control);

            foreach (var control in _visualObjects)
                control.Refresh(MainCanvas, _scale, _offset);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            RestartButton.IsEnabled = false;

            SetupObjects();

            await RunSimulationTaskAsync();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            RestartButton.IsEnabled = true;
        }

        private async Task RunSimulationTaskAsync()
        {
            _run = true;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int frames = 0;
            while (_run)
            {
                _engine.Update(_speedValue);
                foreach (var control in _visualObjects)
                    control.Refresh(MainCanvas, _scale, _offset);
                foreach (var control in _visualExplosions)
                    control.Refresh(MainCanvas, _scale, _offset);

                await Task.Delay((int)_refreshRate);
                frames++;
                if (watch.ElapsedMilliseconds >= 1000)
                {
                    watch.Stop();
                    FramerateLabel.Content = $"FPS: {frames}";
                    watch.Restart();
                    frames = 0;
                }
            }
        }

        private void AddExplosion(HashSet<OrbitalBody> collidedBodies)
        {
            _visualExplosions.Add(new ExplosionControl(collidedBodies));
        }

        private void AddBody(OrbitalBody body)
        {
            var visualObject = new OrbiterObjectControl(body);
            MainCanvas.Children.Add(visualObject);
            _visualObjects.Add(visualObject);
        }

        private void DeleteBody(OrbitalBody body)
        {
            var visualObject = _visualObjects.Single(x => x.Item.ID == body.ID);
            visualObject.Cleanup(MainCanvas);
            MainCanvas.Children.Remove(visualObject);
            _visualObjects.Remove(visualObject);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _run = false;
        }

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

                var invScale = 1 / _scale;
                _newBody = PresetBuilder.GetPresetBodyFromID(_targetOption);
                _newBody.Location = new Point(((_startDrawPoint.X) * invScale) - _offset.X, ((MainCanvas.ActualHeight - _startDrawPoint.Y) * invScale) - _offset.Y);
                if (DrawStationary.IsChecked == true)
                    _newBody.IsStationary = true;
                else
                    _newBody.IsStationary = false;

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
                _ghostDraw = new Ellipse()
                {
                    Width = _newBody.Radius * 2 * _scale,
                    Height = _newBody.Radius * 2 * _scale,
                    Margin = new Thickness(
                        ((_offset.X + _newBody.Location.X) * _scale) - ((_newBody.Radius * 2 * _scale) / 2),
                        (MainCanvas.ActualHeight - (_offset.Y + _newBody.Location.Y) * _scale) - ((_newBody.Radius * 2 * _scale) / 2), 
                        0,0),
                    Opacity = 0.25,
                    Fill = Brushes.DarkGray
                };
                MainCanvas.Children.Add(_ghostDraw);

                _isDrawing = true;
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                var thisLocation = e.GetPosition(MainCanvas);
                _line.X2 = thisLocation.X;
                _line.Y2 = thisLocation.Y;

                var invScale = 1 / _scale;
                var multiplier = Math.Round(DragMultiplier.Value, 0);
                var newVelocity = new Point(((thisLocation.X - _startDrawPoint.X) / multiplier) * invScale, (-(thisLocation.Y - _startDrawPoint.Y) / multiplier) * invScale);

                foreach (var point in _drawPrediction)
                    MainCanvas.Children.Remove(point);
                _drawPrediction.Clear();

                _newBody.VelocityVector = newVelocity;

                var predictedPath = _engine.PredictPath(
                    _newBody,
                    100,
                    MainCanvas.ActualHeight * invScale * 2);

                var startPoint = predictedPath[0];
                foreach(var point in predictedPath.Skip(1))
                {
                    var line = new Line()
                    {
                        X1 = (_offset.X + startPoint.X) * _scale,
                        Y1 = MainCanvas.ActualHeight - (_offset.Y + startPoint.Y) * _scale,
                        X2 = (_offset.X + point.X) * _scale,
                        Y2 = MainCanvas.ActualHeight - (_offset.Y + point.Y) * _scale,
                        StrokeThickness = 2,
                        Stroke = Brushes.Green,
                        StrokeDashArray = new DoubleCollection() { 2 },
                        StrokeLineJoin = PenLineJoin.Round,
                        IsHitTestVisible = false,
                    };
                    startPoint = point;
                    _drawPrediction.Add(line);
                    MainCanvas.Children.Add(line);
                }

                _velocityLabel.Content = $"({Math.Round(newVelocity.X, 2)},{Math.Round(newVelocity.Y, 2)})";
            }
            else if (_isOffsetting)
            {
                var thisLocation = e.GetPosition(MainCanvas);
                var invScale = 1 / _scale;
                _offset = new Point(_currentOffsetPoint.X - (_startOffsetPoint.X - thisLocation.X) * invScale, _currentOffsetPoint.Y + (_startOffsetPoint.Y - thisLocation.Y) * invScale);
                OffsetLabel.Content = $"Offset: ({Math.Round(_offset.X, 2)},{Math.Round(_offset.Y, 2)})";
                SetupObjects();
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                MainCanvas.Children.Remove(_line);
                MainCanvas.Children.Remove(_velocityLabel);
                MainCanvas.Children.Remove(_ghostDraw);
                foreach (var point in _drawPrediction)
                    MainCanvas.Children.Remove(point);
                _drawPrediction.Clear();

                var thisLocation = e.GetPosition(MainCanvas);

                var invScale = 1 / _scale;
                var multiplier = Math.Round(DragMultiplier.Value, 0);
                var newVelocity = new Point(((thisLocation.X - _startDrawPoint.X) / multiplier) * invScale, (-(thisLocation.Y - _startDrawPoint.Y) / multiplier) * invScale);

                _newBody.VelocityVector = newVelocity;
                _engine.AddNewBody(_newBody);

                SetupObjects();
            } else if (_isOffsetting)
            {
                _isOffsetting = false;
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
            {
                _speedValue = Math.Round(Math.Pow(10, e.NewValue), 5);
                SpeedSliderLabel.Content = $"Speed: {_speedValue}x";
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.Bodies.Clear();
            _offset = new Point();
            _scale = 1;

            ScaleLabel.Content = $"Scale: {_scale}x";
            OffsetLabel.Content = $"Offset: ({Math.Round(_offset.X, 2)},{Math.Round(_offset.Y, 2)})";

            SetupObjects();
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var curInvScale = 1 / _scale;
            var startOffsetPoint = e.GetPosition(MainCanvas);
            startOffsetPoint.Y -= MainCanvas.ActualHeight;
            startOffsetPoint.X *= curInvScale;
            startOffsetPoint.Y *= curInvScale;
            bool isModified = (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down) || Keyboard.GetKeyStates(Key.RightShift) == KeyStates.Down;

            if (e.Delta > 0)
            {
                _scale += _scale / 10;
                if (isModified)
                    _scale += _scale / 10;
            }
            else
            {
                _scale -= _scale / 10;
                if (isModified)
                    _scale -= _scale / 10;
            }

            _scale = Math.Round(_scale, 15);
            if (_scale <= _minScale)
                _scale = _minScale;
            if (_scale >= _maxScale)
                _scale = _maxScale;

            var newInvScale = 1 / _scale;
            var newOffsetPoint = e.GetPosition(MainCanvas);
            newOffsetPoint.Y -= MainCanvas.ActualHeight;
            newOffsetPoint.X *= newInvScale;
            newOffsetPoint.Y *= newInvScale;

            _offset = new Point(_offset.X - (startOffsetPoint.X - newOffsetPoint.X), _offset.Y + (startOffsetPoint.Y - newOffsetPoint.Y));

            OffsetLabel.Content = $"Offset: ({Math.Round(_offset.X, 2)},{Math.Round(_offset.Y, 2)})";
            ScaleLabel.Content = $"Scale: {_scale}x";
            SetupObjects();
        }

        private void SetPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                if (button.Tag is BuilderOptions option)
                    _targetOption = option;
        }

        private void DragMultiplier_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
                DragMultiplierLabel.Content = $"Draw Multiplier: {Math.Round(e.NewValue, 0)}x";
        }
    }
}
