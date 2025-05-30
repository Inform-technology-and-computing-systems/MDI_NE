using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace MDIPaint
{
    public partial class DocumentForm : Form
    {
        private Bitmap bitmap;
        private Point startPoint;
        private Point currentPoint;
        private bool isDrawing = false;
        private bool isPreviewing = false;

        public string FilePath { get; set; } = null;
        public bool IsDirty { get; set; } = false;

        public int BitmapWidth => bitmap?.Width ?? 0;
        public int BitmapHeight => bitmap?.Height ?? 0;


        public DocumentForm()
        {
            InitializeComponent();
            InitializeCanvas(800, 600);
            this.DoubleBuffered = true;
            this.FormClosing += DocumentForm_FormClosing;
            this.MouseEnter += (s, e) => (this.MdiParent as MainForm)?.UpdateChildFormCursor();
        }

        private void InitializeCanvas(int width, int height, Bitmap existingBitmap = null)
        {
            Bitmap oldBitmap = bitmap;

            if (existingBitmap != null)
            {
                bitmap = existingBitmap;
            }
            else
            {
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                }
            }

            if (oldBitmap != null && oldBitmap != bitmap)
            {
                oldBitmap.Dispose();
            }

            this.ClientSize = new Size(bitmap.Width, bitmap.Height);
            this.AutoScrollMinSize = new Size(bitmap.Width, bitmap.Height);
            this.IsDirty = (existingBitmap == null);
            this.Invalidate();
        }

        public void ResizeCanvas(int newWidth, int newHeight)
        {
            if (bitmap == null || (bitmap.Width == newWidth && bitmap.Height == newHeight))
                return;

            Bitmap oldBitmap = bitmap;
            Bitmap newBitmap = new Bitmap(newWidth, newHeight, oldBitmap.PixelFormat);

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.White);
                g.DrawImage(oldBitmap, 0, 0);
            }

            bitmap = newBitmap;
            oldBitmap.Dispose();

            this.AutoScrollMinSize = bitmap.Size;
            this.IsDirty = true;
            this.Invalidate();
        }


        private void DocumentForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && bitmap != null)
            {
                startPoint = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
                currentPoint = startPoint;

                switch (MainForm.CurrentTool)
                {
                    case MainForm.DrawingTool.Pen:
                    case MainForm.DrawingTool.Eraser:
                        isDrawing = true;
                        DrawStep(currentPoint);
                        break;
                    case MainForm.DrawingTool.Line:
                    case MainForm.DrawingTool.Ellipse:
                        isPreviewing = true;
                        this.Capture = true;
                        break;
                }
            }
        }

        private void DocumentForm_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosOnBitmap = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
            currentPoint = mousePosOnBitmap;

            if (isDrawing)
            {
                DrawStep(mousePosOnBitmap);
            }
            else if (isPreviewing)
            {
                this.Refresh();
            }
        }

        private void DocumentForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mouseUpPosOnBitmap = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);

                if (isDrawing)
                {
                    isDrawing = false;
                }
                else if (isPreviewing)
                {
                    isPreviewing = false;
                    this.Capture = false;
                    using (Graphics gBitmap = Graphics.FromImage(bitmap))
                    {
                        DrawShapeFinal(gBitmap, startPoint, mouseUpPosOnBitmap);
                    }
                    this.IsDirty = true;
                    this.Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (bitmap != null)
            {
                e.Graphics.DrawImage(bitmap, this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            }

            if (isPreviewing)
            {
                Point previewStart = new Point(startPoint.X + AutoScrollPosition.X, startPoint.Y + AutoScrollPosition.Y);
                Point previewCurrent = new Point(currentPoint.X + AutoScrollPosition.X, currentPoint.Y + AutoScrollPosition.Y);
                DrawShapePreview(e.Graphics, previewStart, previewCurrent);
            }
        }

        private void DrawStep(Point currentPosOnBitmap)
        {
            if (bitmap == null) return;

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                switch (MainForm.CurrentTool)
                {
                    case MainForm.DrawingTool.Pen:
                        using (Pen pen = new Pen(MainForm.CurrentColor, MainForm.CurrentPenWidth))
                        {
                            pen.StartCap = LineCap.Round;
                            pen.EndCap = LineCap.Round;
                            g.DrawLine(pen, startPoint, currentPosOnBitmap);
                        }
                        break;
                    case MainForm.DrawingTool.Eraser:
                        int eraserSize = MainForm.CurrentPenWidth * 3;
                        Rectangle eraseRect = new Rectangle(
                            currentPosOnBitmap.X - eraserSize / 2,
                            currentPosOnBitmap.Y - eraserSize / 2,
                            eraserSize, eraserSize);
                        g.FillRectangle(Brushes.White, eraseRect);
                        break;
                }
            }
            this.IsDirty = true;
            startPoint = currentPosOnBitmap;
            this.Invalidate();
        }

        private void DrawShapePreview(Graphics gForm, Point p1Window, Point p2Window)
        {
            using (Pen previewPen = new Pen(Color.Gray, 1))
            {
                previewPen.DashStyle = DashStyle.Dash;
                switch (MainForm.CurrentTool)
                {
                    case MainForm.DrawingTool.Line:
                        gForm.DrawLine(previewPen, p1Window, p2Window);
                        break;
                    case MainForm.DrawingTool.Ellipse:
                        gForm.DrawEllipse(previewPen, GetRectangle(p1Window, p2Window));
                        break;
                }
            }
        }

        private void DrawShapeFinal(Graphics gBitmap, Point p1Bitmap, Point p2Bitmap)
        {
            gBitmap.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = GetRectangle(p1Bitmap, p2Bitmap);

            using (Pen pen = new Pen(MainForm.CurrentColor, MainForm.CurrentPenWidth))
            using (Brush brush = new SolidBrush(MainForm.CurrentColor))
            {
                switch (MainForm.CurrentTool)
                {
                    case MainForm.DrawingTool.Line:
                        gBitmap.DrawLine(pen, p1Bitmap, p2Bitmap);
                        break;
                    case MainForm.DrawingTool.Ellipse:
                        if (MainForm.FillShapes)
                        {
                            gBitmap.FillEllipse(brush, rect);
                        }
                        else
                        {
                            gBitmap.DrawEllipse(pen, rect);
                        }
                        break;
                }
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        public void SaveBitmap(string path, ImageFormat format)
        {
            if (bitmap != null)
            {
                try
                {
                    bitmap.Save(path, format);
                    this.IsDirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения Bitmap: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void LoadBitmap(string path)
        {
            try
            {
                Bitmap loadedBitmap = new Bitmap(path);
                InitializeCanvas(loadedBitmap.Width, loadedBitmap.Height, loadedBitmap);
                this.IsDirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                InitializeCanvas(800, 600);
            }
        }

        private void DocumentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.IsDirty)
            {
                DialogResult result = MessageBox.Show(
                    $"Сохранить изменения в файле \"{this.Text}\"?",
                    "MDIPaint - Обнаружены изменения",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    bool savedSuccessfully = false;
                    if (this.MdiParent is MainForm main)
                    {
                        this.Activate();

                        savedSuccessfully = main.SaveActiveDocument();
                    }

                    if (!savedSuccessfully)
                    {
                        e.Cancel = true;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}