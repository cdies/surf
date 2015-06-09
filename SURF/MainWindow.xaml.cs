using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SURF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Image<Bgr, Byte> SURF_image;
        Image[] img;
        Button[] btn;
        TextBox[] txt;
        TextBox[] txt_class;
        Button[] btn_clear;
        Image<Bgr, Byte>[] class_image;
        public MainWindow()
        {
            InitializeComponent();

            img = new Image[] { img_1, img_2, img_3, img_4, img_5 };
            btn = new Button[] { btn_1, btn_2, btn_3, btn_4, btn_5 };
            txt = new TextBox[] { txt_1, txt_2, txt_3, txt_4, txt_5 };
            txt_class = new TextBox[] { txt_class_1, txt_class_2, txt_class_3, txt_class_4, txt_class_5 };
            btn_clear = new Button[] { btn_clear_1, btn_clear_2, btn_clear_3, btn_clear_4, btn_clear_5 };
            class_image = new Image<Bgr, byte>[5];
        }

        #region  вкладка "SURF детектор"
        // Добавление изображения для SURF детектора
        private void btn_add_surf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                txt_add_surf.Text = openFileDialog.FileName;
            SURF_image = new Image<Bgr, byte>(txt_add_surf.Text);
            img_SURF.Source = BitmapSourceConvert.ToBitmapSource(SURF_image);
        }
        // Определение локальных особенностей
        private void btn_surf_Click(object sender, RoutedEventArgs e)
        {
            if (SURF_image == null)
                MessageBox.Show("Укажите изображение!");
            else
                img_SURF.Source = BitmapSourceConvert.ToBitmapSource(DrawMatches.SingleDraw(SURF_image));                
        }
        // Очистка локальных особенностей
        private void btn_clear_surf_Click(object sender, RoutedEventArgs e)
        {
            if (SURF_image == null)
                MessageBox.Show("Укажите изображение!");
            else
                img_SURF.Source = BitmapSourceConvert.ToBitmapSource(SURF_image);
        }
        #endregion

        #region  вкладка "Обучающая выборка"
        // Добавление изображений для обучающей выборки
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            int n = 0;
            switch ((sender as Button).Name)
            {
                case "btn_1":
                    n = 0; break;
                case "btn_2":
                    n = 1; break;
                case "btn_3":
                    n = 2; break;
                case "btn_4":
                    n = 3; break;
                case "btn_5":
                    n = 4; break;
                default:
                    break;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                txt[n].Text = openFileDialog.FileName;
            class_image[n] = new Image<Bgr, byte>(txt[n].Text);
            img[n].Source = BitmapSourceConvert.ToBitmapSource(class_image[n]);

        }
        // Удаление информации из обучающей выборки
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            int n = 0;
            switch ((sender as Button).Name)
            {
                case "btn_clear_1":
                    n = 0; break;
                case "btn_clear_2":
                    n = 1; break;
                case "btn_clear_3":
                    n = 2; break;
                case "btn_clear_4":
                    n = 3; break;
                case "btn_clear_5":
                    n = 4; break;
                default:
                    break;
            }
            img[n].Source = null;
            txt[n].Text = "";
            txt_class[n].Text = Convert.ToString(n + 1);
            class_image[n] = null;
        }
        #endregion
    }
}
