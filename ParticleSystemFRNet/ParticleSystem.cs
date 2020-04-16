using FastReport;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ParticleSystemFRNet
{
    public enum AvoidClippingMethod
    {
        LimitCoordinates = 0,
        RandomizeCoordinates = 1
    }

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
        private bool avoidClipping;
        private AvoidClippingMethod avoidClippingMethod;
        private float minOpacity;
        private float maxOpacity;
        private bool adjustableOpacity;
        private string dataColumn;
        private Image image;
        private uint opacityImageCacheLength;
        private Image[] opacityImageCache;
        private bool opacityImageCacheReady = false;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets seed for randomizer.
        /// </summary>
        [DefaultValue(0)]
        [Category("Data")]
        public int Seed
        {
            get => this.seed;
            set => this.seed = value;
        }

        /// <summary>
        /// Gets or sets particles count.
        /// </summary>
        [DefaultValue(50)]
        [Category("Data")]
        public uint ParticlesCount
        {
            get => this.particlesCount;
            set => this.particlesCount = value;
        }

        /// <summary>
        /// Gets or sets minimal particle width.
        /// </summary>
        [DefaultValue(16)]
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
        [DefaultValue(16)]
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
        [DefaultValue(32)]
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
        [DefaultValue(32)]
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
        [DefaultValue(true)]
        [Category("Data")]
        public bool KeepParticleAspectRatio
        {
            get => this.keepParticleAspectRatio;
            set => this.keepParticleAspectRatio = value;
        }

        /// <summary>
        /// Gets or sets value indicating that all particle must be inside bounds, else particles will be clipped.
        /// </summary>
        [DefaultValue(true)]
        [Category("Data")]
        public bool AvoidClipping
        {
            get => this.avoidClipping;
            set => this.avoidClipping = value;
        }

        /// <summary>
        /// Determines which of methods will be used to avoid particles clipping.
        /// </summary>
        [DefaultValue(AvoidClippingMethod.RandomizeCoordinates)]
        [Category("Data")]
        public AvoidClippingMethod AvoidClippingMethod
        {
            get => this.avoidClippingMethod;
            set => this.avoidClippingMethod = value;
        }

        /// <summary>
        /// Gets or sets minimal opacity of particle.
        /// </summary>
        [DefaultValue(0.1f)]
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
                ClearOpacityImageCache();
            }
        }

        /// <summary>
        /// Gets or sets maximal opacity of particle.
        /// </summary>
        [DefaultValue(1.0f)]
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
                ClearOpacityImageCache();
            }
        }

        /// <summary>
        /// Gets or sets value that allows to change particles opacity. Warning! Opacity changing is slow.
        /// </summary>
        [DefaultValue(false)]
        [Category("Data")]
        public bool AdjustableOpacity
        {
            get => this.adjustableOpacity;
            set => this.adjustableOpacity = value;
        }

        /// <summary>
        /// Gets or sets the data column name to get particle image from.
        /// </summary>
        [DefaultValue("")]
        [Category("Data")]
        [Editor("FastReport.TypeEditors.DataColumnEditor, FastReport", typeof(UITypeEditor))]
        public string DataColumn
        {
            get { return this.dataColumn; }
            set { this.dataColumn = value; }
        }

        /// <summary>
        /// Gets or sets particle image.
        /// </summary>
        [Category("Data")]
        public Image Image
        {
            get => this.image;
            set
            {
                this.image.Dispose();
                this.image = value.Clone() as Image;
                ClearOpacityImageCache();
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Changes opacity of the image.
        /// </summary>
        /// <param name="opacity">Opacity</param>
        /// <returns>Image with selected opacity</returns>
        /// <remarks>Caution! Slow!</remarks>
        private Image SetImageOpacity(float opacity)
        {
            Bitmap bmp = new Bitmap(this.image.Width, this.image.Height);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                ImageAttributes attributes = new ImageAttributes();

                matrix.Matrix33 = opacity;
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.DrawImage(this.image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, this.image.Width, this.image.Height, GraphicsUnit.Pixel, attributes);

                attributes.Dispose();
                gfx.Dispose();
            }
            return bmp;
        }

        /// <summary>
        /// Fills cache with changed opacity images. 
        /// </summary>
        private void PrepareOpacityImageCache()
        {
            float step = (this.maxOpacity - this.minOpacity) / (this.opacityImageCacheLength - 1);
            for(int i = 0; i < opacityImageCache.Length; i++)
            {
                this.opacityImageCache[i] = SetImageOpacity(this.minOpacity + i * step); ;
            }
            this.opacityImageCacheReady = true;
        }

        /// <summary>
        /// Clears opacity images cache.
        /// </summary>
        private void ClearOpacityImageCache()
        {
            for (int i = 0; i < this.opacityImageCacheLength; i++)
            {
                this.opacityImageCache[i]?.Dispose();
            }
            this.opacityImageCacheReady = false;
        }

        #endregion

        #region Protected Methods
        ///<inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { 
                ClearOpacityImageCache();
                Image.Dispose();
            }
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
                AvoidClipping = src.AvoidClipping;
                AvoidClippingMethod = src.AvoidClippingMethod;
                MinOpacity = src.MinOpacity;
                MaxOpacity = src.MaxOpacity;
                AdjustableOpacity = src.AdjustableOpacity;
                DataColumn = src.DataColumn;
                Image.Dispose();
                Image = src.Image.Clone() as Image;
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
                Random opacityRandom = (this.adjustableOpacity) ? (new Random(this.seed)) : (null);
                float width = 0;
                float height = 0;
                float x = 0.0f;
                float y = 0.0f;

                for (int i = 0; i < particlesCount; i++)
                {
                    x = drawLeft + (float)random.NextDouble() * drawWidth;
                    y = drawTop + (float)random.NextDouble() * drawHeight;

                    if (this.keepParticleAspectRatio)
                    {
                        width = random.Next((int)this.minParticleWidth, (int)this.maxParticleWidth + 1);
                        height = (int)((float)this.maxParticleHeight / ((float)maxParticleWidth / width));
                    }
                    else
                    {
                        width = random.Next((int)this.minParticleWidth, (int)this.maxParticleWidth + 1);
                        height = random.Next((int)this.minParticleHeight, (int)this.maxParticleHeight + 1);
                    }

                    width *= e.ScaleX;
                    height *= e.ScaleY;

                    if (this.avoidClipping)
                    {
                        if (x + width > drawLeft + drawWidth)
                        {
                            switch (this.avoidClippingMethod)
                            {
                                case AvoidClippingMethod.LimitCoordinates:
                                    x -= (x + width) - (drawLeft + drawWidth);
                                    break;
                                case AvoidClippingMethod.RandomizeCoordinates:
                                    x = drawLeft + (float)random.NextDouble() * (drawWidth - width);
                                    break;
                            }
                        }
                        if (y + height > drawTop + drawHeight)
                        {
                            switch (this.avoidClippingMethod)
                            {
                                case AvoidClippingMethod.LimitCoordinates:
                                    y -= (y + height) - (drawTop + drawHeight);
                                    break;
                                case AvoidClippingMethod.RandomizeCoordinates:
                                    y = drawTop + (float)random.NextDouble() * (drawHeight - height);
                                    break;
                            }
                        }
                    }

                    if (this.adjustableOpacity && !opacityImageCacheReady)
                        PrepareOpacityImageCache();
     
                    g.DrawImage(
                        (this.adjustableOpacity) ? (this.opacityImageCache[opacityRandom.Next(0, (int)this.opacityImageCacheLength)]) : (this.image),
                        x, y,
                        width, height);
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
            if (AvoidClipping != c.AvoidClipping)
                writer.WriteBool("AvoidClipping", AvoidClipping);
            if (AvoidClippingMethod != c.AvoidClippingMethod)
                writer.WriteValue("AvoidClippingMethod", AvoidClippingMethod);
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
            this.avoidClipping = true;
            this.avoidClippingMethod = AvoidClippingMethod.RandomizeCoordinates;
            this.opacityImageCacheLength = 10;
            this.opacityImageCache = new Image[this.opacityImageCacheLength];
            this.minOpacity = 0.1f;
            this.maxOpacity = 1.0f;
            this.adjustableOpacity = true;
            this.dataColumn = "";
            this.image = Properties.Resources.ParticleSystemIcon.Clone() as Image;
        }
    }
}
