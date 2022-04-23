using System.Windows;
using System.Windows.Controls;
namespace blocktemplate
{
    /// <summary>
    /// show_source_code.xaml の相互作用ロジック
    /// </summary>
    public partial class show_source_code : Window
    {
        public show_source_code()
        {
            InitializeComponent();
        }

        private void LockLogButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public string source_code
        {
            get
            {
                return TextBlock.Text;
            }
            set
            {
                TextBlock.Text = value;
            }
        }
    }
}
