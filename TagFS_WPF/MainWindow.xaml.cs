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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            txtSearch.Focus();
        }

        private String OneWordLike(String colname, String prm) {
            String res = "(";

            colname += " LIKE ";

            res += colname +  "'% " + prm + " %' OR ";
            res += colname +  "'% " + prm + "' OR ";
            res += colname +    "'" + prm + " %' OR ";

            res += colname +  "'% " + prm + "/_%' ESCAPE '/' OR ";
            res += colname + "'%/_" + prm + " %' ESCAPE '/' OR ";

            res += colname + "'%/_" + prm + "/_%' ESCAPE '/' OR ";
            res += colname + "'%/_" + prm + "' ESCAPE '/' OR ";
            res += colname +    "'" + prm + "/_%' ESCAPE '/' OR ";

            res += colname +    "'" + prm + "')";

            return res;
        }

        private String BeginingWordLike(String colname, String prm) {
            String res = "(";

            colname += " LIKE ";

            res += colname + "'%/_" + prm + "%' ESCAPE '/' OR ";
            res += colname + "'% " + prm + "%' OR ";
            res += colname + "'" + prm + "%')";

            return res;
        }

        private String InWordLike(String colname, String prm) {
            return "(" + colname + " LIKE '%" + prm + "%')";
        }
        private String WordLike(String colname, String prm) {
            return "(" + colname + " LIKE '" + prm + "')";
        }

        private List<TFSTag> GetTagsChildren(List<TFSTag> Tags) {
            List<TFSTag> TagsChildren = new List<TFSTag>();
            if (Tags != null && Tags.Count > 0)
                foreach (TFSTag Tag in Tags) {
                    var tc = mTagsDB.FindAllTagChildren(Tag);
                    TagsChildren.AddRange(tc);
                }
            return TagsChildren;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            if (txtSearch.Text == "") return;

            Title = "Обновление списков тегов и атрибутов ...";
            
            mTagsDB.LoadAllTags();
            mTagsDB.LoadAllAttrs();

            Title = "Поиск " + txtSearch.Text + " ...";

            String tagswhere = "";
            String attrswhere = "";
            String tagswhere1 = "";
            String attrswhere1 = "";
            String tagswhere2 = "";
            String attrswhere2 = "";
            String tagswhere3 = "";
            String attrswhere3 = "";

            var prms = txtSearch.Text.Split(' ');

            foreach (var prm in prms) {
                if (prm != "") {
                    tagswhere += WordLike("[name]", prm) + "  OR ";
                    attrswhere += WordLike("[value]", prm) + "  OR ";

                    tagswhere1  += OneWordLike("[name]", prm) + "  OR ";
                    attrswhere1 += OneWordLike("[value]", prm) + "  OR ";

                    tagswhere2 += BeginingWordLike("[name]", prm) + "  OR ";
                    attrswhere2 += BeginingWordLike("[value]", prm) + "  OR ";

                    tagswhere3 += InWordLike("[name]", prm) + "  OR ";
                    attrswhere3 += InWordLike("[value]", prm) + "  OR ";
                }
            }
            tagswhere = tagswhere.Substring(0, tagswhere.Length - 5);
            attrswhere = attrswhere.Substring(0, attrswhere.Length - 5);

            tagswhere1  = tagswhere1.Substring(0, tagswhere1.Length - 5);
            attrswhere1 = attrswhere1.Substring(0, attrswhere1.Length - 5);

            tagswhere2  = tagswhere2.Substring(0, tagswhere2.Length - 5);
            attrswhere2 = attrswhere2.Substring(0, attrswhere2.Length - 5);

            tagswhere3  = tagswhere3.Substring(0, tagswhere3.Length - 5);
            attrswhere3 = attrswhere3.Substring(0, attrswhere3.Length - 5);


            String txt = "";

            List<TFSTag> Tags  = GetTagsChildren(mTagsDB.FindTagsWhere(tagswhere));
            List<TFSTag> Tags1 = GetTagsChildren(mTagsDB.FindTagsWhere(tagswhere1));
            List<TFSTag> Tags2 = GetTagsChildren(mTagsDB.FindTagsWhere(tagswhere2));
            List<TFSTag> Tags3 = GetTagsChildren(mTagsDB.FindTagsWhere(tagswhere3));

            Title = "Совпадений с тегами: " + Tags.Count() + ", " + Tags1.Count() + ", " + Tags2.Count() + ", " + Tags3.Count();
            txt += "Совпадений с тегами: " + Tags.Count();
            if (Tags.Count < 10) {
                txt += ": ";
                foreach (TFSTag Tag in Tags)
                    txt += Tag.FullName() + ", ";
                txt = txt.Substring(0, txt.Length - 2);
            }
            txt += "; " + Tags1.Count();
            if (Tags1.Count < 10 && Tags.Count < 2 && Tags1.Count > Tags.Count) {
                txt += ": ";
                foreach (TFSTag Tag in Tags1)
                    txt += Tag.FullName() + ", ";
                txt = txt.Substring(0, txt.Length - 2);
            }
            txt += "; " + Tags2.Count();
            if (Tags2.Count < 10 && Tags1.Count < 2 && Tags2.Count > Tags1.Count) {
                txt += ": ";
                foreach (TFSTag Tag in Tags2)
                    txt += Tag.FullName() + ", ";
                txt = txt.Substring(0, txt.Length - 2);
            }
            txt += "; " + Tags3.Count() + ".";




            List<TFSFileAttribute> Attrs = mTagsDB.FindFileAttrsWhere(attrswhere);

            lblInfo.Text = txt;

            var Files = mTagsDB.FindFiles(Tags, attrswhere, Tags1, attrswhere1, Tags2, attrswhere2, Tags3, attrswhere3); //Like("%" + txtSearch.Text + "%");
            
            if (Files == null) return;
            
            if (Files.Count > 50) Files = Files.GetRange(0, 50);
            
            Title = "Заполнение атрибутов найденных файлов ...";
            mTagsDB.FillFilesAttributes(Files);
            Title = "Заполнение списка тегов найденных файлов ...";
            mTagsDB.FillFilesTags(Files);
            
            Title = "Создание списка ...";

            pnlFiles.Children.Clear();
            foreach (TFSFile File in Files) {
                ucFile ucF = new ucFile(File);
                ucF.GotFocus += ucF_GotFocus;
                pnlFiles.Children.Add(ucF);
            }

            Title = txtSearch.Text;

            //lblInfo.RenderSize = lblInfo.DesiredSize;


            //if (Tags != null)
            //if (Tags.Count > 0) {
            //    txt = "Поиск файлов с тегами: ";
            //    foreach (TFSTag Tag in Tags) {
            //        txt += Tag.FullName();

            //        var tc = mTagsDB.FindAllTagChildren(Tag);
            //        TagsChildren.AddRange(tc);
            //        if (tc != null && tc.Count > 0) {
            //            AllTags.Add(tc);
            //            if (tc.Count > 1) {
            //                txt += " (";
            //                foreach (TFSTag tcc in tc.GetRange(1, tc.Count - 1))
            //                    txt += tcc.Name + ", ";
            //                txt = txt.Substring(0, txt.Length - 2) + ")";
            //            }
            //        }
            //        txt += ", ";
            //    }
            //    txt = txt.Substring(0, txt.Length - 2) + ".";
            //}
            //Tags = TagsChildren;


            //var Attrs = mTagsDB.FindFileAttrsWhere(attrswhere);
            //List<TFSAttr> FoundedInAttrs = new List<TFSAttr>();
            //if (Attrs != null)
            //if (Attrs.Count > 0) {
            //    txt += " C совпадением в атрибутах: ";
            //    foreach (TFSFileAttribute FileAttr in Attrs) {
            //        if (!FoundedInAttrs.Contains(FileAttr.Attr)) {
            //            FoundedInAttrs.Add(FileAttr.Attr);
            //            txt += FileAttr.Attr.Name + ", ";
            //        }
            //    }
            //    txt = txt.Substring(0, txt.Length - 2) + ".";
            //}


        }

        void ucF_GotFocus(object sender, RoutedEventArgs e) {
            pnlFileAttributes.Children.Clear();

            mTagsDB.LoadAllAttrs();

            TFSFile File = ((ucFile)sender).File;
            SelectedFile = File;

            if (File.Attrs != null)
                foreach (TFSFileAttribute FileAttr in File.Attrs) {
                    var Attribute = new ucFileAttribute(FileAttr);
                    pnlFileAttributes.Children.Add(Attribute);
                }

            AddFA.File = File;
        }

        void File_Changed(object sender, TFSItem.ChangedEventArgs e) {
            TFSFile File = (TFSFile)sender;

            pnlFileAttributes.Children.Clear();
            if (File.Attrs != null)
                foreach (TFSFileAttribute FileAttr in File.Attrs) {
                    var Attribute = new ucFileAttribute(FileAttr);
                    pnlFileAttributes.Children.Add(Attribute);
                }
        }

        private TFSFile mSelectedFile = null;
        private TFSFile SelectedFile {
            get {
                return mSelectedFile;
            }
            set {
                if (mSelectedFile != null)
                    mSelectedFile.Changed -= File_Changed;

                mSelectedFile = value;

                mSelectedFile.Changed += File_Changed;

                lblAttr.Content = "Атрибуты " + mSelectedFile.DisplayName();
            }
        }

        private TagsDB mTagsDB = new TagsDB();

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            foreach (String path in txtUpdatePath.Text.Split(new String[] { ", " }, StringSplitOptions.RemoveEmptyEntries)) {
                Title = "Обновление из " + path + " ...";
                mTagsDB.UpdateBase(path);
            }
            Title = "Обновление завершено";
        }
    }
}
