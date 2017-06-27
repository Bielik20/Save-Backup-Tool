using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SimpleNotifications
{
    public class DisplayControlViewModel
    {
        Window window;
        Stages stage = Stages.Start;
        long passedTime;
        long fadingTime;
        long displayTime;

        enum Stages
        {
            Start,
            FadingIn,
            Displayed,
            FadingOut,
            End
        }

        public DisplayControlViewModel(NotificationManager notificationMenager, DisplayControl ds)
        {
            passedTime = 0;
            fadingTime = notificationMenager.FadingTime;
            displayTime = notificationMenager.DisplayTime;

            var displayControl = new DisplayControl();
            displayControl = ds;

            window = new Window
            {
                Content = displayControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                AllowsTransparency = true,
                Topmost = true,
                ShowInTaskbar = false,
                Focusable = false,
                Background = Brushes.Transparent,
            };

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(500000), DispatcherPriority.Normal, TimerTick, Dispatcher.CurrentDispatcher);
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            switch (stage)
            {
                case Stages.Start:
                    passedTime = 0;
                    stage = Stages.FadingIn;

                    window.Opacity = 0;
                    window.Show();
                    break;
                case Stages.FadingIn:
                    passedTime += 500000;
                    window.Opacity = (double)passedTime / fadingTime;

                    if (passedTime >= fadingTime)
                    {
                        passedTime = 0;
                        stage = Stages.Displayed;
                    }
                    break;
                case Stages.Displayed:
                    passedTime += 500000;

                    if (passedTime >= displayTime)
                    {
                        passedTime = 0;
                        stage = Stages.FadingOut;
                    }
                    break;
                case Stages.FadingOut:
                    passedTime += 500000;
                    window.Opacity = 1 - (double)passedTime / fadingTime;

                    if (passedTime >= fadingTime)
                    {
                        passedTime = 0;
                        stage = Stages.End;
                    }
                    break;
                case Stages.End:
                    window.Close();
                    (sender as DispatcherTimer).Stop();
                    break;
                default:
                    break;
            }
        }
    }
}
