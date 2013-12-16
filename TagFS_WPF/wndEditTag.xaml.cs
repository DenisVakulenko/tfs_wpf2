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

namespace TagsFS_WPF {
    /// <summary>
    /// Interaction logic for wndEditTag.xaml
    /// </summary>
    public partial class wndEditTag : Window {
        public wndEditTag() {
            InitializeComponent();
        }
        public wndEditTag(TFSTag _Tag) {
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

                pnlFileAttributes.Children.Clear();

                TFSTag t = mTag;
                t.Changed += t_Changed;
                pnlFileAttributes.Children.Add(new ucTag(t));
                while (t.ParentTag != null) {
                    t = t.ParentTag;
                    t.Changed += t_Changed;
                    pnlFileAttributes.Children.Insert(0, new ucTag(t));
                }

                
            }
        }

        void t_Changed(object sender, TFSItem.ChangedEventArgs e) {
            if (mTag.ID != -1)
                Tag = mTag;
            else {
                while (mTag.ParentTag != null && mTag.ID == -1)
                    Tag = mTag.ParentTag;
                if (Tag.ID == -1)
                    Close();
            }
            mTag.DB.mTagsActual = false;
            mTag.DB.LoadAllTags();

            Tag = mTag.DB.mTags[Tag.ID];
        }

    }
}
