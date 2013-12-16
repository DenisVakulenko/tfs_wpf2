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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TagsFS_WPF {
    /// <summary>
    /// Interaction logic for ucFileTag.xaml
    /// </summary>
    public partial class ucFileTag : UserControl {
        public ucFileTag() {
            InitializeComponent();
        }
        public ucFileTag(TFSFileTag _FileTag) {
            InitializeComponent();
            FileTag = _FileTag;
        }

        protected TFSFileTag mFileTag;
        public TFSFileTag FileTag {
            get {
                return mFileTag;
            }
            set {
                mFileTag = value;
                lblName.Content = mFileTag.Tag.FullName();
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e) {
            mFileTag.Delete();
        }

        private void lblName_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var m = new wndEditTag(mFileTag.Tag);
            m.ShowDialog();
            mFileTag.File.UpdateView();
        }
    }
}
