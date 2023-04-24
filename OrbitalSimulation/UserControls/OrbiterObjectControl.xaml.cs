using OrbitalSimulation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private OrbiterObject Item { get; set; }
        private List<Ellipse> Traces { get; set; }
        private int _maxTraces = 50;

        public OrbiterObjectControl(OrbiterObject item)
        {
            InitializeComponent();
            Item = item;
            Traces = new List<Ellipse>();

            VisualEllipse.Width = Item.Radius * 2;
            VisualEllipse.Height = Item.Radius * 2;
        }

        public void Refresh(Canvas source)
        {
            this.Margin = new Thickness(Item.Location.X - this.Width / 2, source.ActualHeight - Item.Location.Y - this.Height / 2, 0, 0);

            AddTrace(source);

            if (Item.IsCollided)
                VisualEllipse.Fill = Brushes.Red;
            else
                VisualEllipse.Fill = Brushes.Blue;

            if (!Item.IsStationary && !Item.IsCollided)
            {
                VelocityLabel.Visibility = Visibility.Visible;
                VelocityLine.Visibility = Visibility.Visible;

                VelocityLabel.Content = $"({Math.Round(Item.VelocityVector.X, 2)},{Math.Round(Item.VelocityVector.Y, 2)})";

                var newDirection = GetDirectionalVector(
                    new Point(this.Width / 2, this.Height / 2),
                    new Point(this.Width / 2 + Item.VelocityVector.X, this.Height / 2 - Item.VelocityVector.Y));
                VelocityLine.X1 = this.Width / 2;
                VelocityLine.Y1 = this.Height / 2;
                VelocityLine.X2 = VelocityLine.X1 + (Item.VelocityVector.X * 10);
                VelocityLine.Y2 = VelocityLine.Y1 - (Item.VelocityVector.Y * 10);
            }
            else
            {
                VelocityLabel.Visibility = Visibility.Hidden;
                VelocityLine.Visibility = Visibility.Hidden;
            }
        }

        private Point GetDirectionalVector(Point origin, Point velocity)
        {
            double angle = Math.Atan2((origin.Y - velocity.Y), (origin.X - velocity.X));
            Point force = new Point();

            force.X = Math.Cos(angle) * 50;
            force.Y = Math.Sin(angle) * 50;

            return force;
        }

        private void AddTrace(Canvas source)
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
                Stroke = Brushes.Black,
                Margin = new Thickness(Item.Location.X - 1, source.ActualHeight - Item.Location.Y - 1, 0, 0)
            };

            Traces.Add(ellipse);
            source.Children.Add(ellipse);
        }
    }
}
