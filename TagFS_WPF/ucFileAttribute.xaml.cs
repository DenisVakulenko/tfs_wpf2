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
    /// Interaction logic for ucAttribute.xaml
    /// </summary>
    public partial class ucFileAttribute : UserControl {
        public ucFileAttribute() {
            InitializeComponent();
        }
        public ucFileAttribute(TFSFileAttribute _FileAttribute) {
            InitializeComponent();
            FileAttribute = _FileAttribute;
        }

        public void Update() {
            if (mFileAttribute == null) return;
            if (mFileAttribute.Attr == null) return;
            lblName.Content = mFileAttribute.Attr.Name;
            txtValue.Text = mFileAttribute.Value;
        }

        protected TFSFileAttribute mFileAttribute = null;
        public TFSFileAttribute FileAttribute {
            get {
                return mFileAttribute;
            }
            set {
                mFileAttribute = value;
                txtValue.Text = mFileAttribute.Value;
                lblName.Content = mFileAttribute.Attr.Name;
            }
        }

        private void txtValue_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            mFileAttribute.Delete();
            if (Parent != null)
                ((Panel)Parent).Children.Remove(this);
        }

        private void txtValue_LostFocus(object sender, RoutedEventArgs e) {
            if (mFileAttribute == null) return;
            if (mFileAttribute.Value == txtValue.Text) return;
            mFileAttribute.Value = txtValue.Text;
            mFileAttribute.FixInBase();
            if (mFileAttribute.File != null)
                mFileAttribute.File.UpdateView();
        }

        private void txtValue_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (mFileAttribute == null) return;
                if (mFileAttribute.Value == txtValue.Text) return;
                mFileAttribute.Value = txtValue.Text;
                mFileAttribute.FixInBase();
                if (mFileAttribute.File != null)
                    mFileAttribute.File.UpdateView();
            }
        }
    }
}
