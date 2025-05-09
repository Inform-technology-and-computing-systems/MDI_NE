using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class MainForm: Form
    {
        public static Color CurrentColor { get; set; } = Color.Black; // Цвет по умолчанию
        public static int CurrentPenWidth { get; set; } = 3; // Толщина по умолчанию

        public MainForm()
        {
            InitializeComponent();
            UpdateUiState();
        }

        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DocumentForm frm = new DocumentForm();
            frm.MdiParent = this; // Указываем, что это дочернее окно
            frm.Show();
        }
        private void tstxtPenWidth_Click(object sender, EventArgs e)
        {
            if (int.TryParse(tstxtPenWidth.Text, out int width))
            {
                if (width > 0 && width < 100) // Ограничение толщины
                {
                    MainForm.CurrentPenWidth = width;
                }
                else
                {
                    MessageBox.Show("Толщина должна быть числом от 1 до 99.");
                    // Можно вернуть предыдущее значение или оставить некорректное
                }
            }
            else
            {
                MessageBox.Show("Введите числовое значение для толщины.");
            }
        }

        private void красныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.CurrentColor = Color.Red;
            // Обновим иконку кнопки DropDown (опционально)
            tsddbColor.Image = красныйToolStripMenuItem.Image;
        }

        private void синийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.CurrentColor = Color.Blue;
            tsddbColor.Image = синийToolStripMenuItem.Image;
        }

        private void зеленыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.CurrentColor = Color.Green;
            tsddbColor.Image = зеленыйToolStripMenuItem.Image;
        }

        private void другойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                MainForm.CurrentColor = cd.Color;
                // Можно создать однотонную иконку для выбранного цвета и установить ее
                // tsddbColor.Image = CreateColorIcon(cd.Color); // Пример
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

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImageFormat selectedFormat = ImageFormat.Bmp; // По умолчанию
                        string extension = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                        switch (extension)
                        {
                            case ".jpg":
                            case ".jpeg":
                                selectedFormat = ImageFormat.Jpeg;
                                break;
                            case ".png":
                                selectedFormat = ImageFormat.Png;
                                break;
                                // *.bmp будет по умолчанию
                        }

                        activeChild.SaveBitmap(dlg.FileName, selectedFormat);
                        activeChild.FilePath = dlg.FileName;
                        activeChild.Text = System.IO.Path.GetFileName(dlg.FileName); // Обновить заголовок окна
                        activeChild.IsDirty = false;
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
                // Если файл еще не сохранялся (нет пути) ИЛИ если он был изменен
                if (string.IsNullOrEmpty(activeChild.FilePath) || activeChild.IsDirty)
                {
                    // Вызываем логику "Сохранить как..."
                    сохранитькакToolStripMenuItem_Click(sender, e);
                }
                // Если путь есть и изменений не было, можно ничего не делать
                // или принудительно сохранить по тому же пути:
                // else if (!string.IsNullOrEmpty(activeChild.FilePath))
                // {
                //    try
                //    {
                //        // Определить формат по расширению или хранить его
                //        ImageFormat format = DetermineFormatFromPath(activeChild.FilePath);
                //        activeChild.SaveBitmap(activeChild.FilePath, format);
                //        activeChild.IsDirty = false;
                //    }
                //    catch (Exception ex) { /* обработка ошибки */ }
                // }
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
                    frm.LoadBitmap(dlg.FileName); // Метод загрузки в DocumentForm
                    frm.MdiParent = this;
                    frm.FilePath = dlg.FileName;
                    frm.Text = System.IO.Path.GetFileName(dlg.FileName);
                    frm.IsDirty = false; // Свежеоткрытый файл не изменен
                    frm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при открытии файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void слеванаправоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void сверхуВнизToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void упорядочитьЗначкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm frmAbout = new AboutForm();
            frmAbout.ShowDialog(); // Показываем как модальное окно
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
            // Перед выходом хорошо бы проверить несохраненные изменения во всех окнах
        }

        private void UpdateUiState()
        {
            bool isChildActive = (this.ActiveMdiChild != null);

            // Меню Файл
            сохранитьToolStripMenuItem.Enabled = isChildActive;
            сохранитькакToolStripMenuItem.Enabled = isChildActive;

            // Меню Рисунок (если есть команды, зависящие от окна)
            размерХолстаToolStripMenuItem.Enabled = isChildActive;

            // Меню Окно (команды расположения)
            каскадомToolStripMenuItem.Enabled = isChildActive;
            слеванаправоToolStripMenuItem.Enabled = isChildActive;
            сверхуВнизToolStripMenuItem.Enabled = isChildActive;
            упорядочитьЗначкиToolStripMenuItem.Enabled = isChildActive;

            // ToolStrip (инструменты, цвет, толщина)
            tsddbColor.Enabled = isChildActive;
            tstxtPenWidth.Enabled = isChildActive;
            // Добавьте сюда кнопки инструментов, когда они появятся
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            UpdateUiState();
        }
    }
}
