using FastReport;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static FastReport.PolyLineObject;

namespace ParticleSystemFRNet
{
    public enum AvoidClippingMethod
    {
        LimitCoordinates = 0,
        RandomizeCoordinates = 1
    }

    public enum PolygonMode
    {
        InsidePolygon = 0,
        OutsidePolygon = 1
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
        private PolyLineObject polyLineObject = null;
        private PolygonMode polygonMode;

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

        /// <summary>
        /// Gets or sets polygon or polyline for drawing particles in.
        /// </summary>
        [Category("Data")]
        [DefaultValue(null)]
        [TypeConverter(typeof(FastReport.TypeConverters.ComponentRefConverter))]
        [Editor("FastReport.TypeEditors.ReportComponentRefEditor, FastReport", typeof(UITypeEditor))]
        public PolyLineObject PolyLineObject
        {
            get => this.polyLineObject;
            set
            {
                this.polyLineObject = value;
                if (this.polyLineObject != null)
                {
                    this.polyLineObject.Disposed += (sender, e) => { this.polyLineObject = null; };
                }
            }
        }

        /// <summary>
        /// Determines how particles will spread in polygon object.
        /// </summary>
        [DefaultValue(PolygonMode.InsidePolygon)]
        [Category("Data")]
        public PolygonMode PolygonMode
        {
            get => this.polygonMode;
            set => this.polygonMode = value;
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
                this.opacityImageCache[i] = SetImageOpacity(this.minOpacity + i * step);
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
                PolyLineObject = src.PolyLineObject;
                PolygonMode = src.PolygonMode;
            }
        }

        ///<inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            base.Draw(e);

            Graphics g = e.Graphics;
            if (Image == null)
                Image = Properties.Resources.ParticleSystemIcon;
            
            float drawLeft = 0.0f;
            float drawTop = 0.0f;
            float drawWidth = 0.0f;
            float drawHeight = 0.0f;
            GraphicsPath path = null;

            if (this.polyLineObject == null)
            {
                drawLeft = AbsLeft * e.ScaleX;
                drawTop = AbsTop * e.ScaleY;
                drawWidth = Width * e.ScaleX;
                drawHeight = Height * e.ScaleY;
            }
            else if(this.polyLineObject != null)
            {
                drawLeft = this.polyLineObject.AbsLeft * e.ScaleX;
                drawTop = this.polyLineObject.AbsTop * e.ScaleY;
                drawWidth = this.polyLineObject.Width * e.ScaleX;
                drawHeight = this.polyLineObject.Height * e.ScaleY;
                path = this.polyLineObject.GetPath(new Pen(Color.Black), this.polyLineObject.AbsLeft, this.polyLineObject.AbsTop, this.polyLineObject.AbsLeft + this.polyLineObject.Width, this.polyLineObject.AbsTop + this.polyLineObject.Height, e.ScaleX, e.ScaleY);
            }

            RectangleF drawRect = new RectangleF(drawLeft, drawTop, drawWidth, drawHeight);

            GraphicsState state = g.Save();
            try
            {     
                if(this.polyLineObject == null)
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

                PointF[] points = (this.polyLineObject is PolyLineObject) ? (path.PathPoints) : (null);
                PolyPointCollection polyPoints = (this.polyLineObject is PolyLineObject) ? (polyLineObject.Points) : (null);
                PointF absPolyLineZero = (this.polyLineObject is PolyLineObject && polyPoints.Count > 0) ? (new PointF(points[0].X / e.ScaleX- polyPoints[0].X, points[0].Y / e.ScaleY - polyPoints[0].Y)) : (new PointF(0.0f, 0.0f));

                for (int i = 0; i < particlesCount; i++)
                {
                    
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

                    if ((this.polyLineObject != null) && !(this.polyLineObject is PolygonObject) && polyPoints.Count > 0)
                    {
                        int n = random.Next(1, polyPoints.Count);
                        double pos = random.NextDouble();

                        if (polyPoints[n - 1].RightCurve != null || polyPoints[n].LeftCurve != null)
                        {
                            PointF p1 = points[0], p2 = points[0], p3 = points[0], p4 = points[0];
                            if (polyPoints[n].LeftCurve != null)
                            {
                                p1 = new PointF(polyPoints[n - 1].X, polyPoints[n - 1].Y);
                                p4 = new PointF(polyPoints[n].X, polyPoints[n].Y);
                                if (polyPoints[n - 1].RightCurve != null)
                                    p2 = new PointF(p1.X + polyPoints[n - 1].RightCurve.X, p1.Y + polyPoints[n - 1].RightCurve.Y);
                                else
                                    p2 = new PointF(p1.X + (p4.X - p1.X) * 0.333f, p1.Y + (p4.Y - p1.Y) * 0.333f);
                                p3 = new PointF(p4.X + polyPoints[n].LeftCurve.X, p4.Y + polyPoints[n].LeftCurve.Y);  
                            }
                            else if (polyPoints[n - 1].RightCurve != null)
                            {
                                p1 = new PointF(polyPoints[n - 1].X, polyPoints[n - 1].Y);
                                p2 = new PointF(p1.X + polyPoints[n - 1].RightCurve.X, p1.Y + polyPoints[n - 1].RightCurve.Y);
                                p4 = new PointF(polyPoints[n].X, polyPoints[n].Y);
                                if(polyPoints[n].LeftCurve != null)
                                    p3 = new PointF(p4.X + polyPoints[n].LeftCurve.X, p4.Y + polyPoints[n].LeftCurve.Y);
                                else
                                    p3 = new PointF(p4.X + (p1.X - p4.X) * 0.333f, p4.Y + (p1.Y - p4.Y) * 0.333f);
                            }



                            float x1 = p1.X + (float)(pos * (p2.X - p1.X));
                            float y1 = p1.Y + (float)(pos * (p2.Y - p1.Y));

                            float x2 = p2.X + (float)(pos * (p3.X - p2.X));
                            float y2 = p2.Y + (float)(pos * (p3.Y - p2.Y));

                            float x3 = p3.X + (float)(pos * (p4.X - p3.X));
                            float y3 = p3.Y + (float)(pos * (p4.Y - p3.Y));

                            float x21 = x1 + (float)(pos * (x2 - x1));
                            float y21 = y1 + (float)(pos * (y2 - y1));

                            float x22 = x2 + (float)(pos * (x3 - x2));
                            float y22 = y2 + (float)(pos * (y3 - y2));

                            x = x21 + (float)(pos * (x22 - x21));
                            y = y21 + (float)(pos * (y22 - y21));
                        }
                        else
                        {
                            x = (polyPoints[n - 1].X + (float)(pos * (polyPoints[n].X - polyPoints[n - 1].X)));
                            y = (polyPoints[n - 1].Y + (float)(pos * (polyPoints[n].Y - polyPoints[n - 1].Y)));
                        }

                        x = (absPolyLineZero.X + x) * e.ScaleX - width / 2;
                        y = (absPolyLineZero.Y + y) * e.ScaleY - height / 2;
                    }
                    else
                    {
                        x = drawLeft + (float)random.NextDouble() * drawWidth;
                        y = drawTop + (float)random.NextDouble() * drawHeight;
                        if (this.polyLineObject is PolygonObject)
                        {
                            if (drawWidth > width && drawHeight > height)
                            {
                                switch (this.polygonMode)
                                {
                                    case PolygonMode.InsidePolygon:
                                        while (!path.IsVisible(x, y))
                                        {
                                            x = drawLeft + (float)random.NextDouble() * drawWidth;
                                            y = drawTop + (float)random.NextDouble() * drawHeight;
                                        }
                                        break;
                                    case PolygonMode.OutsidePolygon:
                                        while (path.IsVisible(x, y))
                                        {
                                            x = drawLeft + (float)random.NextDouble() * drawWidth;
                                            y = drawTop + (float)random.NextDouble() * drawHeight;
                                        }
                                        break;
                                }
                            }
                            x -= width / 2;
                            y -= height / 2;
                        }
                    }

                    if (this.avoidClipping && this.polyLineObject == null)
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
            if (PolyLineObject != c.PolyLineObject)
                writer.WriteRef("PolyLineObject", PolyLineObject);
            if (PolygonMode != c.PolygonMode)
                writer.WriteValue("PolygonMode", PolygonMode);
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
                Image.Dispose();

                if (data is Image)
                {
                    Image = (data as Image).Clone() as Image;
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
            this.PolygonMode = PolygonMode.InsidePolygon;
        }
    }
}
