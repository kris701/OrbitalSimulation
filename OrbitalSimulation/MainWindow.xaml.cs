using OrbitalSimulation.Engines;
using OrbitalSimulation.Models;
using OrbitalSimulation.UserControls;
using System;
using System.Collections.Generic;
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

namespace OrbitalSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IPhysicsEngine engine = new BasicEngine();
            List<OrbiterObject> objects = new List<OrbiterObject>();

            var planet = new OrbiterObject(
                true,
                new Point(400, 200),
                new Point(0, 0),
                10000000000,
                25);
            objects.Add(planet);

            var orbiter1 = new OrbiterObject(
                false,
                new Point(400, 100),
                new Point(0, 0),
                10000,
                5);
            orbiter1.VelocityVector = new Point(engine.GetHorisontalOrbitalSpeed(planet, orbiter1),0);
            objects.Add(orbiter1);

            var orbiter2 = new OrbiterObject(
                false,
                new Point(400, 350),
                new Point(0, 0),
                10000,
                5);
            orbiter2.VelocityVector = new Point(-engine.GetHorisontalOrbitalSpeed(planet, orbiter2), 0);
            objects.Add(orbiter2);

            List<OrbiterObjectControl> controls = new List<OrbiterObjectControl>();
            foreach(var obj in objects)
                controls.Add(new OrbiterObjectControl(obj));
            foreach (var control in controls)
                MainCanvas.Children.Add(control);

            while (true)
            {
                engine.Update(objects);
                foreach (var control in controls)
                    control.Refresh(MainCanvas);

                await Task.Delay((int)SpeedSlider.Value);
            }
        }
    }
}
