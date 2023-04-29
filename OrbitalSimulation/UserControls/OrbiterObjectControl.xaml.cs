using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
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

namespace OrbitalSimulation.UserControls
{
    /// <summary>
    /// Interaction logic for OribterObjectControl.xaml
    /// </summary>
    public partial class OrbiterObjectControl : UserControl
    {
        public OrbitalBody Item { get; set; }
        private List<Ellipse> Traces { get; set; }
        private int _maxTraces = 100;

        public OrbiterObjectControl(OrbitalBody item)
        {
            InitializeComponent();
            Item = item;
            Traces = new List<Ellipse>();

            if (item.Image != "")
            {
                VisualEllipse.Fill = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Resources/{item.Image}")),
                    Stretch = Stretch.Fill
                };
            }
        }

        public void Refresh(Canvas source, double scale, Point offset)
        {
            this.Margin = new Thickness(((offset.X + Item.Location.X) * scale) - (this.Width / 2), (source.ActualHeight - (offset.Y + Item.Location.Y) * scale) - (this.Height / 2), 0, 0);

            if (!Item.IsStationary)
                AddTrace(source, scale, offset);

            VisualEllipse.Width = Item.Radius * 2 * scale;
            VisualEllipse.Height = Item.Radius * 2 * scale;
            VelocityLabel.Margin = new Thickness(0, -(Item.Radius * 2 * scale) - VelocityLabel.ActualHeight, 0,0);

            if (Item.HasAtmosphere)
            {
                VisualAtmEllipse.Width = Item.AtmTopLevel * 2 * scale;
                VisualAtmEllipse.Height = Item.AtmTopLevel * 2 * scale;
            }

            if (Item.Image == "")
            {
                if (Item.IsStationary)
                    VisualEllipse.Fill = Brushes.Orange;
                else
                    VisualEllipse.Fill = Brushes.Blue;
            }
            else
            {
                if (Item.GetVelocity() != 0)
                {
                    var angle = -Math.Atan(Item.VelocityVector.Y / Item.VelocityVector.X) * (180 / Math.PI);
                    VisualEllipse.RenderTransform = new RotateTransform(angle, Item.Radius * scale, Item.Radius * scale);
                }
                else
                    VisualEllipse.RenderTransform = new RotateTransform(0);
            }

            if (!Item.IsStationary)
            {
                VelocityLabel.Visibility = Visibility.Visible;
                VelocityLine.Visibility = Visibility.Visible;

                VelocityLabel.Content = $"({Math.Round(Item.VelocityVector.X, 2)},{Math.Round(Item.VelocityVector.Y, 2)})";

                VelocityLine.X1 = this.Width / 2;
                VelocityLine.Y1 = this.Height / 2;
                VelocityLine.X2 = VelocityLine.X1 + (Item.VelocityVector.X * 10 * scale);
                VelocityLine.Y2 = VelocityLine.Y1 - (Item.VelocityVector.Y * 10 * scale);
            }
            else
            {
                VelocityLabel.Visibility = Visibility.Hidden;
                VelocityLine.Visibility = Visibility.Hidden;
            }
        }

        private void AddTrace(Canvas source, double scale, Point offset)
        {
            if (Traces.Count > _maxTraces)
            {
                source.Children.Remove(Traces[0]);
                Traces.RemoveAt(0);
            }

            var ellipse = new Ellipse()
            {
                Width = 2,
                Height = 2, 
                Stroke = Brushes.White,
                IsHitTestVisible = false,
                Margin = new Thickness(((offset.X + Item.Location.X) * scale) - 1, (source.ActualHeight - (offset.Y + Item.Location.Y) * scale) - 1, 0, 0)
            };

            Traces.Add(ellipse);    
            source.Children.Add(ellipse);
        }

        public void Cleanup(Canvas source)
        {
            foreach (var trace in Traces)
                source.Children.Remove(trace);
            Traces.Clear();
        }
    }
}
