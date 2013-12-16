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
    /// Interaction logic for ucTag.xaml
    /// </summary>
    public partial class ucTag : UserControl {
        public ucTag() {
            InitializeComponent();
        }
        public ucTag(TFSTag _Tag) {
            InitializeComponent();
            Tag = _Tag;
        }

        protected TFSTag mTag;
        public TFSTag Tag {
            get {
                return mTag;
            }
            set {
                mTag = value;
                txtName.Text = mTag.Name;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            var r = mTag.DB.mTags.Values;
            foreach (var t in r)
                if (t.ParentTag == mTag) {
                    t.ParentTag = mTag.ParentTag;
                    t.ParentID = mTag.ParentID;
                    t.FixInBase(false);
                }

            mTag.Delete();
            mTag.UpdateView();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var tt = mTag.DB.FindAllTagChildren(mTag);
            
            foreach (var t in tt)
                t.Delete();

            mTag.UpdateView();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e) {
            mTag.Name = txtName.Text;
            mTag.UpdateInBase();
        }
    }
}
