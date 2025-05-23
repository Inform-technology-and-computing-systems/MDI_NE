using System;
using System.Drawing;
using System.Drawing.Drawing2D; // Для SmoothingMode, LineCap, DashStyle
using System.Windows.Forms;
using System.Drawing.Imaging;   // Для ImageFormat

namespace MDIPaint
{
    public partial class DocumentForm : Form
    {
        private Bitmap bitmap;
        private Point startPoint;   // Для всех инструментов, требующих начальной точки
        private Point currentPoint; // Для превью
        private bool isDrawing = false;    // Для непрерывного рисования (Перо, Ластик)
        private bool isPreviewing = false; // Для превью (Линия, Эллипс)

        public string FilePath { get; set; } = null;
        public bool IsDirty { get; set; } = false;

        // Свойства для получения размеров Bitmap (для "Размер холста")
        public int BitmapWidth => bitmap?.Width ?? 0;
        public int BitmapHeight => bitmap?.Height ?? 0;


        public DocumentForm()
        {
            InitializeComponent();
            InitializeCanvas(800, 600); // Используем новый метод
            this.DoubleBuffered = true; // Уменьшение мерцания
            this.FormClosing += DocumentForm_FormClosing; // Запрос на сохранение
            this.MouseEnter += (s, e) => (this.MdiParent as MainForm)?.UpdateChildFormCursor();
        }

        private void InitializeCanvas(int width, int height, Bitmap existingBitmap = null)
        {
            Bitmap oldBitmap = bitmap; // Сохраняем ссылку на старый, чтобы освободить память

            if (existingBitmap != null) // Если передали существующий Bitmap (например, при загрузке)
            {
                bitmap = existingBitmap; // Используем его
            }
            else // Создаем новый пустой Bitmap
            {
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                }
            }

            if (oldBitmap != null && oldBitmap != bitmap) // Если был старый и он не тот же, что новый
            {
                oldBitmap.Dispose(); // Освобождаем память от старого
            }

            this.ClientSize = new Size(bitmap.Width, bitmap.Height); // Размер окна под Bitmap
            this.AutoScrollMinSize = new Size(bitmap.Width, bitmap.Height); // Для прокрутки
            this.IsDirty = (existingBitmap == null); // Новый холст - грязный, загруженный - нет
            this.Invalidate(); // Запросить перерисовку
        }

        // Метод для изменения размера холста извне (например, из MainForm)
        public void ResizeCanvas(int newWidth, int newHeight)
        {
            if (bitmap == null || (bitmap.Width == newWidth && bitmap.Height == newHeight))
                return;

            Bitmap oldBitmap = bitmap;
            Bitmap newBitmap = new Bitmap(newWidth, newHeight, oldBitmap.PixelFormat);

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.White);
                // Копируем содержимое старого изображения в левый верхний угол нового
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
                // Координаты мыши относительно AutoScrollPosition
                startPoint = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
                currentPoint = startPoint;

                switch (MainForm.CurrentTool)
                {
                    case MainForm.DrawingTool.Pen:
                    case MainForm.DrawingTool.Eraser:
                        isDrawing = true;
                        DrawStep(currentPoint); // Первый мазок/стирание
                        break;
                    case MainForm.DrawingTool.Line:
                    case MainForm.DrawingTool.Ellipse:
                        isPreviewing = true;
                        this.Capture = true; // Захват мыши для рисования превью вне окна
                        break;
                }
            }
        }

        private void DocumentForm_MouseMove(object sender, MouseEventArgs e)
        {
            // Координаты мыши относительно AutoScrollPosition
            Point mousePosOnBitmap = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
            currentPoint = mousePosOnBitmap; // Обновляем всегда для превью

            if (isDrawing) // Перо или Ластик
            {
                DrawStep(mousePosOnBitmap);
            }
            else if (isPreviewing) // Линия или Эллипс (превью)
            {
                this.Refresh(); // Вызовет OnPaint, который отрисует bitmap, затем превью
            }
        }

        private void DocumentForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Координаты мыши относительно AutoScrollPosition
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
                        DrawShapeFinal(gBitmap, startPoint, mouseUpPosOnBitmap); // Финальная точка
                    }
                    this.IsDirty = true;
                    this.Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Не вызываем base.OnPaint(e);
            if (bitmap != null)
            {
                // Рисуем Bitmap с учетом прокрутки
                e.Graphics.DrawImage(bitmap, this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            }

            // Рисуем превью поверх, если активен режим превью
            if (isPreviewing)
            {
                // Превью рисуется в координатах ОКНА (не Bitmap)
                // startPoint и currentPoint уже в координатах Bitmap, их нужно преобразовать обратно
                Point previewStart = new Point(startPoint.X + AutoScrollPosition.X, startPoint.Y + AutoScrollPosition.Y);
                Point previewCurrent = new Point(currentPoint.X + AutoScrollPosition.X, currentPoint.Y + AutoScrollPosition.Y);
                DrawShapePreview(e.Graphics, previewStart, previewCurrent);
            }
        }

        private void DrawStep(Point currentPosOnBitmap) // currentPosOnBitmap уже в координатах Bitmap
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
                        g.FillRectangle(Brushes.White, eraseRect); // Стираем белым
                        break;
                }
            }
            this.IsDirty = true;
            startPoint = currentPosOnBitmap; // Обновляем startPoint для следующего шага
            this.Invalidate(); // Перерисовать видимую часть
        }

        // Рисуем превью на графике ОКНА (gForm)
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

        // Рисуем финальную фигуру на графике BITMAP (gBitmap)
        // p1Bitmap и p2Bitmap уже в координатах Bitmap
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
                    this.IsDirty = false; // <--- СБРОС ФЛАГА ПОСЛЕ УСПЕШНОГО СОХРАНЕНИЯ
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения Bitmap: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Здесь IsDirty не сбрасываем, так как сохранение не удалось
                }
            }
        }

        public void LoadBitmap(string path)
        {
            try
            {
                // Сначала загружаем в временный Bitmap, чтобы не испортить текущий в случае ошибки
                Bitmap loadedBitmap = new Bitmap(path);
                InitializeCanvas(loadedBitmap.Width, loadedBitmap.Height, loadedBitmap); // Передаем загруженный Bitmap
                this.IsDirty = false; // Сразу после загрузки файл не изменен
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // В случае ошибки можно создать пустой холст или ничего не делать
                InitializeCanvas(800, 600); // Создаем пустой холст по умолчанию
            }
        }

        private void DocumentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.IsDirty) // Только если есть несохраненные изменения
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
                        this.Activate(); // Убедимся, что это активное окно для main.SaveActiveDocument()

                        // Вот ключевой момент: SaveActiveDocument должен сам решить,
                        // использовать "Сохранить" или "Сохранить как".
                        savedSuccessfully = main.SaveActiveDocument();
                    }

                    if (!savedSuccessfully)
                    {
                        // Если сохранение не удалось (например, пользователь нажал "Отмена" в диалоге "Сохранить как")
                        e.Cancel = true; // Отменяем закрытие
                    }
                    // Если savedSuccessfully == true, то IsDirty уже должен быть сброшен внутри SaveBitmap или SaveActiveDocumentAs
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // Отменяем закрытие
                }
                // Если No, то просто закрываем без сохранения, IsDirty остается true, но окно закрывается
            }
        }
    }
}