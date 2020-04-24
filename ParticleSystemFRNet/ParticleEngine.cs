using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using FastReport;
using FastReport.Utils;

namespace ParticleSystemFRNet
{
    public class ParticleEngine : Component
    {
        private List<Particle> particles = new List<Particle>();
        private ShapeObject shapeObject;
        private ShapeKind shapeKind;
        public ParticleSystem particleSystem;

        public List<Particle> Particles
        {
            get => this.particles;
        }

        public ShapeObject ShapeObject
        {
            get => this.shapeObject;
            set => this.shapeObject = value;
        }

        public ShapeKind ShapeKind
        {
            get => this.shapeKind;
            set => this.shapeKind = value;
        }

        public void Add(Particle p)
        {
            this.particles.Add(p);
        }

        public void Update()
        {
            for (int i = 0; i < this.particles.Count; ++i)
            {
                this.particles[i].Update();
            }

        }

        public void Draw(FRPaintEventArgs e)
        {
            for (int i = 0; i < this.particles.Count; ++i)
            {
                this.particles[i].Draw(e, this.particleSystem.AbsLeft, this.particleSystem.AbsTop);
            }
        }

        public ParticleEngine(ParticleSystem particleSystem)
        {
            this.particleSystem = particleSystem;
            this.shapeObject = null;
            this.shapeKind = ShapeKind.Rectangle;
        }
    }
}
