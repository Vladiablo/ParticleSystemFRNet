using FastReport;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ParticleSystemFRNet
{
    public class Particle
    {
        private float x;
        private float y;

        private float xv;
        private float yv;

        private float xa;
        private float ya;

        private float width;
        private float height;

        private Collider collider;
        private ParticleEngine particleEngine;

        private Image texture;

        public float X
        {
            get => this.x;
            set => this.SetPos(value, this.y);
        }

        public float Y
        {
            get => this.y;
            set => this.SetPos(this.x, value);
        }

        public float XVelocity
        {
            get => this.xv;
            set => this.xv = value;
        }

        public float YVelocity
        {
            get => this.yv;
            set => this.yv = value;
        }

        public float XAcceleration
        {
            get => this.xa;
            set => this.xa = value;
        }

        public float YAcceleration
        {
            get => this.ya;
            set => this.ya = value;
        }

        public Particle(ParticleEngine engine, float x, float y, float xa, float ya, float width, float height, Image texture)
        {
            this.particleEngine = engine;
            this.texture = texture;
            this.SetPos(x, y);

            this.xa = xa;
            this.ya = ya;

            this.xv = 0.0f;
            this.yv = 0.0f;

            this.width = width;
            this.height = height;
        }

        //public Particle()
        //{
        //    this.texture = Properties.Resources.ParticleSystemIcon;
        //    this.SetPos(x, y);

        //    this.xa = 1.0f;
        //    this.ya = 1.0f;

        //    this.xv = 0.0f;
        //    this.yv = 0.0f;
        //}

        public void Update()
        {
            this.xv += this.xa;
            this.yv += this.ya;

            this.Move(this.xv, this.yv);

            float x;
            float y;
            //if (this.particleEngine.ShapeObject == null)
            //{
            //    switch(particleEngine.ShapeKind)
            //    {
            //        case ShapeKind.Rectangle:
            //            {
            //                x = this.collider.ClipXCollide(0);
            //                if (x != 0) this.Move(x);
            //            }
            //    }
            //}
            //else
            //{

            //}
        }

        public void Draw(FRPaintEventArgs e, float offsetX, float offsetY)
        {
            Graphics g = e.Graphics;

            g.DrawImage(this.texture, new RectangleF((offsetX + this.collider.x0) * e.ScaleX, (offsetY + this.collider.y0)* e.ScaleY, this.width * e.ScaleX, this.height * e.ScaleY));
        }

        private void SetPos(float x, float y)
        {
            this.x = x;
            this.y = y;

            float w = this.width / 2.0F;
            float h = this.height / 2.0F;
            this.collider = new Collider(x - w, y - h, x + w, y + h);
        }

        public void Move(float xa, float ya)
        {
            float xaOrg = xa;
            float yaOrg = ya;
            //List<AABB> aABBs = this.level.GetCubes(this.bb.Expand(xa, ya, za));

            //int i;
            //for (i = 0; i < aABBs.Count; ++i)
            //{
            //    ya = aABBs[i].ClipYCollide(this.bb, ya);
            //}

            this.collider.Move(0.0F, ya);

            //for (i = 0; i < aABBs.Count; ++i)
            //{
            //    xa = aABBs[i].ClipXCollide(this.bb, xa);
            //}

            this.collider.Move(xa, 0.0F);

            //for (i = 0; i < aABBs.Count; ++i)
            //{
            //    za = aABBs[i].ClipZCollide(this.bb, za);
            //}

            //this.bb.Move(0.0F, 0.0F, za);
            //this.onGround = yaOrg != ya && yaOrg < 0.0F;
            if (xaOrg != xa)
            {
                this.xv = 0.0F;
            }

            if (yaOrg != ya)
            {
                this.yv = 0.0F;
            }

            this.x = (this.collider.x0 + this.collider.x1) / 2.0F;
            this.y = (this.collider.y0 + this.collider.y1) / 2.0F;
        }

        public void Stop()
        {
            this.xa = 0.0f;
            this.ya = 0.0f;

            this.xv = 0.0f;
            this.yv = 0.0f;
        }
    }
}
