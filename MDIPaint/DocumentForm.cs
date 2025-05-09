using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging; // Для ImageFormat (позже)

namespace MDIPaint
{
    public partial class DocumentForm: Form
    {
        private Bitmap bitmap; // Холст для рисования
        private Point lastPoint; // Последняя точка для рисования линии
        private bool isDrawing = false; // Флаг, указывающий, идет ли рисование
        public string FilePath { get; set; } = null; // Путь к файлу (для сохранения)
        public bool IsDirty { get; set; } = false; // Флаг изменений

        public DocumentForm()
        {
            InitializeComponent();
            // Создаем Bitmap размером, например, 800x600
            // Лучше сделать размер настраиваемым (через "Размер холста")
            bitmap = new Bitmap(800, 600);
            // Заполним его белым цветом
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }
            this.ClientSize = bitmap.Size; // Подгоним размер окна под Bitmap
            this.IsDirty = false; // Новый файл не изменен
        }

        private void DocumentForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                lastPoint = e.Location;
            }
        }

        private void DocumentForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                // Рисуем на Bitmap
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Используем статические свойства из MainForm
                    using (Pen pen = new Pen(MainForm.CurrentColor, MainForm.CurrentPenWidth))
                    {
                        pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        g.DrawLine(pen, lastPoint, e.Location);
                    }
                }
                lastPoint = e.Location;
                this.Invalidate(); // Запрашиваем перерисовку формы
                this.IsDirty = true; // Помечаем как измененный
            }
        }

        private void DocumentForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Отображаем наш Bitmap
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        public void SaveBitmap(string path, ImageFormat format)
        {
            if (bitmap != null)
            {
                bitmap.Save(path, format);
            }
        }
        public void LoadBitmap(string path)
        {
            // Важно освободить предыдущий Bitmap, если он есть
            if (bitmap != null)
            {
                bitmap.Dispose();
            }
            // Загружаем новый Bitmap из файла
            bitmap = new Bitmap(path);
            // Обновляем размер формы и AutoScrollMinSize
            this.ClientSize = bitmap.Size;
            this.AutoScrollMinSize = bitmap.Size;
        }
    }
}
