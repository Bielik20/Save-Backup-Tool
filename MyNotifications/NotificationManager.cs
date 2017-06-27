using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace SimpleNotifications
{
    public class NotificationManager
    {
        #region Properties
        public long FadingTime
        {
            get { return _fadingTime; }
            set
            {
                if (value >= 0)
                    _fadingTime = value;
                else
                    throw new System.Exception("Fading time must be gratter or equal 0.");
            }
        }
        private long _fadingTime = 5000000;

        public long DisplayTime
        {
            get { return _displayTime; }
            set
            {
                if (value > 0)
                    _displayTime = value;
                else
                    throw new System.Exception("Display time must be gratter than 0.");
            }
        }
        private long _displayTime = 6000000;


        private DisplayControl ds;
        public System.Windows.Controls.TextBlock TextBlock { get; set; }
        public System.Windows.Controls.Border WindowBorder { get; set; }
        #endregion

        public NotificationManager()
        {
            ds = new DisplayControl();
            TextBlock = ds.TextBlock;
            WindowBorder = ds.WindowBorder;
        }

        public void Show(string message)
        {
            TextBlock.Text = message;
            
            var vm = new DisplayControlViewModel(this, ds);
        }

    }
}
