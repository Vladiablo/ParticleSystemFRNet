using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleSystemFRNet
{
    public class Collider
    {
        public float x0;
        public float y0;
        public float x1;
        public float y1;

        public Collider(float x0, float y0, float x1, float y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }

        public Collider Expand(float xa, float ya)
        {
            float _x0 = this.x0;
            float _y0 = this.y0;
            float _x1 = this.x1;
            float _y1 = this.y1;
            if (xa < 0.0F)
            {
                _x0 += xa;
            }

            if (xa > 0.0F)
            {
                _x1 += xa;
            }

            if (ya < 0.0F)
            {
                _y0 += ya;
            }

            if (ya > 0.0F)
            {
                _y1 += ya;
            }

            return new Collider(_x0, _y0, _x1, _y1);
        }

        public Collider Grow(float xa, float ya)
        {
            float _x0 = this.x0 - xa;
            float _y0 = this.y0 - ya;
            float _x1 = this.x1 + xa;
            float _y1 = this.y1 + ya;
            return new Collider(_x0, _y0, _x1, _y1);
        }

        public float ClipXCollide(float xa)
        {
            if (this.x1 > xa)
            {
                xa = this.x1 - xa;
            }       
            else if (this.x0 < xa)
            {
                xa = this.x0 - xa;
            }

            return xa;
        }

        public float ClipYCollide(float ya)
        {
            if (this.y1 > ya)
            {
                ya = this.y1 - ya;
            }
            else if (this.y0 < ya)
            {
                ya = this.y0 - ya;
            }

            return ya;
        }

        public bool Intersects(Collider c)
        {
            if (c.x1 > this.x0 && c.x0 < this.x1)
            {
                return c.y1 > this.y0 && c.y0 < this.y1;
            }
            else
            {
                return false;
            }
        }

        public void Move(float xa, float ya)
        {
            this.x0 += xa;
            this.y0 += ya;
            this.x1 += xa;
            this.y1 += ya;
        }
    }
}
