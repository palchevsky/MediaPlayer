using MediaPlayer.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer _timer;
        TimeSpan _totalTime;
        readonly List<FileNames> _files;
        bool _isDraggingSlider;
        private bool _fullscreen;
        private readonly DispatcherTimer _doubleClickTimer;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1)
            };
            _timer.Tick += Timer_Tick;
            _files = new List<FileNames>();
            _isDraggingSlider = false;
            lbListOfFiles.MouseDoubleClick += LbListOfFiles_MouseDoubleClick;
            _doubleClickTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(GetDoubleClickTime())};
            _doubleClickTimer.Tick += (s, e) => _doubleClickTimer.Stop();
        }

        private void MeVideo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_doubleClickTimer.IsEnabled)
            {
                _doubleClickTimer.Start();
            }
            else
            {
                if (!_fullscreen)
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
                _fullscreen = !_fullscreen;
            }
        }

        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (meVideo.Source == null || !meVideo.NaturalDuration.HasTimeSpan || _isDraggingSlider) return;
            slControl.Minimum = 0;
            slControl.Maximum = meVideo.NaturalDuration.TimeSpan.TotalSeconds;
            slControl.Value = meVideo.Position.TotalSeconds;
        }

        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter =
                    "All Files(*.*)|*.*|Форматы Windows Media|*.WMA;*.WMV|Audio Visual Interleave|*.AVI|Moving Pictures Experts Group|*.MPG;*.MPEG;*.MP3;*.MP4|Аудио Windows|*.WAV",
                FilterIndex = 3
            };

            try
            {
                if (openFileDialog.ShowDialog().Value)
                {
                    meVideo.Source = new Uri(openFileDialog.FileName);

                    if (_files.Count != 0 && lbListOfFiles.Items.Count != 0)
                    {
                        _files.Clear();
                        lbListOfFiles.Items.Clear();
                    }

                    for (var i = 0; i < openFileDialog.FileNames.Length; i++)
                    {
                        _files.Add(new FileNames(openFileDialog.SafeFileNames[i], openFileDialog.FileNames[i]));
                        lbListOfFiles.Items.Add(new FileNames(openFileDialog.SafeFileNames[i], openFileDialog.FileNames[i]));
                    }
                    lbListOfFiles.SelectedItem = lbListOfFiles.Items[0];
                    BtPlay_Click(new object(), new RoutedEventArgs());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void BtPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!meVideo.IsLoaded) return;
            meVideo.Play();
            _timer.Start();
        }

        private void BtStop_Click(object sender, RoutedEventArgs e)
        {
            if (!meVideo.IsLoaded) return;
            meVideo.Stop();
            _timer.Stop();
            slControl.Value = 0;
        }

        private void SlControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbLeft.Content = meVideo.IsLoaded ? TimeSpan.FromSeconds(slControl.Value).ToString(@"hh\:mm\:ss") : "00:00:00";
        }

        private void MeVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            _totalTime = meVideo.NaturalDuration.TimeSpan;
            lbTotal.Content = $"{_totalTime.Hours:00}:{_totalTime.Minutes:00}:{_totalTime.Seconds:00}";
            slVolume.Value = meVideo.Volume;
        }

        private void MeVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (lbListOfFiles.SelectedIndex+1<lbListOfFiles.Items.Count)
            {
                lbListOfFiles.SelectedItem = lbListOfFiles.Items[lbListOfFiles.SelectedIndex + 1];
                meVideo.Source = new Uri(_files[lbListOfFiles.SelectedIndex].FullName);
                BtPlay_Click(new object(), new RoutedEventArgs());
            }
            else
            {
                lbListOfFiles.SelectedIndex = 0;
                lbListOfFiles.SelectedItem = lbListOfFiles.Items[lbListOfFiles.SelectedIndex];
                meVideo.Source = new Uri(_files[lbListOfFiles.SelectedIndex].FullName);
                BtPlay_Click(new object(), new RoutedEventArgs());
            }
        }

        private void SlControl_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDraggingSlider = true;
        }

        private void SlControl_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDraggingSlider = false;
            meVideo.Position = TimeSpan.FromSeconds(slControl.Value);
        }

        private void BtPause_Click(object sender, RoutedEventArgs e)
        {
            meVideo.Pause();
        }

        private void LbListOfFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            meVideo.Stop();
            if (lbListOfFiles.SelectedItem != null)
            {
                meVideo.Source = new Uri(_files[lbListOfFiles.SelectedIndex].FullName);
                BtPlay_Click(new object(), new RoutedEventArgs());
            }
        }

        private void SlVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            meVideo.Volume = slVolume.Value;
        }
    }
}
