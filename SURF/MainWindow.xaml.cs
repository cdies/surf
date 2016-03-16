using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace SURF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Image<Bgr, Byte> SURF_image; // Изображение для поиска

        Image[] img; // Ссылки на все изображения вкладки "Визуальные объекты для поиска"
        Button[] btn; // Ссылки на все кнопки для выбора изображений (...) вкладки "Визуальные объекты для поиска"
        TextBox[] txt; // Ссылки на элементы, отображающие путь вкладки "Визуальные объекты для поиска"
        TextBox[] txt_class; // Ссылки на названия элементов (классов) вкладки "Визуальные объекты для поиска"
        Button[] btn_clear; // Ссылки на кнопки очистки вкладки "Визуальные объекты для поиска"
        Image<Bgr, Byte>[] class_image; // Массив визуальных объектов для поиска на SURF_image
        Thread camera_thread;

        Image[] img_class_result; // отображает гомографию(совпадение) (вкладка "Результаты")
        public MainWindow()
        {
            InitializeComponent();

            img = new Image[] { img_1, img_2, img_3, img_4, img_5 };
            btn = new Button[] { btn_1, btn_2, btn_3, btn_4, btn_5 };
            txt = new TextBox[] { txt_1, txt_2, txt_3, txt_4, txt_5 };
            txt_class = new TextBox[] { txt_class_1, txt_class_2, txt_class_3, txt_class_4, txt_class_5 };
            btn_clear = new Button[] { btn_clear_1, btn_clear_2, btn_clear_3, btn_clear_4, btn_clear_5 };
            class_image = new Image<Bgr, byte>[5];
            img_class_result = new Image[] { img_result_1, img_result_2, img_result_3, img_result_4, img_result_5 };
        }

        #region  вкладка "SURF детектор"
        // Добавление изображения для SURF детектора
        private void btn_add_surf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txt_add_surf.Text = openFileDialog.FileName;
                SURF_image = new Image<Bgr, byte>(txt_add_surf.Text);
                img_SURF.Source = BitmapSourceConvert.ToBitmapSource(SURF_image);
            }
        }
        // Определение локальных особенностей
        private void btn_surf_Click(object sender, RoutedEventArgs e)
        {
            if (SURF_image == null)
                MessageBox.Show("Укажите изображение!");
            else
            {
                SURF_image = new Image<Bgr, byte>(txt_add_surf.Text);
                img_SURF.Source = BitmapSourceConvert.ToBitmapSource(DrawMatches.SingleDraw(SURF_image));
            }
        }
        // Очистка локальных особенностей
        private void btn_clear_surf_Click(object sender, RoutedEventArgs e)
        {
            if (SURF_image == null)
                MessageBox.Show("Укажите изображение!");
            else
            {
                SURF_image = new Image<Bgr, byte>(txt_add_surf.Text);
                img_SURF.Source = BitmapSourceConvert.ToBitmapSource(SURF_image);
            }
        }
        #endregion

        #region  вкладка "Визуальные объекты для поиска"
        // Добавление визуальных объектов для поиска
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
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


                txt[n].Text = openFileDialog.FileName;
                class_image[n] = new Image<Bgr, byte>(txt[n].Text);
                img[n].Source = BitmapSourceConvert.ToBitmapSource(class_image[n]);
                img_class_result[n].Source = BitmapSourceConvert.ToBitmapSource(class_image[n]);
            }
        }
        // Удаление информации из выборки
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
            img_class_result[n].Source = null;
            txt[n].Text = "";
            txt_class[n].Text = Convert.ToString(n + 1);
            class_image[n] = null;
        }
        #endregion

        #region  вкладка "Результаты"
        // Отображение гомографии и гистограммы нейронной сети
        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (img_result.Source == null)
                MessageBox.Show("Результат не расчитан (нажмите кнопку \"Отобразить результаты\")");
            else
            {
                int n = 0;
                switch ((sender as Image).Name)
                {
                    case "img_result_1":
                        n = 0; break;
                    case "img_result_2":
                        n = 1; break;
                    case "img_result_3":
                        n = 2; break;
                    case "img_result_4":
                        n = 3; break;
                    case "img_result_5":
                        n = 4; break;
                    default:
                        break;
                }
                Image<Bgr, Byte> temp = new Image<Bgr, byte>(txt_add_surf.Text);
                Image<Bgr, Byte> result = DrawMatches.DrawWithHomography(class_image[n], temp, SURF_image);
                img_result.Source = BitmapSourceConvert.ToBitmapSource(result);
            }
        }
        // Кнопка отобразить результаты
        private void btn_result_Click(object sender, RoutedEventArgs e)
        {
            if (SURF_image == null)
                MessageBox.Show("Не загружено основное SURF изображение! (вкладка \"SURF детектор\")");
            else
            {
                SURF_image = new Image<Bgr, byte>(txt_add_surf.Text);
                IColor[] colors = { new Bgr(System.Drawing.Color.Red), new Bgr(System.Drawing.Color.Green),
                                 new Bgr(System.Drawing.Color.Blue), new Bgr(System.Drawing.Color.Orange),
                                 new Bgr(System.Drawing.Color.Violet)};
                string[] names = { txt_class_1.Text, txt_class_2.Text, txt_class_3.Text,
                                     txt_class_4.Text, txt_class_5.Text };
                SURF_image = DrawMatches.Draw(class_image, SURF_image, names, colors);
                img_result.Source = BitmapSourceConvert.ToBitmapSource(SURF_image);
            }
        }
        // Кнопка очистить
        private void btn_result_clear_Click(object sender, RoutedEventArgs e)
        {
            img_result.Source = null;
        }
        #endregion

        #region  вкладка "Web Камера"
        // Кнопка Start
        private void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            string[] names = { txt_class_1.Text, txt_class_2.Text, txt_class_3.Text,
                                     txt_class_4.Text, txt_class_5.Text };
            camera_thread = new Thread(new ParameterizedThreadStart(Video));
            camera_thread.Start(names);
            Start_btn.IsEnabled = false;
        }
        // Кнопка Stop
        private void Stop_btn_Click(object sender, RoutedEventArgs e)
        {
            if (camera_thread != null)
                if (camera_thread.IsAlive)
                    camera_thread.Abort();
            Start_btn.IsEnabled = true;
        }

        private void Video(object o)
        {
            try
            {
                using (Capture capture = new Capture())
                {
                    IColor[] colors = { new Bgr(System.Drawing.Color.Red), new Bgr(System.Drawing.Color.Green),
                                 new Bgr(System.Drawing.Color.Blue), new Bgr(System.Drawing.Color.Orange),
                                 new Bgr(System.Drawing.Color.Violet)};
                    string[] names = (string[])o;
                    Image<Bgr, byte> result;

                    capture.Start();

                    while (true)
                    {
                        result = capture.QueryFrame();
                        if (result != null)
                        {
                            try
                            {
                                result = DrawMatches.Draw(class_image, result, names, colors);

                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                                {
                                    img_Camera.Source = BitmapSourceConvert.ToBitmapSource(result);
                                });
                            }
                            catch { continue; }
                            Thread.Sleep(200);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Не работает камера!");
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    Start_btn.IsEnabled = true;
                });
                return;
            }
        }
        #endregion

        // Удалить поток камеры при закрытии
        private void Window_Closed(object sender, EventArgs e)
        {
            if (camera_thread.IsAlive)
                camera_thread.Abort();
        }
    }
}
