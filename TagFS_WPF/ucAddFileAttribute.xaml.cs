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
    /// Interaction logic for ucAddFileAttribute.xaml
    /// </summary>
    public partial class ucAddFileAttribute : UserControl {
        public ucAddFileAttribute() {
            InitializeComponent();
        }

        protected TFSFile mFile;
        public TFSFile File {
            get {
                return mFile;
            }
            set {
                if (mFile == null || (mFile.DB != null && mFile.DB != value.DB)) {
                    cmbAttr.Items.Clear();
                    if (value.DB == null) return;
                    foreach (TFSAttr Attr in value.DB.mAttrs) {
                        cmbAttr.Items.Add(Attr.Name);
                    }
                }
                mFile = value;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (cmbAttr.SelectedIndex >= 0) {
                TFSAttr Attr = mFile.DB.mAttrs[cmbAttr.SelectedIndex];
                TFSFileAttribute FileAttr = new TFSFileAttribute(mFile, Attr, txtValue.Text, 1, mFile.DB);

                mFile.Attrs.Add(FileAttr);
                FileAttr.FixInBase();
                mFile.UpdateView();

                txtValue.Text = "";
            }
            else {
                String strAttr = "";
                String value = "";
                int pos;
                if ((pos = txtValue.Text.IndexOf(":")) > -1) {
                    strAttr = txtValue.Text.Substring(0, pos);
                    value = txtValue.Text.Substring(pos + 1);
                }
                else if ((pos = txtValue.Text.IndexOf("-")) > -1) {
                    strAttr = txtValue.Text.Substring(0, pos);
                    value = txtValue.Text.Substring(pos + 1);
                }
                else if ((pos = txtValue.Text.IndexOf("=")) > -1) {
                    strAttr = txtValue.Text.Substring(0, pos);
                    value = txtValue.Text.Substring(pos + 1);
                }

                strAttr = strAttr.ToLower().Trim();
                value = value.Trim();

                TFSAttr Attr = null;
                foreach (TFSAttr a in mFile.DB.mAttrs)
                    if (a.Name.ToLower() == strAttr)
                        Attr = a;

                if (Attr != null) {
                    TFSFileAttribute FileAttr = new TFSFileAttribute(mFile, Attr, value, 1, mFile.DB);

                    mFile.Attrs.Add(FileAttr);
                    FileAttr.FixInBase();
                    mFile.UpdateView();

                    txtValue.Text = "";
                }
                else if (strAttr != "") {
                    Attr = mFile.DB.AddAttribute(strAttr);
                    if (Attr == null) return;

                    TFSFileAttribute FileAttr = new TFSFileAttribute(mFile, Attr, value, 1, mFile.DB);

                    mFile.Attrs.Add(FileAttr);
                    FileAttr.FixInBase();
                    mFile.UpdateView();

                    txtValue.Text = "";
                }
            }
        }

        private void txtValue_TextChanged(object sender, TextChangedEventArgs e) {

        }
    }
}
