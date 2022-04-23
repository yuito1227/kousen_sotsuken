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
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace blocktemplate
{
    /// <summary>
    /// progress_bar.xaml の相互作用ロジック
    /// </summary>
    public partial class progress_bar : Window
    {
        public progress_bar()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;

            /*
             * 表示位置(Top)を調整。
             * 「ディスプレイの作業領域の高さ」-「表示するWindowの高さ」
             */
            this.Top = 100;

            /*
             * 表示位置(Left)を調整
             * 「ディスプレイの作業領域の幅」-「表示するWindowの幅」
             */
            this.Left = 100;
        }

    }
}
