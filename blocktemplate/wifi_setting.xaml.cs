using System.Text;
using System.Windows;
using System.IO;

namespace blocktemplate
{
    /// <summary>
    /// wifi_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class wifi_setting : Window
    {
        public wifi_setting()
        {
            InitializeComponent();
            StreamReader setting_file = new StreamReader("wifi_setting.txt", Encoding.GetEncoding("Shift_JIS"));
            string setting = setting_file.ReadToEnd();
            setting_file.Close();
            SSID.Text = setting.Split(',')[0];
            Password.Password = setting.Split(',')[1];

        }

        private void confirm(object sender, RoutedEventArgs e)
        {
            string setting = SSID.Text +"," + Password.Password + ",";
            StreamWriter setting_file = new StreamWriter("wifi_setting.txt", false, Encoding.GetEncoding("Shift_JIS"));
            setting_file.WriteLine(setting);
            setting_file.Close();
            this.Close();
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
