using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using System.Data;
using System.IO;

namespace TagsFS_WPF {
    public abstract class TFSItem {
        public class ChangedEventArgs : EventArgs {
            public ChangedEventArgs() {
            }
        }
        public delegate void ChangedEventHandler(object sender, ChangedEventArgs e);
        public event EventHandler<ChangedEventArgs> Changed;

        public void UpdateView() {
            EventHandler<ChangedEventArgs> handler = this.Changed;
            if (handler != null)
                handler(this, new ChangedEventArgs());
        }

        public virtual String GetTableName() { return null; }
        public virtual String GetUpdateString() { return null; }
        public virtual String GetInsertString() { return null; }
        public virtual void UpdateInBase() {
            String UpdateString = GetUpdateString();
            if (UpdateString == null) return;

            ConnectionState prevConnectionState = DB.Connection().State; DB.OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(UpdateString, DB.Connection()))
                cmd.ExecuteNonQuery();

            if (prevConnectionState == ConnectionState.Closed) DB.Connection().Close();
        }
        public virtual void AddToBase() {
            String InsertString = GetInsertString();
            if (InsertString == null) return;

            ConnectionState prevConnectionState = DB.Connection().State; DB.OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(InsertString, DB.Connection()))
                cmd.ExecuteNonQuery();

            ID = DB.Connection().LastInsertRowId;

            if (prevConnectionState == ConnectionState.Closed) DB.Connection().Close();
        }
        public virtual void FixInBase(bool update = true) {
            if (DB == null) return;
            if (ID < 0)
                AddToBase();
            else
                UpdateInBase();
            if (update) UpdateView();
        }
        public virtual void Delete() {
            if (DB == null) { throw new MissingFieldException(); }
            if (ID < 0) return;

            ConnectionState prevConnectionState = DB.Connection().State; DB.OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + GetTableName() + " WHERE id = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();

            if (prevConnectionState == ConnectionState.Closed) DB.Connection().Close();

            ID = -1;
        }
        //public virtual void loadFromBase() { }

        public Int64 ID = -1;
        public TagsDB DB = null;
    }
    public enum StandardTag {
        File,
        Text,
        Note,
        Song,
        Music,
        Book,
        Tab,
        Guitar_Pro_Tab,
        Image,
        Photo,
        Photo_Raw,
        Film,
        Music_Video,
        Video,
        TVSeries
    }
    public enum StandardAttr {
        Name,
        Extention,
        Type,
        Rating,
        Created,
        Modified,

        Song_By,
        Song_Name,
        Song_Genre,
        Song_Year,
        Song_Lyrics,
        Song_Length,
        Song_Album,

        Image_Resolution,
        Image_Place,
        Image_Peoples,

        Thumbnail_Id,
        Thumbnail_Date,

        TVSeries_Season,
        TVSeries_Episode
    }
    
    public class TFSTag : TFSItem {
        public TFSTag(StandardTag _Tag, TagsDB _DB = null) {
            ID = (Int64)_Tag;
            Name = _Tag.ToString().Replace('_', ' ');
            DB = _DB;
        }
        public TFSTag(String _Name = "", Int64 _ParentID = -1, TagsDB _DB = null) {
            Name = _Name;
            ParentID = _ParentID;
            DB = _DB;
        }
        public TFSTag(Int64 _ID, String _Name = "", Int64 _ParentID = -1, TFSTag _ParentTag = null, TagsDB _DB = null) {
            ID = _ID;
            Name = _Name;
            ParentID = _ParentID;
            ParentTag = _ParentTag;
            DB = _DB;
        }

        public String FullName() {
            TFSTag t = this;
            String result = t.Name;
            while (t.ParentTag != null) {
                t = t.ParentTag;
                result = t.Name + "\\" + result;
            }
            return result;
        }

        public override String GetTableName() {
            return "tags";
        }
        public override String GetUpdateString() {
            return "UPDATE tags SET name = '" + Name + "', parentid = " + ParentID + " WHERE id = " + ID.ToString() + ";\n";
        }
        public override String GetInsertString() {
            return "INSERT INTO tags(name, parentid) VALUES('" + Name.Replace("'", "''") + "', " + ParentID + ");\n";
        }
        public override void Delete() {
            if (DB == null) { throw new MissingFieldException(); }
            if (ID < 0) return;

            ConnectionState prevConnectionState = DB.Connection().State; DB.OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + GetTableName() + " WHERE id = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();
            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + "filestags" + " WHERE tagid = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();
            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + "tagsattributes" + " WHERE tagid = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();

            if (prevConnectionState == ConnectionState.Closed) DB.Connection().Close();

            ID = -1;
        }

        public String Name = "";
        public Int64  ParentID = -1;
        public TFSTag ParentTag = null;
    }
    public class TFSAttr : TFSItem {
        public TFSAttr(StandardAttr _Attr, TagsDB _DB = null) {
            ID = (Int64)_Attr;
            Name = _Attr.ToString().Replace('_', ' ');
            //if (Name.IndexOf(" ") > 0)
            //    Name = Name.Substring(Name.IndexOf(" ") + 1);
            DB = _DB;
        }
        public TFSAttr(String _Name = "", String _Values = "", TagsDB _DB = null) {
            Name = _Name;
            Values += _Values;
            DB = _DB;
        }
        public TFSAttr(Int64 _ID, String _Name = "", String _Values = "", TagsDB _DB = null) {
            ID = _ID;
            Name = _Name;
            Values += _Values;
            DB = _DB;
        }

        public override String GetTableName() {
            return "attributes";
        }
        public override String GetUpdateString() {
            return "UPDATE attrs SET name = '" + Name + "', values = '" + Values + "' WHERE id = " + ID.ToString() + ";\n";
        }
        public override String GetInsertString() {
            return "INSERT INTO attributes(name, values) VALUES('" + Name.Replace("'", "''") + "', '" + Values.Replace("'", "''") + "');\n";
        }

        public override void Delete() {
            if (DB == null) { throw new MissingFieldException(); }
            if (ID < 0) return;

            ConnectionState prevConnectionState = DB.Connection().State; DB.OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + GetTableName() + " WHERE id = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();
            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + "filesattributes" + " WHERE attributeid = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();
            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM " + "tagsattributes" + " WHERE attributeid = " + ID.ToString() + ";", DB.Connection()))
                cmd.ExecuteNonQuery();

            if (prevConnectionState == ConnectionState.Closed) DB.Connection().Close();

            ID = -1;
        }

        public String Name = "";
        public String Values = "";
    }
    public class TFSFile : TFSItem {
        public TFSFile(String _Path = "", TFSTag _Tag = null, TagsDB _DB = null) {
            ID = -1;
            Path = _Path;
            DB = _DB;

            Tags = new List<TFSFileTag>();
            Tags.Add(new TFSFileTag(this, _Tag));
        }
        public TFSFile(String _Path = "", List<TFSTag> _Tags = null, TagsDB _DB = null) {
            ID = -1;
            Path = _Path;
            DB = _DB;

            Tags = new List<TFSFileTag>();
            foreach (TFSTag Tag in _Tags) {
                Tags.Add(new TFSFileTag(this, Tag));
            }
        }
        public TFSFile(String _Path = "", List<TFSFileTag> _Tags = null, TagsDB _DB = null) {
            ID = -1;
            Path = _Path;
            Tags = _Tags;
            DB = _DB;
        }
        public TFSFile(Int64 _ID, String _Path = "", List<TFSFileTag> _Tags = null, List<TFSFileAttribute> _Attrs = null, TagsDB _DB = null) {
            ID = _ID;
            Path = _Path;
            Tags = _Tags;
            Attrs = _Attrs;
            DB = _DB;
        }

        public String DisplayName() {
            String s = "";
            String Name = GetAttributeValue("Name");
            String SongBy = GetAttributeValue("Song By");
            String SongName = GetAttributeValue("Song Name");
            if (SongBy != null && SongName != null)
                s = SongBy + " - " + SongName;
            else
                s = Name == null ? Path : Name;

            return s;
        }
        public override String GetTableName() {
            return "files";
        }
        public override String GetUpdateString() {
            return "UPDATE files SET path = '" + Path + "' WHERE id = " + ID.ToString() + ";\n";
        }
        public override String GetInsertString() {
            //return "INSERT INTO files(path, name) VALUES('" + Path.Replace("'", "''") + "', '" + System.IO.Path.GetFileName(Path.Replace("'", "''")) + "');\n"; ;
            return "INSERT INTO files(path) VALUES('" + Path.Replace("'", "''") + "');\n"; ;
        }
        public override void Delete() {
            if (Tags != null) {
                while (Tags.Count > 0)
                    Tags.First().Delete();
                //if (Tags.Count() > 0) { Tags.Clear(); throw new DataException(); }
            }

            if (Attrs != null) {
                while (Attrs.Count > 0)
                    Attrs.First().Delete();
                //if (Attrs.Count() > 0) { Attrs.Clear(); throw new DataException(); }
            }

            base.Delete();
        }
        public TFSFileTag AddTag(TFSFileTag NewTag) {
            if (Tags == null) Tags = new List<TFSFileTag>();
            NewTag.File = this;
            Tags.Add(NewTag);
            return NewTag;
        }
        public TFSFileAttribute AddAttribute(TFSFileAttribute NewAttr) {
            if (Attrs == null) Attrs = new List<TFSFileAttribute>();
            NewAttr.File = this;
            Attrs.Add(NewAttr);
            return NewAttr;
        }
        public TFSFileAttribute GetAttribute(String AttrName) {
            if (Attrs == null) return null;
            foreach (TFSFileAttribute result in Attrs)
                if (result.Attr.Name == AttrName)
                    return result;
            return null;
        }
        public String GetAttributeValue(String AttrName) {
            TFSFileAttribute FA = GetAttribute(AttrName);
            if (FA != null) return FA.Value;
            else return null;
        }

        public String Path = "";

        public bool TagsSynchronized = false;
        public List<TFSFileTag> Tags = null;

        public bool AttrsSynchronized = false;
        public List<TFSFileAttribute> Attrs = null;
    }

    public class TFSFileTag : TFSItem {
        public TFSFileTag(TFSFile _File, TFSTag _Tag, Double _Accuracy = 1, TagsDB _DB = null) {
            File = _File;
            Tag = _Tag;
            Accuracy = _Accuracy;
            DB = _DB;
        }
        public TFSFileTag(Int64 _ID, TFSFile _File, TFSTag _Tag, Double _Accuracy = 1, TagsDB _DB = null) {
            ID = _ID;
            File = _File;
            Tag = _Tag;
            Accuracy = _Accuracy;
            DB = _DB;
        }

        public override string GetTableName() {
            return "filestags";
        }
        public bool IsInvalid() {
            if (File == null) return true;
            if (File.ID < 0) return true;
            if (Tag == null) return true;
            if (Tag.ID < 0) return true;
            return false;
        }
        public override String GetUpdateString() {
            if (IsInvalid()) return null;
            return "UPDATE filestags SET fileid = " + File.ID.ToString() + ", tagid = " + Tag.ID.ToString() + ", accuracy = " + Accuracy.ToString() + " WHERE id = " + ID.ToString() + ";\n";
        }
        public override String GetInsertString() {
            if (IsInvalid()) return null;
            return "INSERT INTO filestags(fileid, tagid, accuracy) VALUES(" + File.ID.ToString() + ", " + Tag.ID.ToString() + ", " + Accuracy.ToString() + ");\n";
        }
        public override void Delete() {
            if (File != null) if (File.Tags != null) { File.Tags.Remove(this); File.UpdateView(); }
            base.Delete();
        }

        public TFSFile File = null;
        public TFSTag  Tag = null;
        public Double  Accuracy = 1;
    }
    public class TFSFileAttribute : TFSItem {
        public TFSFileAttribute(TFSFile _File = null, TFSAttr _Attr = null, String _Value = "", Double _Accuracy = 1, TagsDB _DB = null) {
            File = _File;
            Attr = _Attr;
            Value = _Value;
            Accuracy = _Accuracy;
            DB = _DB;
        }
        public TFSFileAttribute(Int64 _ID, TFSFile _File = null, TFSAttr _Attr = null, String _Value = "", Double _Accuracy = 1, TagsDB _DB = null) {
            ID = _ID;
            File = _File;
            Attr = _Attr;
            Value = _Value;
            Accuracy = _Accuracy;
            DB = _DB;
        }

        public override string GetTableName() {
            return "filesattributes";
        }
        public bool IsInvalid() {
            if (File == null) return true;
            if (File.ID < 0) return true;
            if (Attr == null) return true;
            if (Attr.ID < 0) return true;
            return false;
        }
        public override String GetUpdateString() {
            if (IsInvalid()) return null;
            return "UPDATE filesattributes SET fileid = " + File.ID.ToString() + ", attributeid = " + Attr.ID.ToString() + ", value = '" + Value + "', accuracy = " + Accuracy.ToString() + " WHERE id = " + ID.ToString() + ";\n";
        }
        public override String GetInsertString() {
            if (IsInvalid()) return null;
            return "INSERT INTO filesattributes(fileid, attributeid, value, accuracy) VALUES(" + File.ID.ToString() + ", " + Attr.ID.ToString() + ", '" + Value.Replace("'", "''") + "', " + Accuracy.ToString() + ");\n";
        }
        public override void Delete() {
            if (File != null) if (File.Attrs != null) { File.Attrs.Remove(this); File.UpdateView(); }
            base.Delete();
        }

        public TFSFile File = null;
        public TFSAttr Attr = null;
        public String  Value = "";
        public Double  Accuracy = 1;
    }


    public class TagsDB {
        public TagsDB() {
            String Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Tags.db3";

            if (!System.IO.File.Exists(Path)) {
                mConnection = new SQLiteConnection("URI=file:" + Path);
                CreateDB("URI=file:" + Path);
            }
            else
                mConnection = new SQLiteConnection("URI=file:" + Path);

            OpenConnection();
        }

        public SQLiteConnection Connection() { return mConnection; }
        public void OpenConnection() {
            if (mConnection.State != ConnectionState.Open) {
                mConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                    cmd.CommandText = "PRAGMA synchronous = 0;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "PRAGMA journal_mode = OFF;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "PRAGMA cache_size = 8000;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "PRAGMA recursive_triggers = ON;";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void CloseConnection() {
            if (mConnection.State != ConnectionState.Closed) mConnection.Close();
        }

        #region Tag
        public void CheckTags(List<TFSTag> Tags) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM tags WHERE";

                foreach (TFSTag Tag in Tags) {
                    cmd.CommandText += " (name = '" + Tag.Name.Replace("'", "''") + "' AND parentid = " + Tag.ParentID + ") OR";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 3) + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        foreach (TFSTag Tag in Tags) {
                            if (Tag.Name == record.GetString(1))
                                Tag.ID = record.GetInt64(0);
                        }
                    }
                }

                int n = 0;
                cmd.CommandText = "";
                var tr = mConnection.BeginTransaction();
                foreach (TFSTag Tag in Tags) {
                    if (Tag.ID == -1) {
                        n++;
                        cmd.CommandText += "INSERT INTO tags(name, parentid) VALUES('" + Tag.Name.Replace("'", "''") + "', '" + Tag.ParentID + "');\n";
                    }
                }
                cmd.ExecuteNonQuery();
                tr.Commit();

                long id = mConnection.LastInsertRowId - n;
                foreach (TFSTag Tag in Tags) {
                    if (Tag.ID == -1) {
                        id++;
                        Tag.ID = id;
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();
        }
        #endregion
        #region File
        public void CheckFiles(List<TFSFile> Files) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM files WHERE";

                foreach (TFSFile File in Files) {
                    File.Path = File.Path.ToLower();
                    cmd.CommandText += " path = '" + File.Path.Replace("'", "''") + "' OR";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 3) + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        foreach (TFSFile File in Files) {
                            if (File.Path == record.GetString(1))
                                File.ID = record.GetInt64(0);
                        }
                    }
                }

                int n = 0;
                cmd.CommandText = "";
                var tr = mConnection.BeginTransaction();
                foreach (TFSFile File in Files) {
                    if (File.ID == -1) {
                        n++;
                        cmd.CommandText += "INSERT INTO files(path, name) VALUES('" + File.Path.Replace("'", "''") + "', '" + System.IO.Path.GetFileName(File.Path.Replace("'", "''")) + "');\n";
                    }
                }
                cmd.ExecuteNonQuery();
                tr.Commit();

                long id = mConnection.LastInsertRowId - n;
                foreach (TFSFile File in Files) {
                    if (File.ID == -1) {
                        id++;
                        File.ID = id;
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();
        }
        #endregion
        
        public void CheckFilesTags(List<TFSFileTag> _FilesTags) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM filestags WHERE";

                foreach (TFSFileTag FileTag in _FilesTags) {
                    if (FileTag.ID == -1)
                        cmd.CommandText += " (fileid = " + FileTag.File.ID.ToString() + " AND tagid = " + FileTag.Tag.ID.ToString() + " AND accuracy = " + FileTag.Accuracy.ToString() + ") OR";
                    else
                        cmd.CommandText += " (id = " + FileTag.ID.ToString() + ") OR";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 3) + ";";

                String newcmdtext = "";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        foreach (TFSFileTag FileTag in _FilesTags) {
                            if (FileTag.ID == -1) {
                                if (FileTag.File.ID == record.GetInt64(1) && FileTag.Tag.ID == record.GetInt64(2) && FileTag.Accuracy == record.GetDouble(3))
                                    FileTag.ID = record.GetInt64(0);
                            }
                            else
                                if (FileTag.ID == record.GetInt64(0))
                                    if (FileTag.File.ID != record.GetInt64(1) || FileTag.Tag.ID != record.GetInt64(2))
                                        newcmdtext += "UPDATE filestags SET fileid = " + FileTag.File.ID.ToString() + ", tagid = " + FileTag.Tag.ID.ToString() + ", accuracy = " + FileTag.Accuracy.ToString() + " WHERE id = " + FileTag.ID.ToString() + ";\n";
                        }
                    }
                }

                int n = 0;
                var tr = mConnection.BeginTransaction();
                cmd.CommandText = newcmdtext;
                //cmd.ExecuteNonQuery();
                //cmd.CommandText = "";
                foreach (TFSFileTag FileTag in _FilesTags) {
                    if (FileTag.ID == -1) {
                        n++;
                        cmd.CommandText += "INSERT INTO filestags(fileid, tagid, accuracy) VALUES(" + FileTag.File.ID.ToString() + ", " + FileTag.Tag.ID.ToString() + ", " + FileTag.Accuracy.ToString() + ");\n";
                        //if (n % 50 == 0) {
                        //    cmd.ExecuteNonQuery();
                        //    cmd.CommandText = "";
                        //}
                    }
                }
                cmd.ExecuteNonQuery();
                tr.Commit();

                long id = mConnection.LastInsertRowId - n;
                foreach (TFSFileTag FileTag in _FilesTags) {
                    if (FileTag.ID == -1) {
                        id++;
                        FileTag.ID = id;
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();
        }
        public List<TFSFileAttribute> GenerateFileAttributes(List<TFSFile> _Files) {
            List<TFSFileAttribute> Result = new List<TFSFileAttribute>();

            foreach (TFSFile File in _Files) {
                if (File.Attrs == null) File.Attrs = new List<TFSFileAttribute>();

                File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Name),
                               System.IO.Path.GetFileNameWithoutExtension(File.Path)));
                File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Extention),
                               System.IO.Path.GetExtension(File.Path)));

                File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Created),
                               System.IO.File.GetCreationTime(File.Path).ToString()));
                File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Modified),
                               System.IO.File.GetLastWriteTime(File.Path).ToString()));

                String ext = System.IO.Path.GetExtension(File.Path);
                if (ext == ".mp3") {
                    try {
                        TagLib.File mp3File = TagLib.File.Create(File.Path);

                        foreach (var s in mp3File.Tag.Performers)
                            File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Song_By), s));
                        
                        File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Song_Name), mp3File.Tag.Title));
                        File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Song_Album), mp3File.Tag.Album));
                        File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Song_Year), mp3File.Tag.Year.ToString()));
                        
                        foreach (var s in mp3File.Tag.Genres)
                            File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Song_Genre), s));

                        String len = Math.Truncate(mp3File.Properties.Duration.TotalMinutes).ToString() + ".";
                        if (Math.Round(mp3File.Properties.Duration.TotalSeconds % 60) < 10) len += "0";
                        len += Math.Round(mp3File.Properties.Duration.TotalSeconds % 60).ToString();

                        File.Attrs.Add(new TFSFileAttribute(File, new TFSAttr(StandardAttr.Song_Length), len));
                    }
                    catch { }
                }

                Result.AddRange(File.Attrs);
            }

            return Result;
        }
        public List<TFSFileTag> GenerateFileTags(List<TFSFile> _Files) {
            List<TFSFileTag> Result = new List<TFSFileTag>();

            foreach (TFSFile File in _Files) {
                if (File.Tags == null) File.Tags = new List<TFSFileTag>();

                String ext = System.IO.Path.GetExtension(File.Path);

                if (new List<String>() { ".mp3", ".flac", ".wav" }.Contains(ext))
                    File.Tags.Add(new TFSFileTag(File, new TFSTag(StandardTag.Music)));
                
                if (new List<String>() { ".gp3", ".gp4", ".gp5", ".gtp" }.Contains(ext))
                    File.Tags.Add(new TFSFileTag(File, new TFSTag(StandardTag.Guitar_Pro_Tab)));

                if (new List<String>() { ".bmp", ".jpg", ".jpeg", ".png" }.Contains(ext))
                    File.Tags.Add(new TFSFileTag(File, new TFSTag(StandardTag.Image)));

                if (new List<String>() { ".cr2" }.Contains(ext))
                    File.Tags.Add(new TFSFileTag(File, new TFSTag(StandardTag.Photo_Raw)));

                if (new List<String>() { ".avi", ".flv", ".mp4" }.Contains(ext))
                    File.Tags.Add(new TFSFileTag(File, new TFSTag(StandardTag.Video)));

                if (new List<String>() { ".txt", ".doc", ".docx", ".odt" }.Contains(ext))
                    File.Tags.Add(new TFSFileTag(File, new TFSTag(StandardTag.Text)));

                Result.AddRange(File.Tags);
            }

            return Result;
        }
        public void CheckFilesAttributes(List<TFSFileAttribute> _FilesAttributes) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            int max = 600;
            if (_FilesAttributes.Count > max) {
                for (int i = 0; i < _FilesAttributes.Count; i += max)
                    CheckFilesAttributes(_FilesAttributes.GetRange(i, max + i > _FilesAttributes.Count ? _FilesAttributes.Count - i : max));
                return;
            }

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM filesattributes WHERE";

                foreach (TFSFileAttribute FileAttr in _FilesAttributes) {
                    if (FileAttr.ID == -1)
                        cmd.CommandText += " (fileid = " + FileAttr.File.ID.ToString() + " AND attributeid = " + FileAttr.Attr.ID.ToString() + ") OR";
                    else
                        cmd.CommandText += " (id = " + FileAttr.ID.ToString() + ") OR";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 3) + ";";

                String newcmdtext = "";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        foreach (TFSFileAttribute FileAttr in _FilesAttributes) {
                            if (FileAttr.ID == -1) {
                                if (FileAttr.File.ID == record.GetInt64(1) && FileAttr.Attr.ID == record.GetInt64(2))
                                    FileAttr.ID = record.GetInt64(0);
                            }
                            //else
                            //    if (FileAttr.ID == record.GetInt64(0))
                            //        if (FileAttr.File.ID != record.GetInt64(1) || FileAttr.Tag.ID != record.GetInt64(2))
                            //            newcmdtext += "UPDATE filesattributes SET fileid = " + FileAttr.File.ID.ToString() + ", tagid = " + FileAttr.Tag.ID.ToString() + ", accuracy = " + FileAttr.Accuracy.ToString() + " WHERE id = " + FileAttr.ID.ToString() + ";\n";
                        }
                    }
                }

                int n = 0;
                var tr = mConnection.BeginTransaction();
                cmd.CommandText = "";
                foreach (TFSFileAttribute FileAttr in _FilesAttributes) {
                    if (FileAttr.ID == -1) {
                        if (FileAttr.Value != null) {
                            n++;
                            cmd.CommandText += "INSERT INTO filesattributes(fileid, attributeid, value, accuracy) VALUES(" + FileAttr.File.ID.ToString() + ", " + FileAttr.Attr.ID.ToString() + ", '" + FileAttr.Value.Replace("'", "''") + "', " + FileAttr.Accuracy.ToString() + ");\n";
                        }
                        //if (n % 10 == 0) {
                        //    cmd.ExecuteNonQuery();
                        //    cmd.CommandText = "";
                        //}
                    }
                }
                cmd.ExecuteNonQuery();
                tr.Commit();

                long id = mConnection.LastInsertRowId - n;
                foreach (TFSFileAttribute FileAttr in _FilesAttributes) {
                    if (FileAttr.ID == -1) {
                        id++;
                        FileAttr.ID = id;
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();
        }
        public void FillFilesAttributes(List<TFSFile> _Files) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM filesattributes WHERE";

                foreach (TFSFile File in _Files) {
                    if (File.ID != -1)
                        cmd.CommandText += " fileid = " + File.ID.ToString() + " OR";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 3) + ";";

                String newcmdtext = "";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        foreach (TFSFile File in _Files) {
                            if (File.ID == record.GetInt64(1)) {
                                if (File.Attrs == null) File.Attrs = new List<TFSFileAttribute>();

                                if (mAttrs.Count > (int)record.GetInt64(2))
                                    File.Attrs.Add(new TFSFileAttribute(record.GetInt64(0), File, mAttrs[(int)record.GetInt64(2)], record.GetString(3), record.GetDouble(4), this));
                                else
                                    File.Attrs.Add(new TFSFileAttribute(record.GetInt64(0), File, new TFSAttr((StandardAttr)record.GetInt64(2)), record.GetString(3), record.GetDouble(4), this));
                            }
                            //else
                            //    if (FileAttr.ID == record.GetInt64(0))
                            //        if (FileAttr.File.ID != record.GetInt64(1) || FileAttr.Tag.ID != record.GetInt64(2))
                            //            newcmdtext += "UPDATE filesattributes SET fileid = " + FileAttr.File.ID.ToString() + ", tagid = " + FileAttr.Tag.ID.ToString() + ", accuracy = " + FileAttr.Accuracy.ToString() + " WHERE id = " + FileAttr.ID.ToString() + ";\n";
                        }
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();
        }
        public void FillFilesTags(List<TFSFile> _Files) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            if (!mTagsActual) LoadAllTags();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM filestags WHERE";

                foreach (TFSFile File in _Files) {
                    if (File.ID != -1)
                        cmd.CommandText += " fileid = " + File.ID.ToString() + " OR";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 3) + ";";

                String newcmdtext = "";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        Int64 ID = record.GetInt64(1);
                        foreach (TFSFile File in _Files) {
                            if (File.ID == ID) {
                                if (File.Tags == null) File.Tags = new List<TFSFileTag>();
                                Int64 TagID = record.GetInt64(2);
                                if (mTags.ContainsKey(TagID))
                                    File.Tags.Add(new TFSFileTag(record.GetInt64(0), File, mTags[(int)TagID], record.GetDouble(3), this));
                            }
                            //else
                            //    if (FileAttr.ID == record.GetInt64(0))
                            //        if (FileAttr.File.ID != record.GetInt64(1) || FileAttr.Tag.ID != record.GetInt64(2))
                            //            newcmdtext += "UPDATE filesattributes SET fileid = " + FileAttr.File.ID.ToString() + ", tagid = " + FileAttr.Tag.ID.ToString() + ", accuracy = " + FileAttr.Accuracy.ToString() + " WHERE id = " + FileAttr.ID.ToString() + ";\n";
                        }
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();
        }

        // Процедура обновления данных в бд для всех файлов папки Dir
        public void UpdateBase(String Dir) {
            if (!Directory.Exists(Dir)) return;

            var Tags = new List<TFSTag>();

            long PrevID = -1;
            foreach (String Tag in Dir.Split('\\')) {
                if (Tag != "") {
                    TFSTag CurrentTag = AddTag(Tag, PrevID);
                    PrevID = CurrentTag.ID;
                    Tags.Add(CurrentTag);
                }
            }

            OpenConnection();
            addFilesInDir(Dir, PrevID, Tags);
            CloseConnection();
        }
        private void addFilesInDir(String Dir, long _ParentTagID = -1, List<TFSTag> _Tags = null) {
            //try {
                List<TFSFile> NewFiles = new List<TFSFile>();
                foreach (String file in Directory.GetFiles(Dir)) {
                    NewFiles.Add(new TFSFile(file, _Tags.Last()));
                }
                List<TFSFileAttribute> NewFilesAttributes = GenerateFileAttributes(NewFiles);

                CheckFiles(NewFiles); // Добавление/обновление файлов в БД
                CheckFilesAttributes(NewFilesAttributes); // Добавление/обновление атрибутов в БД

                List<TFSFileTag> NewFilesTags = new List<TFSFileTag>();
                GenerateFileTags(NewFiles);
                foreach (TFSFile File in NewFiles) {
                    NewFilesTags.AddRange(File.Tags);
                }
                CheckFilesTags(NewFilesTags); // Добавление/обновление связей файл-тег в БД

                List<TFSTag> SubDirsTags = new List<TFSTag>(); // Теги для новых папок
                foreach (String SubDir in Directory.GetDirectories(Dir)) {
                    SubDirsTags.Add(new TFSTag(Path.GetFileName(SubDir), _ParentTagID));
                }
                CheckTags(SubDirsTags); // Добавление/обновление тэгов в БД

                int i = 0; // Запуск рекурсии для обхода всех вложенных папок
                foreach (String SubDir in Directory.GetDirectories(Dir)) {
                    List<TFSTag> NewTags = _Tags;
                    NewTags.Add(SubDirsTags[i]);

                    addFilesInDir(SubDir, SubDirsTags[i].ID, NewTags);
                    i++;
                }
            //}
            //catch { }
        }


        internal List<TFSTag> FindAllTagChildren(TFSTag _Tag) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSTag> result = new List<TFSTag>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "CREATE TEMPORARY TABLE alltagschildren (id INTEGER)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TRIGGER alltagschildren AFTER INSERT " +
                                    "ON alltagschildren " +
                                    "BEGIN " +
                                    "INSERT INTO alltagschildren(id) select id from tags where parentid = new.id; " +
                                    "END;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO alltagschildren VALUES(" + _Tag.ID + ");";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT * FROM tags WHERE id IN (SELECT id FROM alltagschildren);";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        if (mTagsActual && mTags.ContainsKey(record.GetInt64(0)))
                            result.Add(mTags[record.GetInt64(0)]);
                        else
                            result.Add(new TFSTag(record.GetInt64(0), record.GetString(1), record.GetInt64(2)));
                    }
                }

                cmd.CommandText = "DROP TABLE alltagschildren;";
                cmd.ExecuteNonQuery();
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }
        public List<TFSTag> FindTagsWhere(String Where) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSTag> result = new List<TFSTag>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM tags WHERE " + Where + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        if (mTagsActual && mTags.ContainsKey(record.GetInt64(0)))
                            result.Add(mTags[record.GetInt64(0)]);
                        else
                            result.Add(new TFSTag(record.GetInt64(0), record.GetString(1), record.GetInt64(2)));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }
        public List<TFSTag> FindTagsLike(String Name) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSTag> result = new List<TFSTag>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM tags WHERE name LIKE '" + Name + "';";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        result.Add(new TFSTag(record.GetInt64(0), record.GetString(1), record.GetInt64(2)));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }
        public List<TFSFile> FindFilesWithTags(List<TFSTag> _Tags) {
            if (_Tags == null || _Tags.Count <= 0) return null;
            LoadAllTags();
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSFile> result = new List<TFSFile>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText += "select * from files f inner join filestags ft on f.id = ft.fileid where ";
                foreach (var t in _Tags)
                    cmd.CommandText += "tagid = " + t.ID + " OR ";

                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 4) + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        result.Add(new TFSFile(record.GetInt64(0), record.GetString(1), null, null, this));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }
        //public List<TFSFile> FindFiles(List<TFSTag> _Tags, List<String> _AttrsValues) {
        //    if (_Tags == null || _Tags.Count <= 0) return null;
        //    LoadAllTags();
        //    ConnectionState prevConnectionState = mConnection.State; OpenConnection();

        //    List<TFSFile> result = new List<TFSFile>();

        //    using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
        //        cmd.CommandText += "select id, path, count(*) from (" +
        //            "select distinct f.id, f.path, ft.tagid " +
        //            "from files f inner join filestags ft on f.id = ft.fileid where tagid in (<TAGS>) union " +
        //            "select distinct f2.id, f2.path, fa.value " +
        //            "from files f2 inner join filesattributes fa on f2.id = fa.fileid where value in (<ATTRS>)" +
        //            ") group by id order by count(*) desc;";

        //        String s = "";
        //        foreach (var t in _Tags)
        //            s += t.ID + ", ";
        //        cmd.CommandText = cmd.CommandText.Replace("<TAGS>", s.Substring(0, s.Length - 2));

        //        s = "";
        //        foreach (var t in _AttrsValues)
        //            s += "'" + t + "', ";
        //        cmd.CommandText = cmd.CommandText.Replace("<ATTRS>", s.Substring(0, s.Length - 2));

        //        using (SQLiteDataReader reader = cmd.ExecuteReader()) {
        //            foreach (DbDataRecord record in reader) {
        //                result.Add(new TFSFile(record.GetInt64(0), record.GetString(1), null, null, this));
        //            }
        //        }
        //    }

        //    if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

        //    return result;
        //}
public List<TFSFile> FindFiles(List<TFSTag> _Tags, String _AttrsWhere, List<TFSTag> _Tags1, String _AttrsWhere1, List<TFSTag> _Tags2, String _AttrsWhere2, List<TFSTag> _Tags3, String _AttrsWhere3) {
    //if (_Tags == null || _Tags.Count <= 0) return null;
    LoadAllTags();

    String s;
    ConnectionState prevConnectionState = mConnection.State; OpenConnection();

    List<TFSFile> result = new List<TFSFile>();

    using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
        cmd.CommandText = 
        "select id, path, count(*) from (" + 
        "select distinct f.id,  f.path,  ft.tagid, '1' " +
        "from files f inner join filestags ft on f.id = ft.fileid where tagid in (<TAGS>) union " +
        "select distinct f2.id, f2.path, fa.value, '2' " +
        "from files f2 inner join filesattributes fa on f2.id = fa.fileid where <ATTRSWHERE> union " +

        "select distinct f3.id, f3.path, ft.tagid, '3' " +
        "from files f3 inner join filestags ft on f3.id = ft.fileid where tagid in (<TAGS2>) union " +
        "select distinct f4.id, f4.path, fa4.value, '4' " +
        "from files f4 inner join filesattributes fa4 on f4.id = fa4.fileid where <ATTRSWHERE2> union " +

        "select distinct f5.id, f5.path, ft.tagid, '5' " +
        "from files f5 inner join filestags ft on f5.id = ft.fileid where tagid in (<TAGS3>) union " +
        "select distinct f6.id, f6.path, fa6.value, '6' " +
        "from files f6 inner join filesattributes fa6 on f6.id = fa6.fileid where <ATTRSWHERE3> union " +

        "select distinct f7.id, f7.path, ft.tagid, '7' " +
        "from files f7 inner join filestags ft on f7.id = ft.fileid where tagid in (<TAGS4>) union " +
        "select distinct f8.id, f8.path, fa8.value, '8' " +
        "from files f8 inner join filesattributes fa8 on f8.id = fa8.fileid where <ATTRSWHERE4>" +
        ") group by id order by count(*) desc;";

        if (_Tags.Count > 0) { s = ""; foreach (var t in _Tags) s += t.ID + ", "; } else s = "  ";
        cmd.CommandText = cmd.CommandText.Replace("<TAGS>", s.Substring(0, s.Length - 2));
        cmd.CommandText = cmd.CommandText.Replace("<ATTRSWHERE>", _AttrsWhere);

        if (_Tags1.Count > 0) { s = ""; foreach (var t in _Tags1) s += t.ID + ", "; } else s = "  ";
        cmd.CommandText = cmd.CommandText.Replace("<TAGS2>", s.Substring(0, s.Length - 2));
        cmd.CommandText = cmd.CommandText.Replace("<ATTRSWHERE2>", _AttrsWhere1);

        if (_Tags2.Count > 0) { s = ""; foreach (var t in _Tags2) s += t.ID + ", "; } else s = "  ";
        cmd.CommandText = cmd.CommandText.Replace("<TAGS3>", s.Substring(0, s.Length - 2));
        cmd.CommandText = cmd.CommandText.Replace("<ATTRSWHERE3>", _AttrsWhere2);

        if (_Tags3.Count > 0) { s = ""; foreach (var t in _Tags3) s += t.ID + ", "; } else s = "  ";
        cmd.CommandText = cmd.CommandText.Replace("<TAGS4>", s.Substring(0, s.Length - 2));
        cmd.CommandText = cmd.CommandText.Replace("<ATTRSWHERE4>", _AttrsWhere3);

        using (SQLiteDataReader reader = cmd.ExecuteReader()) {
            foreach (DbDataRecord record in reader) {
                result.Add(new TFSFile(record.GetInt64(0), /*record.GetInt64(2).ToString() + */record.GetString(1), null, null, this));
            }
        }
    }

    if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

    return result;
}
        public List<TFSFileAttribute> FindFileAttrsWhere(String Where) {
            //if (mAttrsActual == false) 
                LoadAllAttrs();
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSFileAttribute> result = new List<TFSFileAttribute>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM filesattributes WHERE " + Where + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        Int64 ID = record.GetInt64(2);
                        TFSAttr fa = (mAttrs.Count > ID) ? mAttrs[(int)ID] : new TFSAttr(ID);
                        result.Add(new TFSFileAttribute(record.GetInt64(0), new TFSFile(record.GetInt64(1)), fa, record.GetString(3), record.GetDouble(4), this));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }
        public List<TFSFile> FindFilesWhere(String Where) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSFile> result = new List<TFSFile>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM files WHERE " + Where + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        result.Add(new TFSFile(record.GetInt64(0), record.GetString(1), null, null, this));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }
        public List<TFSFile> FindFilesLike(String Name) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            List<TFSFile> result = new List<TFSFile>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM files WHERE path LIKE '" + Name + "';";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        result.Add(new TFSFile(record.GetInt64(0), record.GetString(1), null, null, this));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return result;
        }

        public long FindTag(Int64 _TagID) {
            long ID = -1;

            mConnection.Open();
            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {

                cmd.CommandText = "SELECT * FROM tags WHERE id = " + _TagID + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        ID = record.GetInt64(0);
                        break;
                    }
                }
            }
            mConnection.Close();

            return ID;
        }
        public long FindTag(String Name, long PTagID) {
            long ID = -1;

            mConnection.Open();
            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM tags WHERE name = '" + Name + "' AND parentid = " + PTagID.ToString() + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        ID = record.GetInt64(0);
                        break;
                    }
                }
            }
            mConnection.Close();

            return ID;
        }
        public TFSTag AddTag(String _Name, long _PTagID = -1) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            long ID = -1;
            
            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM tags WHERE name = '" + _Name + "' AND parentid = " + _PTagID.ToString() + ";";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        ID = record.GetInt64(0);
                        break;
                    }
                }

                if (ID == -1) {
                    cmd.CommandText = "INSERT INTO tags(name, parentid) VALUES('" + _Name + "', " + _PTagID + ")";
                    cmd.ExecuteNonQuery();

                    ID = mConnection.LastInsertRowId;
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            var Tag = new TFSTag(ID, _Name, _PTagID, null, this);

            if (mTagsActual) {
                if (!mTags.ContainsKey(ID)) {
                    mTags.Add(ID, Tag);
                }
            }

            return Tag;
        }
        public TFSAttr AddAttribute(String _Name) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            long ID = -1;
            Boolean Was = false;

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM attributes WHERE name = '" + _Name + "';";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        ID = record.GetInt64(0);
                        Was = true;
                        break;
                    }
                }

                if (ID == -1) {
                    cmd.CommandText = "INSERT INTO attributes(name) VALUES('" + _Name + "')";
                    cmd.ExecuteNonQuery();

                    ID = mConnection.LastInsertRowId;
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            var Tag = new TFSAttr(ID, _Name, "", this);

            if (mAttrsActual && !Was) {
                mAttrs.Add(Tag);
            }

            return Tag;
        }


        public List<TFSTag> GetTagHierarchy(TFSTag Tag) {
            ConnectionState prevConnectionState = mConnection.State; OpenConnection();
            
            List<TFSTag> Hierarchy = new List<TFSTag>();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                Hierarchy.Add(Tag);

                while (Tag.ParentID != -1) {
                    cmd.CommandText = "SELECT * FROM tags WHERE id = '" + Tag.ParentID.ToString() + "';";

                    using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();
                            Tag.ParentTag = new TFSTag(reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2));
                            Tag = Tag.ParentTag;
                            Hierarchy.Add(Tag);
                        }
                        else {
                            Tag.ParentID = -1;
                        }
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            return Hierarchy;
        }


        private void CreateDB(String Path) {
            String StrCommand = "";
            StrCommand += "CREATE TABLE files       (id INTEGER PRIMARY KEY ASC, path TEXT, name TEXT);\n";
            StrCommand += "CREATE TABLE tags        (id INTEGER PRIMARY KEY ASC, name TEXT, parentid INTEGER);\n";
            StrCommand += "CREATE TABLE attributes  (id INTEGER PRIMARY KEY ASC, name TEXT, [values] TEXT);\n";

            StrCommand += "CREATE TABLE filestags         (id INTEGER PRIMARY KEY ASC, fileid INTEGER, tagid INTEGER, accuracy REAL);\n";
            StrCommand += "CREATE TABLE filesattributes   (id INTEGER PRIMARY KEY ASC, fileid INTEGER, attributeid INTEGER, value TEXT, accuracy REAL);\n";
            StrCommand += "CREATE TABLE tagssynonyms      (id INTEGER PRIMARY KEY ASC, tagida INTEGER, tagidb INTEGER, accuracy REAL);\n";
            StrCommand += "CREATE TABLE tagsattributes    (id INTEGER PRIMARY KEY ASC, tagid INTEGER, attributeid INTEGER, value TEXT, accuracy INTEGER);\n";

            SQLiteCommand Command = new SQLiteCommand(StrCommand, mConnection);
            mConnection.Open();
            Command.ExecuteNonQuery();

            Command.CommandText = "";
            foreach (StandardAttr a in (StandardAttr[])Enum.GetValues(typeof(StandardAttr))) {
                TFSAttr Attr = new TFSAttr(a);
                Command.CommandText += "INSERT INTO attributes   (id, name)    VALUES(" + Attr.ID.ToString() + ", '" + Attr.Name + "');\n";
            }
            Command.ExecuteNonQuery();

            Command.CommandText = "";
            foreach (StandardTag t in (StandardTag[])Enum.GetValues(typeof(StandardTag))) {
                TFSTag Tag = new TFSTag(t);
                Command.CommandText += "INSERT INTO tags   (id, name, parentid)    VALUES(" + Tag.ID.ToString() + ", '" + Tag.Name + "', " + Tag.ParentID.ToString() + ");\n";
            }
            Command.ExecuteNonQuery();

            mConnection.Close();
        }


        public void LoadAllAttrs() {
            if (mAttrsActual == true) return;

            if (mAttrs == null) mAttrs = new List<TFSAttr>();
            else mAttrs.Clear();

            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM attributes;";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        mAttrs.Add(new TFSAttr(record.GetInt64(0), record.GetString(1), !record.IsDBNull(2) ? record.GetString(2) : "", this));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            mAttrsActual = true;
        }
        public void LoadAllTags() {
            if (mTags == null) mTags = new Dictionary<long, TFSTag>();
            else mTags.Clear();

            ConnectionState prevConnectionState = mConnection.State; OpenConnection();

            using (SQLiteCommand cmd = new SQLiteCommand(mConnection)) {
                cmd.CommandText = "SELECT * FROM tags;";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    foreach (DbDataRecord record in reader) {
                        mTags.Add(record.GetInt64(0), new TFSTag(record.GetInt64(0), record.GetString(1), record.GetInt64(2), null, this));
                    }
                }
            }

            if (prevConnectionState == ConnectionState.Closed) mConnection.Close();

            foreach (var Tag in mTags) {
                if (Tag.Value.ParentID >= 0) {
                    if (mTags.ContainsKey(Tag.Value.ParentID))
                        Tag.Value.ParentTag = mTags[(int)Tag.Value.ParentID];
                    else 
                        Tag.Value.ParentID = -1;
                }
            }

            mTagsActual = true;
        }


        private Boolean mAttrsActual = false;
        public List<TFSAttr> mAttrs;
        public Boolean mTagsActual = false;
        public Dictionary<Int64, TFSTag> mTags;
        private SQLiteConnection mConnection;
    }


}
