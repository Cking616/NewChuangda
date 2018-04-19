using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using Microsoft.Win32;
using NewChuangda;

namespace NcdPacker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskManager manager;
        public MainWindow()
        {
            InitializeComponent();
            manager = new TaskManager();
            manager.Initialize("init.lua");

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            timer.Tick += OnHignTimer;
            timer.IsEnabled = true;

            DispatcherTimer timer1 = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            timer1.Tick += OnTimer;
            timer1.IsEnabled = true;
        }

        public void IrSend_Click(object sender, RoutedEventArgs e)
        {
            String S = IrSendTextBox.Text;
            if (S == "")
            {
                return;
            }

            manager.DoString(S);

            IrSendTextBox.Text = "";
        }

        public void IrFilePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "文本文件|*.lua"
            };
            if (dialog.ShowDialog() == true)
            {
                IrScriptFilePath.Text = dialog.FileName;
            }
        }

        public void IrSendFile_Click(object sender, RoutedEventArgs e)
        {
            string FilePath = IrScriptFilePath.Text;
            if (File.Exists(FilePath))
            {
                manager.DoScript(FilePath);
            }
            else
            {
                MessageBox.Show("文件不存在，请选择正确的文件然后重试");
            }
        }

        public void OnHignTimer(Object state, EventArgs e)
        {
            manager.OnHighTimer();
        }

        public void OnTimer(Object state, EventArgs e)
        {
            manager.OnTimer();
        }
    }
}
