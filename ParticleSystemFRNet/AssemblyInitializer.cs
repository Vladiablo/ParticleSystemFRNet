using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using FastReport.Utils;

namespace ParticleSystemFRNet
{
    class AssemblyInitializer : AssemblyInitializerBase
    {
        public AssemblyInitializer()
        {
            RegisteredObjects.Add(typeof(ParticleSystem), "ReportPage", Properties.Resources.ParticleSystemIcon, "Particle System");
        }
    }
}
