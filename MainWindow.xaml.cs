using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.WPF;
using System;
using System.Timers;


namespace SinusTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double[] data;
        public int Amp;
        public int Freq;
        private int i;
        double point = 0;
        WpfPlot plots = new WpfPlot();
        Thread myThread;
        Thread myThread1;
        private SignalPlot signalPlot;
        bool interrupt1;
        bool interrupt2;
        private object Locker;
        private Application app = Application.Current;
        private ManualResetEvent exitEvent = new ManualResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();
            myThread = new Thread(myThreadGenerateMethod) {  IsBackground = true };
            myThread1 = new Thread(myThread1ReadMethod) { IsBackground = true };
            Locker = new object();
            myThread.Start();
            myThread1.Start();
            interrupt1 = false;

            this.Closing += Window_Closing;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    Application.Current.Shutdown();
                }
            }

            var plot = plots.Plot;
            Grid.SetColumn(plots, 0);
            plot.Margins(0, 0, 0, 0);
            plot.Width = 500;
            Grid.SetColumnSpan(plots, 2);

            MyGrid.Children.Add(plots);
            data = new double[1000];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
            plots.Plot.AddSignal(data);
        }
        //метод генерации точек
        void myThreadGenerateMethod()
        {
            while (true)
            {
                while(!exitEvent.WaitOne(0))
                {
                    while (interrupt1)
                    {
                        point = (Amp * Math.Sin(2 * Math.PI * Freq * (double)i / 1000)); //отрисовка синуса
                        lock (Locker) //мьютекс
                        {
                            Push(data, point);
                        }
                        i++;
                        Thread.Sleep(2);

                    }
                    Thread.Sleep(1);
                    
                }
                
            }

        }
        //метод отрисовки 
        void myThread1ReadMethod()
        {
            while (true)
            {
                while (!exitEvent.WaitOne(0))
                {
                    while (interrupt1)
                    {
                        try
                        {
                            Dispatcher.Invoke(() =>
                            {

                                plots.Plot.AxisAuto();//автомасштабирование
                                plots.Refresh();

                            });
                        }
                        catch { }
                        Thread.Sleep(1);
                    }

                    Thread.Sleep(1);
                    
                }
                
            }

        }

        private double[] Push(double[] arr, double j)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = j;

            return arr;
        }
        //Поле для ввода амплитуды
        private void AmpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string temp = AmpTextBox.Text;
            if (temp.Length == 0)
            {
                return;
            }
            int i = int.Parse(temp);
            if (i > 0)
            {
                Amp = i;
            }
        }
        // Поле для ввода Частоты
        private void FreqTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string temp = FreqTextBox.Text;
            if (temp.Length == 0)
            {
                return;
            }
            int i = int.Parse(temp);
            if (i > 0)
            {
                Freq = i;
            }

        }
        //Кнопка Запуск
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            interrupt1 = true;
        }
        //Кнопка СТОП
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            interrupt1 = false;
        }
        //Закрытие окна
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //Закрытие приложения
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            interrupt1 = false;
            exitEvent.Set();
        }
      
        //кнопка выход
        private void Window_Closing_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}