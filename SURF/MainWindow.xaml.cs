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
        public MainWindow()
        {
            InitializeComponent();
        }
        ////////////////////////////////////////////////////////////////////////// вкладка SURF детектор
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
            img_SURF.Source = BitmapSourceConvert.ToBitmapSource(SURF_image);
        }
        /////////////////////////////////////////////////////////////////////////////////////////


    }
}
