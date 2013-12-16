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
using System.Drawing;
using System.Windows.Interop;

namespace TagsFS_WPF {
    /// <summary>
    /// Interaction logic for ucFile.xaml
    /// </summary>
    public partial class ucFile : UserControl {
        public ucFile() {
            InitializeComponent();
        }
        public ucFile(TFSFile _File) {
            InitializeComponent();
            File = _File;
        }

        void mFile_Changed(object sender, TFSItem.ChangedEventArgs e) {
            Update();
        }

        public void Update() {
            pnlTags.Children.Clear();
            if (mFile == null) return;
            lblFileName.Content = mFile.DisplayName();
            if (mFile.Tags != null)
            foreach (TFSFileTag T in mFile.Tags) {
                if (T.Tag != null) {
                    //Label lbl = new Label();
                    //lbl.Margin = new Thickness(0, 0, 10, 0);
                    //lbl.Foreground = Brushes.Gray;
                    //lbl.Padding = new Thickness(0);
                    //lbl.RenderSize = lbl.DesiredSize;
                    //lbl.Content = T.Tag.FullName();
                    ucFileTag t = new ucFileTag(T);
                    pnlTags.Children.Add(t);
                }
            }
            pnlTags.Children.Add(new ucAddFileTag(mFile));

            if (System.IO.File.Exists(mFile.Path)) {
                var IH = new IconHelper();

                var ico = Icon.ExtractAssociatedIcon(mFile.Path).ToBitmap();
                BitmapImage myBitmapImage = new BitmapImage();


                BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(ico.GetHbitmap(),
                                                                       IntPtr.Zero,
                                                                       Int32Rect.Empty,
                                                                       BitmapSizeOptions.FromEmptyOptions());

                imgIco.Source = i;
            }
        }


        protected TFSFile mFile = null;
        public TFSFile File {
            get {
                return mFile;
            }
            set {
                if (mFile != null)
                    mFile.Changed -= mFile_Changed;
                mFile = value;
                mFile.Changed += mFile_Changed;
                Update();
            }
        }

        private void UserControl_LostFocus_1(object sender, RoutedEventArgs e) {
            Background = System.Windows.Media.Brushes.Transparent;
        }
        private void UserControl_GotFocus_1(object sender, RoutedEventArgs e) {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        }

        private void UserControl_MouseDown_1(object sender, MouseButtonEventArgs e) {
            Focus();
        }

        private void lblFileName_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start(mFile.Path);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            mFile.Delete();
            ((StackPanel)Parent).Children.Remove(this);
        }

    }
}
