using System.Text;
using System.Windows;
using System.IO;

namespace blocktemplate
{
    /// <summary>
    /// bluetooth_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class key_setting : Window
    {
        public key_setting()
        {
            InitializeComponent();
            StreamReader device_name_file = new StreamReader("API_key.txt", Encoding.GetEncoding("Shift_JIS"));
            string device_name = device_name_file.ReadToEnd();
            device_name_file.Close();
            DeviceName.Text = device_name.Split(',')[0];
        }

        private void confirm(object sender, RoutedEventArgs e)
        {
            string device_name = DeviceName.Text + ",";
            StreamWriter device_name_file = new StreamWriter("API_key.txt", false, Encoding.GetEncoding("Shift_JIS"));
            device_name_file.WriteLine(device_name);
            device_name_file.Close();
            this.Close();
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
