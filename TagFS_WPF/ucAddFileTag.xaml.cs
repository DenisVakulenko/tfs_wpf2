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
    /// Interaction logic for ucAddFileTag.xaml
    /// </summary>
    public partial class ucAddFileTag : UserControl {
        public ucAddFileTag() {
            InitializeComponent();
        }
        public ucAddFileTag(TFSFile _File) {
            InitializeComponent();
            File = _File;
        }

        protected TFSFile mFile;
        public TFSFile File {
            get {
                return mFile;
            }
            set {
                if (value == null) { mFile = value; return; }
                if (mFile == null || (mFile.DB != null && mFile.DB != value.DB)) {
                    List<String> Tags = new List<string>();
                    if (value.DB == null) return;
                    foreach (var Attr in value.DB.mTags) {
                        Tags.Add(Attr.Value.FullName());
                    }
                    txtTag.ItemsSource = Tags;
                    //new AutoCompleteBox;
                    //cmbAttr.Items.Clear();
                    //if (value.DB == null) return;
                    //foreach (TFSAttr Attr in value.DB.mAttrs) {
                    //    cmbAttr.Items.Add(Attr.Name);
                    //}
                }
                mFile = value;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            TFSFileTag result = null;
            
            foreach (var Tag in mFile.DB.mTags)
                if (Tag.Value.FullName().ToLower() == txtTag.Text.ToLower()) {
                    result = new TFSFileTag(mFile, Tag.Value, 1, mFile.DB);
                    break;
                }

            if (result == null) {
                var NewTag = mFile.DB.AddTag(txtTag.Text);
                if (NewTag == null) return;

                result = new TFSFileTag(mFile, NewTag, 1, mFile.DB);
            }

            mFile.AddTag(result);
            result.FixInBase();
            mFile.UpdateView();

            txtTag.Text = "";
        }

        private void txtTag_KeyDown(object sender, KeyEventArgs e) {

        }

        private void txtTag_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                btnAdd_Click(sender, e);
        }

        private void txtTag_LostFocus(object sender, RoutedEventArgs e) {
            txtTag.Background = Brushes.Transparent;
        }

        private void txtTag_GotFocus(object sender, RoutedEventArgs e) {
            txtTag.Background = Brushes.White;
        }
    }
}
