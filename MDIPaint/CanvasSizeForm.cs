using System;
using System.Windows.Forms;

namespace MDIPaint // Убедитесь, что это пространство имен совпадает с MainForm.cs
{
    public partial class CanvasSizeForm : Form
    {
        public CanvasSizeForm()
        {
            InitializeComponent();
        }

        // Свойство для получения/установки значения ширины из NumericUpDown
        public int CanvasWidth
        {
            get { return (int)numericUpDownWidth.Value; }
            set { numericUpDownWidth.Value = value; }
        }

        // Свойство для получения/установки значения высоты из NumericUpDown
        public int CanvasHeight
        {
            get { return (int)numericUpDownHeight.Value; }
            set { numericUpDownHeight.Value = value; }
        }

        // Обработчики событий для кнопок OK и Cancel не нужны,
        // так как мы установили DialogResult для них,
        // и форма закроется автоматически с соответствующим результатом.
    }
}
