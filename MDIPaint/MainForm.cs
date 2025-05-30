using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class MainForm : Form
    {
        public static Color CurrentColor { get; set; } = Color.Black;
        public static int CurrentPenWidth { get; set; } = 3;

        public enum DrawingTool
        {
            Pen,
            Line,
            Ellipse,
            Eraser
        }
        public static DrawingTool CurrentTool { get; private set; } = DrawingTool.Pen;
        public static bool FillShapes { get; private set; } = false;

        public MainForm()
        {
            InitializeComponent();


            if (toolStripComboBoxWidth != null)
            {
                toolStripComboBoxWidth.Items.AddRange(new object[] { "1", "2", "3", "5", "8", "10", "15" });
                toolStripComboBoxWidth.SelectedItem = CurrentPenWidth.ToString();
                toolStripComboBoxWidth.SelectedIndexChanged += ToolStripComboBoxWidth_SelectedIndexChanged;
            }
            else if (tstxtPenWidth != null)
            {
                tstxtPenWidth.Text = CurrentPenWidth.ToString();
            }


            UpdateUiState();
            if (tsbPen != null) tsbPen.Checked = (CurrentTool == DrawingTool.Pen);
            if (tsbLine != null) tsbLine.Checked = (CurrentTool == DrawingTool.Line);
            if (tsbEllipse != null) tsbEllipse.Checked = (CurrentTool == DrawingTool.Ellipse);
            if (tsbEraser != null) tsbEraser.Checked = (CurrentTool == DrawingTool.Eraser);
            if (tsbToggleFill != null) tsbToggleFill.Checked = FillShapes;
        }

        private void ToolStripComboBoxWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBoxWidth.SelectedItem != null &&
                int.TryParse(toolStripComboBoxWidth.SelectedItem.ToString(), out int width))
            {
                if (width > 0 && width < 100)
                {
                    CurrentPenWidth = width;
                }
            }
        }

        private void tstxtPenWidth_Validated(object sender, EventArgs e)
        {
            if (tstxtPenWidth == null) return;

            if (int.TryParse(tstxtPenWidth.Text, out int width))
            {
                if (width > 0 && width < 100)
                {
                    MainForm.CurrentPenWidth = width;
                }
                else
                {
                    MessageBox.Show("Толщина должна быть числом от 1 до 99.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tstxtPenWidth.Text = MainForm.CurrentPenWidth.ToString();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите числовое значение для толщины.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tstxtPenWidth.Text = MainForm.CurrentPenWidth.ToString();
            }
        }


        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm frm = new DocumentForm();
            frm.MdiParent = this;
            frm.Text = "Новый документ " + (this.MdiChildren.Length + 1);
            frm.Show();
        }

        private void красныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentColor = Color.Red;
            if (tsddbColor != null && красныйToolStripMenuItem.Image != null)
                tsddbColor.Image = красныйToolStripMenuItem.Image;
        }
        private void синийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentColor = Color.Blue;
            if (tsddbColor != null && синийToolStripMenuItem.Image != null)
                tsddbColor.Image = синийToolStripMenuItem.Image;
        }
        private void зеленыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentColor = Color.Green;
            if (tsddbColor != null && зеленыйToolStripMenuItem.Image != null)
                tsddbColor.Image = зеленыйToolStripMenuItem.Image;
        }
        private void другойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                CurrentColor = cd.Color;
            }
        }

        private void сохранитькакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm activeChild = this.ActiveMdiChild as DocumentForm;
            if (activeChild != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.AddExtension = true;
                dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png";
                dlg.Title = "Сохранить изображение как...";
                string currentName = activeChild.Text;
                if (currentName.EndsWith("*"))
                {
                    currentName = currentName.Substring(0, currentName.Length - 1);
                }
                dlg.FileName = currentName;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImageFormat selectedFormat = ImageFormat.Bmp;
                        string extension = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                        switch (extension)
                        {
                            case ".jpg": case ".jpeg": selectedFormat = ImageFormat.Jpeg; break;
                            case ".png": selectedFormat = ImageFormat.Png; break;
                        }
                        activeChild.SaveBitmap(dlg.FileName, selectedFormat);
                        activeChild.FilePath = dlg.FileName;
                        activeChild.Text = System.IO.Path.GetFileName(dlg.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm activeChild = this.ActiveMdiChild as DocumentForm;
            if (activeChild != null)
            {
                if (string.IsNullOrEmpty(activeChild.FilePath))
                {
                    сохранитькакToolStripMenuItem_Click(sender, e);
                }
                else if (activeChild.IsDirty)
                {
                    try
                    {
                        ImageFormat format = ImageFormat.Bmp;
                        string extension = System.IO.Path.GetExtension(activeChild.FilePath).ToLower();
                        switch (extension)
                        {
                            case ".jpg": case ".jpeg": format = ImageFormat.Jpeg; break;
                            case ".png": format = ImageFormat.Png; break;
                        }
                        activeChild.SaveBitmap(activeChild.FilePath, format);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            dlg.Title = "Открыть изображение";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DocumentForm frm = new DocumentForm();
                    frm.LoadBitmap(dlg.FileName);
                    frm.MdiParent = this;
                    frm.FilePath = dlg.FileName;
                    frm.Text = System.IO.Path.GetFileName(dlg.FileName);
                    frm.IsDirty = false;
                    frm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при открытии файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.Cascade); }
        private void слеванаправоToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.TileHorizontal); }
        private void сверхуВнизToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.TileVertical); }
        private void упорядочитьЗначкиToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.ArrangeIcons); }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm frmAbout = new AboutForm();
            frmAbout.ShowDialog(this);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool unsavedChangesExist = false;
            foreach (Form child in this.MdiChildren)
            {
                if (child is DocumentForm doc && doc.IsDirty)
                {
                    unsavedChangesExist = true;
                    break;
                }
            }

            if (unsavedChangesExist)
            {
                DialogResult result = MessageBox.Show(
                    "Есть несохраненные изменения. Выйти без сохранения?",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return;
                }
            }
            Application.Exit();
        }

        private void ToolStripButtonTool_Click(object sender, EventArgs e)
        {
            if (tsbPen != null) tsbPen.Checked = false;
            if (tsbLine != null) tsbLine.Checked = false;
            if (tsbEllipse != null) tsbEllipse.Checked = false;
            if (tsbEraser != null) tsbEraser.Checked = false;

            ToolStripButton clickedButton = sender as ToolStripButton;
            if (clickedButton == null) return;

            clickedButton.Checked = true;

            if (clickedButton == tsbPen) CurrentTool = DrawingTool.Pen;
            else if (clickedButton == tsbLine) CurrentTool = DrawingTool.Line;
            else if (clickedButton == tsbEllipse) CurrentTool = DrawingTool.Ellipse;
            else if (clickedButton == tsbEraser) CurrentTool = DrawingTool.Eraser;

            UpdateChildFormCursor();
        }

        private void tsbToggleFill_Click(object sender, EventArgs e)
        {
            if (tsbToggleFill != null)
                FillShapes = tsbToggleFill.Checked;
        }

        public void UpdateChildFormCursor()
        {
            if (this.ActiveMdiChild is DocumentForm activeDoc)
            {
                switch (CurrentTool)
                {
                    case DrawingTool.Pen:
                    case DrawingTool.Line:
                    case DrawingTool.Ellipse:
                        activeDoc.Cursor = Cursors.Cross;
                        break;
                    case DrawingTool.Eraser:
                        activeDoc.Cursor = Cursors.Default;
                        break;
                    default:
                        activeDoc.Cursor = Cursors.Default;
                        break;
                }
            }
        }

        private void UpdateUiState()
        {
            bool isChildActive = (this.ActiveMdiChild != null);

            сохранитьToolStripMenuItem.Enabled = isChildActive;
            сохранитькакToolStripMenuItem.Enabled = isChildActive;
            размерХолстаToolStripMenuItem.Enabled = isChildActive;

            каскадомToolStripMenuItem.Enabled = isChildActive;
            слеванаправоToolStripMenuItem.Enabled = isChildActive;
            сверхуВнизToolStripMenuItem.Enabled = isChildActive;
            упорядочитьЗначкиToolStripMenuItem.Enabled = isChildActive;


            if (tsddbColor != null) tsddbColor.Enabled = isChildActive;
            if (toolStripComboBoxWidth != null) toolStripComboBoxWidth.Enabled = isChildActive;
            else if (tstxtPenWidth != null) tstxtPenWidth.Enabled = isChildActive;


            if (tsbPen != null) tsbPen.Enabled = isChildActive;
            if (tsbLine != null) tsbLine.Enabled = isChildActive;
            if (tsbEllipse != null) tsbEllipse.Enabled = isChildActive;
            if (tsbEraser != null) tsbEraser.Enabled = isChildActive;
            if (tsbToggleFill != null) tsbToggleFill.Enabled = isChildActive;


            if (isChildActive)
            {
                if (tsbPen != null) tsbPen.Checked = (CurrentTool == DrawingTool.Pen);
                if (tsbLine != null) tsbLine.Checked = (CurrentTool == DrawingTool.Line);
                if (tsbEllipse != null) tsbEllipse.Checked = (CurrentTool == DrawingTool.Ellipse);
                if (tsbEraser != null) tsbEraser.Checked = (CurrentTool == DrawingTool.Eraser);
                if (tsbToggleFill != null) tsbToggleFill.Checked = FillShapes;
            }
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            UpdateUiState();
            UpdateChildFormCursor();
        }

        public bool SaveActiveDocument()
        {
            DocumentForm activeChild = this.ActiveMdiChild as DocumentForm;
            if (activeChild == null) return true;

            if (string.IsNullOrEmpty(activeChild.FilePath))
            {
                сохранитькакToolStripMenuItem_Click(this, EventArgs.Empty);
                return !activeChild.IsDirty && !string.IsNullOrEmpty(activeChild.FilePath);
            }
            else
            {
                сохранитьToolStripMenuItem_Click(this, EventArgs.Empty);
                return !activeChild.IsDirty;
            }
        }

        private void размерХолстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm activeDoc = this.ActiveMdiChild as DocumentForm;
            if (activeDoc == null) return;

            using (CanvasSizeForm sizeDialog = new CanvasSizeForm())
            {
                sizeDialog.CanvasWidth = activeDoc.BitmapWidth;
                sizeDialog.CanvasHeight = activeDoc.BitmapHeight;

                if (sizeDialog.ShowDialog(this) == DialogResult.OK)
                {
                    activeDoc.ResizeCanvas(sizeDialog.CanvasWidth, sizeDialog.CanvasHeight);
                }
            }
        }
    }
}