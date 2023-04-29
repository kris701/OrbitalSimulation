using OrbitalSimulation.Helpers;
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
    /// Interaction logic for ExplosionControl.xaml
    /// </summary>
    public partial class ExplosionControl : UserControl
    {
        int _lifeTimeTicks = 0;
        int _targetLifeTimeTicks = 0;
        int _minLifeTimeTicks = 250;
        int _maxLifeTimeTicks = 1000;

        Random _rnd = new Random();
        Point _location = new Point();
        double _size = 0;
        int _rotation = 0;

        public ExplosionControl(HashSet<OrbitalBody> collidedBodies)
        {
            InitializeComponent();

            
            _targetLifeTimeTicks = _rnd.Next(_minLifeTimeTicks, _maxLifeTimeTicks);

            double totalMass = 0;
            foreach (var obj in collidedBodies)
                totalMass += obj.KgMass;

            Point newLocation = new Point();
            foreach (var obj in collidedBodies)
            {
                newLocation.X += obj.Location.X * obj.KgMass;
                newLocation.Y += obj.Location.Y * obj.KgMass;
            }
            _location = new Point(
                newLocation.X / totalMass,
                newLocation.Y / totalMass);

            double combinedArea = 0;
            foreach (var obj in collidedBodies)
                combinedArea += CircleHelper.GetAreaOfRadius(obj.Radius);
            _size = CircleHelper.GetRadiusFromArea(combinedArea);
        }

        public bool Refresh(Canvas source, double scale, Point offset)
        {
            double sizeScale = 1 - ((double)_lifeTimeTicks / (double)_targetLifeTimeTicks);

            this.Margin = new Thickness(((offset.X + _location.X) * scale) - (this.Width / 2), (source.ActualHeight - (offset.Y + _location.Y) * scale) - (this.Height / 2), 0, 0);
            MainCanvas.Width = _size * 2 * scale * sizeScale;
            MainCanvas.Height = _size * 2 * scale * sizeScale;
            MainCanvas.RenderTransform = new RotateTransform(_rotation, _size * scale * sizeScale, _size * scale * sizeScale);

            _lifeTimeTicks++;
            if (_lifeTimeTicks > _targetLifeTimeTicks)
                return true;

            if (_lifeTimeTicks % 10 == 0)
                _rotation = _rnd.Next(0, 360);

            return false;
        }
    }
}
