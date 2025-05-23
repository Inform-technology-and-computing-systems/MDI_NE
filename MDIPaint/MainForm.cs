using System;
using System.Drawing;
using System.Drawing.Imaging; // Для ImageFormat
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class MainForm : Form
    {
        public static Color CurrentColor { get; set; } = Color.Black;
        public static int CurrentPenWidth { get; set; } = 3;

        public enum DrawingTool
        {
            Pen,    // Уже есть
            Line,   // 6-7 баллов
            Ellipse,// 6-7 баллов
            Eraser  // 6-7 баллов
        }
        public static DrawingTool CurrentTool { get; private set; } = DrawingTool.Pen;
        public static bool FillShapes { get; private set; } = false; // Для заливки фигур (6-7 баллов)

        public MainForm()
        {
            InitializeComponent();

            // Настройка ComboBox для толщины (добавьте его в дизайнер, если еще нет)
            // Убедитесь, что у вас есть элемент toolStripComboBoxWidth
            if (toolStripComboBoxWidth != null)
            {
                toolStripComboBoxWidth.Items.AddRange(new object[] { "1", "2", "3", "5", "8", "10", "15" });
                toolStripComboBoxWidth.SelectedItem = CurrentPenWidth.ToString();
                toolStripComboBoxWidth.SelectedIndexChanged += ToolStripComboBoxWidth_SelectedIndexChanged;
            }
            else if (tstxtPenWidth != null) // Если оставили TextBox
            {
                tstxtPenWidth.Text = CurrentPenWidth.ToString();
                // Подпишитесь на tstxtPenWidth.Validated или tstxtPenWidth.KeyDown (Enter)
            }


            UpdateUiState();
            // Установить начальное состояние кнопок инструментов
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
                if (width > 0 && width < 100) // Ограничение
                {
                    CurrentPenWidth = width;
                }
            }
        }
        // Если используете TextBox tstxtPenWidth, добавьте его обработчик Validated или KeyDown(Enter)
        private void tstxtPenWidth_Validated(object sender, EventArgs e)
        {
            if (tstxtPenWidth == null) return; // Добавим проверку на всякий случай

            if (int.TryParse(tstxtPenWidth.Text, out int width))
            {
                if (width > 0 && width < 100) // Примерные ограничения
                {
                    MainForm.CurrentPenWidth = width; // <--- Присваиваем значение статическому свойству
                                                      // System.Diagnostics.Debug.WriteLine($"Pen width set to: {MainForm.CurrentPenWidth}"); // Для отладки
                }
                else
                {
                    MessageBox.Show("Толщина должна быть числом от 1 до 99.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tstxtPenWidth.Text = MainForm.CurrentPenWidth.ToString(); // Вернуть предыдущее корректное значение
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите числовое значение для толщины.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tstxtPenWidth.Text = MainForm.CurrentPenWidth.ToString(); // Вернуть предыдущее корректное значение
            }
        }


        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm frm = new DocumentForm();
            frm.MdiParent = this;
            frm.Text = "Новый документ " + (this.MdiChildren.Length + 1);
            frm.Show();
        }

        // --- Обработчики цвета (красный, синий, зеленый, другой) ---
        // Эти методы у вас уже есть и они корректны
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
                // Можно создать однотонную иконку для tsddbColor, если хотите
            }
        }
        // --- Конец обработчиков цвета ---

        // --- Файловые операции (Открыть, Сохранить, Сохранить как) ---
        // Эти методы у вас уже есть и они в целом корректны
        // Убедитесь, что в "Сохранить" и "Сохранить как" используется ImageFormat.Png для .png
        private void сохранитькакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm activeChild = this.ActiveMdiChild as DocumentForm;
            if (activeChild != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.AddExtension = true;
                dlg.Filter = "Windows Bitmap (*.bmp)|*.bmp|JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png";
                dlg.Title = "Сохранить изображение как...";
                // Предлагаем имя без звездочки, если она есть
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
                        activeChild.SaveBitmap(dlg.FileName, selectedFormat); // SaveBitmap должен сбрасывать IsDirty
                        activeChild.FilePath = dlg.FileName;
                        activeChild.Text = System.IO.Path.GetFileName(dlg.FileName);
                        // IsDirty сбрасывается внутри SaveBitmap, если успешно
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Если пользователь нажал "Отмена", IsDirty не меняется, и FilePath тоже.
            }
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm activeChild = this.ActiveMdiChild as DocumentForm;
            if (activeChild != null)
            {
                if (string.IsNullOrEmpty(activeChild.FilePath)) // Если путь не задан, то это "Сохранить как"
                {
                    сохранитькакToolStripMenuItem_Click(sender, e);
                }
                else if (activeChild.IsDirty) // Если путь есть И есть изменения
                {
                    try
                    {
                        ImageFormat format = ImageFormat.Bmp; // Определить формат по расширению
                        string extension = System.IO.Path.GetExtension(activeChild.FilePath).ToLower();
                        switch (extension)
                        {
                            case ".jpg": case ".jpeg": format = ImageFormat.Jpeg; break;
                            case ".png": format = ImageFormat.Png; break;
                        }
                        activeChild.SaveBitmap(activeChild.FilePath, format); // SaveBitmap должен сбрасывать IsDirty
                                                                              // IsDirty сбрасывается внутри SaveBitmap, если успешно
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Если путь есть и IsDirty = false, ничего не делаем
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
                    frm.Text = System.IO.Path.GetFileName(dlg.FileName); // Установить заголовок
                    frm.IsDirty = false; // Свежеоткрытый файл не изменен
                    frm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при открытии файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // --- Конец файловых операций ---

        // --- Расположение окон (Каскадом и т.д.) ---
        // Эти методы у вас есть и они корректны
        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.Cascade); }
        private void слеванаправоToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.TileHorizontal); }
        private void сверхуВнизToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.TileVertical); }
        private void упорядочитьЗначкиToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.ArrangeIcons); }
        // --- Конец расположения окон ---

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm frmAbout = new AboutForm();
            frmAbout.ShowDialog(this); // Показываем как модальное окно относительно MainForm
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
                    return; // Отменяем выход
                }
            }
            Application.Exit();
        }

        // Общий обработчик для кнопок инструментов
        // Убедитесь, что у вас есть кнопки tsbPen, tsbLine, tsbEllipse, tsbEraser
        // и им назначен этот обработчик. Также установите CheckOnClick = True для них.
        private void ToolStripButtonTool_Click(object sender, EventArgs e)
        {
            // Снимаем Checked со всех кнопок инструментов
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

        // Обработчик для кнопки "Заливка фигур" (tsbToggleFill)
        // Установите CheckOnClick = True для этой кнопки
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
                        activeDoc.Cursor = Cursors.Default; // Или создайте свой курсор-ластик
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
            размерХолстаToolStripMenuItem.Enabled = isChildActive; // Для "Размер холста"

            каскадомToolStripMenuItem.Enabled = isChildActive;
            слеванаправоToolStripMenuItem.Enabled = isChildActive;
            сверхуВнизToolStripMenuItem.Enabled = isChildActive;
            упорядочитьЗначкиToolStripMenuItem.Enabled = isChildActive;

            // Элементы ToolStrip
            if (tsddbColor != null) tsddbColor.Enabled = isChildActive;
            if (toolStripComboBoxWidth != null) toolStripComboBoxWidth.Enabled = isChildActive;
            else if (tstxtPenWidth != null) tstxtPenWidth.Enabled = isChildActive;


            if (tsbPen != null) tsbPen.Enabled = isChildActive;
            if (tsbLine != null) tsbLine.Enabled = isChildActive;
            if (tsbEllipse != null) tsbEllipse.Enabled = isChildActive;
            if (tsbEraser != null) tsbEraser.Enabled = isChildActive;
            if (tsbToggleFill != null) tsbToggleFill.Enabled = isChildActive;


            // Установка Checked состояния при активации окна
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

        // Метод для "Сохранить" из DocumentForm_FormClosing
        public bool SaveActiveDocument() // Возвращает true, если сохранение прошло успешно (или не требовалось), false - если пользователь отменил
        {
            DocumentForm activeChild = this.ActiveMdiChild as DocumentForm;
            if (activeChild == null) return true; // Нет активного документа, нечего сохранять

            if (string.IsNullOrEmpty(activeChild.FilePath)) // Если путь не задан, всегда "Сохранить как"
            {
                // Вызываем логику "Сохранить как"
                // Обратите внимание, что сохранитькакToolStripMenuItem_Click не возвращает bool,
                // поэтому нам нужно проверить IsDirty и FilePath после его вызова.
                сохранитькакToolStripMenuItem_Click(this, EventArgs.Empty); // Передаем фиктивные аргументы
                return !activeChild.IsDirty && !string.IsNullOrEmpty(activeChild.FilePath); // Успешно, если файл теперь сохранен и не "грязный"
            }
            else // Путь есть, используем "Сохранить"
            {
                // Вызываем логику "Сохранить"
                сохранитьToolStripMenuItem_Click(this, EventArgs.Empty);
                return !activeChild.IsDirty; // Успешно, если файл теперь не "грязный"
            }
        }

        // Обработчик для меню "Рисунок" -> "Размер холста"
        private void размерХолстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm activeDoc = this.ActiveMdiChild as DocumentForm;
            if (activeDoc == null) return;

            // Создаем и показываем форму для ввода размера
            // Убедитесь, что CanvasSizeForm существует и настроена
            using (CanvasSizeForm sizeDialog = new CanvasSizeForm())
            {
                sizeDialog.CanvasWidth = activeDoc.BitmapWidth;  // Предполагаем, что есть свойство для получения размера Bitmap
                sizeDialog.CanvasHeight = activeDoc.BitmapHeight; // в DocumentForm

                if (sizeDialog.ShowDialog(this) == DialogResult.OK)
                {
                    activeDoc.ResizeCanvas(sizeDialog.CanvasWidth, sizeDialog.CanvasHeight);
                }
            }
        }
    }
}