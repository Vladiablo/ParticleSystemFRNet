using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using FastReport;
using FastReport.Utils;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;

namespace ParticleSystemFRNet
{
    public class ParticleSystem : ReportComponentBase
    {
        #region Fields
        private int seed;
        private uint particlesCount;
        private uint minParticleWidth;
        private uint minParticleHeight;
        private uint maxParticleWidth;
        private uint maxParticleHeight;
        private bool keepParticleAspectRatio;
        private float minOpacity;
        private float maxOpacity;
        private bool adjustableOpacity;
        private string dataColumn;
        private Image image;   

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets seed for randomizer.
        /// </summary>
        [Category("Data")]
        public int Seed
        {
            get => this.seed;
            set => this.seed = value;
        }

        /// <summary>
        /// Gets or sets particles count.
        /// </summary>
        [Category("Data")]
        public uint ParticlesCount
        {
            get => this.particlesCount;
            set => this.particlesCount = value;
        }

        /// <summary>
        /// Gets or sets minimal particle width.
        /// </summary>
        [Category("Data")]
        public uint MinParticleWidth
        {
            get => this.minParticleWidth;
            set
            {
                if (value == 0) value = 1;
                if (value > this.maxParticleWidth) value = this.maxParticleWidth;
                if (this.keepParticleAspectRatio) this.minParticleHeight = (uint)((float)this.minParticleHeight * ((float)value / this.minParticleWidth));
                this.minParticleWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets minimal particle height.
        /// </summary>
        [Category("Data")]
        public uint MinParticleHeight
        {
            get => this.minParticleHeight;
            set
            {
                if (value == 0) value = 1;
                if (value > this.maxParticleHeight) value = this.maxParticleHeight;
                if (this.keepParticleAspectRatio) this.minParticleWidth = (uint)((float)this.minParticleWidth * ((float)value / this.minParticleHeight));
                this.minParticleHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets maximal particle width.
        /// </summary>
        [Category("Data")]
        public uint MaxParticleWidth
        {
            get => this.maxParticleWidth;
            set
            {
                if (value == 0) value = 1;
                if (value < this.minParticleWidth) value = this.minParticleWidth;
                if (this.keepParticleAspectRatio) this.maxParticleHeight = (uint)((float)this.maxParticleHeight * ((float)value / this.maxParticleWidth));
                this.maxParticleWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets maximal particle height.
        /// </summary>
        [Category("Data")]
        public uint MaxParticleHeight
        {
            get => this.maxParticleHeight;
            set
            {
                if (value == 0) value = 1;
                if (value < this.minParticleWidth) value = this.minParticleWidth;
                if (this.keepParticleAspectRatio) this.maxParticleWidth = (uint)((float)this.maxParticleWidth * ((float)value / this.maxParticleHeight));
                this.maxParticleHeight = value;  
            }
        }

        /// <summary>
        /// Gets or sets value indicating that particle must keep aspect ratio.
        /// </summary>
        [Category("Data")]
        public bool KeepParticleAspectRatio
        {
            get => this.keepParticleAspectRatio;
            set => this.keepParticleAspectRatio = value;
        }

        /// <summary>
        /// Gets or sets minimal opacity of particle.
        /// </summary>
        [Category("Data")]
        public float MinOpacity
        {
            get => this.minOpacity;
            set
            {
                if (value > 1.0f) value = 1.0f;
                else if (value < 0.0f) value = 0.0f;
                if (value > this.maxOpacity) value = this.maxOpacity;
                this.minOpacity = value;
            }
        }

        /// <summary>
        /// Gets or sets maximal opacity of particle.
        /// </summary>
        [Category("Data")]
        public float MaxOpacity
        {
            get => this.maxOpacity;
            set
            {
                if (value > 1.0f)
                    value = 1.0f;
                else if (value < 0.0f)
                    value = 0.0f;
                if (value < this.minOpacity) value = this.minOpacity;
                this.maxOpacity = value;
            }
        }

        /// <summary>
        /// Gets or sets value that allows to change particles opacity. Warning! Opacity changing is slow.
        /// </summary>
        [Category("Data")]
        public bool AdjustableOpacity
        {
            get => this.adjustableOpacity;
            set => this.adjustableOpacity = value;
        }

        /// <summary>
        /// Gets or sets the data column name to get particle image from.
        /// </summary>
        [Category("Data")]
        [Editor("FastReport.TypeEditors.DataColumnEditor, FastReport", typeof(UITypeEditor))]
        public string DataColumn
        {
            get { return dataColumn; }
            set { dataColumn = value; }
        }

        /// <summary>
        /// Gets or sets particle image.
        /// </summary>
        [Category("Data")]
        public Image Image
        {
            get => this.image;
            set => image = value;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Changes opacity of the image.
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="opacity">Opacity</param>
        /// <returns>Image with selected opacity</returns>
        /// <remarks>Caution! Slow!</remarks>
        private Image SetImageOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                ImageAttributes attributes = new ImageAttributes();

                matrix.Matrix33 = opacity;
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);

                attributes.Dispose();
                gfx.Dispose();
            }
            return bmp;
        }

        #endregion

        #region Protected Methods
        ///<inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Image.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods
        ///<inheritdoc/>
        public override void Assign(Base source)
        {
            base.Assign(source);

            ParticleSystem src = source as ParticleSystem;
            if (src != null)
            {
                Seed = src.Seed;
                ParticlesCount = src.particlesCount;
                MinParticleWidth = src.MinParticleWidth;
                MinParticleHeight = src.MinParticleHeight;
                MaxParticleWidth = src.MaxParticleWidth;
                MaxParticleHeight = src.MaxParticleHeight;
                KeepParticleAspectRatio = src.KeepParticleAspectRatio;
                MinOpacity = src.MinOpacity;
                MaxOpacity = src.MaxOpacity;
                AdjustableOpacity = src.AdjustableOpacity;
                DataColumn = src.DataColumn;
                Image = src.Image;
            }
        }

        ///<inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            base.Draw(e);

            Graphics g = e.Graphics;
            if (Image == null)
                Image = Properties.Resources.ParticleSystemIcon;

            float drawLeft = AbsLeft * e.ScaleX;
            float drawTop = AbsTop * e.ScaleY;
            float drawWidth = Width * e.ScaleX;
            float drawHeight = Height * e.ScaleY;

            RectangleF drawRect = new RectangleF(drawLeft, drawTop, drawWidth, drawHeight);

            GraphicsState state = g.Save();
            try
            {
                g.SetClip(drawRect);
                Report report = Report;
                if (report != null && report.SmoothGraphics)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                }

                Random random = new Random(seed);
                int width = 0;
                int height = 0;
                float opacity = 1.0f;

                for (int i = 0; i < particlesCount; i++)
                {
                    if (this.keepParticleAspectRatio)
                    {
                        width = random.Next((int)this.minParticleWidth, (int)this.maxParticleWidth + 1);
                        height = (int)((float)this.maxParticleHeight / ((float)maxParticleWidth / width));
                    }
                    if(this.adjustableOpacity)
                    {
                        opacity = (float)random.NextDouble();
                        if (opacity < this.minOpacity) opacity = this.minOpacity;
                        else if (opacity > this.maxOpacity) opacity = this.maxOpacity;
                    }
                    g.DrawImage(
                        (this.adjustableOpacity) ? (SetImageOpacity(image, opacity)) : (this.image),
                        drawLeft + (float)random.NextDouble() * drawWidth, drawTop + (float)random.NextDouble() * drawHeight,
                        ((this.keepParticleAspectRatio) ? (width) : (random.Next((int)this.minParticleWidth, (int)this.maxParticleWidth + 1))) * e.ScaleX,
                        ((this.keepParticleAspectRatio) ? (height) : (random.Next((int)this.minParticleHeight, (int)this.maxParticleHeight + 1))) * e.ScaleY);
                }

            }
            finally
            {
                g.Restore(state);
            }

            Border.Draw(e, new RectangleF(AbsLeft, AbsTop, Width, Height));
            DrawMarkers(e);
            DrawDesign(e);
        }

        ///<inheritdoc/>
        public override void Serialize(FRWriter writer)
        {
            ParticleSystem c = writer.DiffObject as ParticleSystem;
            base.Serialize(writer);

            if (Seed != c.Seed)
                writer.WriteInt("Seed", Seed);
            if (ParticlesCount != c.ParticlesCount)
                writer.WriteInt("ParticlesCount", (int)ParticlesCount);
            if (MinParticleWidth != c.MinParticleWidth)
                writer.WriteInt("MinParticleWidth", (int)MinParticleWidth);
            if (MinParticleHeight != c.MinParticleHeight)
                writer.WriteInt("MinParticleHeight", (int)MinParticleHeight);
            if (MaxParticleWidth != c.MaxParticleWidth)
                writer.WriteInt("MaxParticleWidth", (int)MaxParticleWidth);
            if (MaxParticleHeight != c.MaxParticleHeight)
                writer.WriteInt("MaxParticleHeight", (int)MaxParticleHeight);
            if (KeepParticleAspectRatio != c.KeepParticleAspectRatio)
                writer.WriteBool("KeepParticleAspectRatio", KeepParticleAspectRatio);
            if (MinOpacity != c.MinOpacity)
                writer.WriteFloat("MinOpacity", MinOpacity);
            if (MaxOpacity != c.MaxOpacity)
                writer.WriteFloat("MaxOpacity", MaxOpacity);
            if (AdjustableOpacity != c.AdjustableOpacity)
                writer.WriteBool("AdjustableOpacity", AdjustableOpacity);
            if (DataColumn != c.DataColumn)
                writer.WriteStr("DataColumn", DataColumn);
            if (Image != c.Image)
                writer.WriteValue("Image", Image);
        }

        ///<inheritdoc/>
        public override void Deserialize(FRReader reader)
        {
            base.Deserialize(reader);
        }

        ///<inheritdoc/>
        public override void RestoreState()
        {
            base.RestoreState();
        }

        ///<inheritdoc/>
        public override void SaveState()
        {
            base.SaveState();
        }

        ///<inheritdoc/>
        public override void OnBeforePrint(EventArgs e)
        {
            base.OnBeforePrint(e);
        }

        ///<inheritdoc/>
        public override void GetData()
        {
            base.GetData();

            object data = Report.GetColumnValueNullable(DataColumn);
            if (!String.IsNullOrEmpty(DataColumn))
            {
                Image = null;

                if (data is Image)
                {
                    Image = data as Image;
                }
            }
        }

        /// <inheritdoc/>
        public override string[] GetExpressions()
        {
            List<string> expressions = new List<string>();
            expressions.AddRange(base.GetExpressions());
            if (!String.IsNullOrEmpty(DataColumn))
                expressions.Add(DataColumn);
            return expressions.ToArray();
        }

        /// <summary>
        /// Generates seed from current system time.
        /// </summary>
        public void GenerateSeedFromTime()
        {
            Seed = (int)DateTime.Now.ToBinary();
        }

        #endregion

        #region Report Engine
        ///<inheritdoc/>
        public override void InitializeComponent()
        {
            base.InitializeComponent();
        }

        ///<inheritdoc/>
        public override void FinalizeComponent()
        {
            base.FinalizeComponent();
        }

        #endregion

        /// <summary>
        /// Initializes ParticleSystem object with default settings.
        /// </summary>
        public ParticleSystem()
        {
            this.seed = (int)DateTime.Now.ToBinary();
            this.particlesCount = 50;
            this.minParticleWidth = 16;
            this.minParticleHeight = 16;
            this.maxParticleWidth = 32;
            this.maxParticleHeight = 32;
            this.keepParticleAspectRatio = true;
            this.minOpacity = 0.0f;
            this.maxOpacity = 1.0f;
            this.adjustableOpacity = false;
            this.dataColumn = "";
            this.image = Properties.Resources.ParticleSystemIcon;
        }
    }
}
