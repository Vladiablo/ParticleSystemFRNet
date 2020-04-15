using FastReport;
using FastReport.Utils;
using ParticleSystemFRNet;
using System;
using System.Windows.Forms;

namespace ParticleSystemFRNetDemo
{
    static class Program
    {
        /// <summary>
        /// Main program entry point.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RegisteredObjects.Add(typeof(ParticleSystem), "ReportPage", ParticleSystemFRNet.Properties.Resources.ParticleSystemIcon, "Particle System");

            using (Report report = new Report())
            {
                report.Design();
            }
        }
    }
}
