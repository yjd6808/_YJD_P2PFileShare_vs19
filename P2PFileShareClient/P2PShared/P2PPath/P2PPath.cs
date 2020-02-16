// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-02-15 오후 1:50:36   
// @PURPOSE     : 피투피 패쓰
// ===============================


using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PShared
{
    public enum P2PPathType : int
    {
        None,
        Directory,
        Audio,
        Video,
        Exe,
        Zip,
        Image,
        Text,
        Ppt,
        Word,
        Excel,
        Pdf,

        //============//예외
        Previous = 999999
    }
    
    [Serializable]
    public class P2PPath
    {
        //적기 개귀찮
        /* ================================    확장자명 모음 ============================== */

        //참고 : https://en.wikipedia.org/wiki/Audio_file_format
        [NonSerialized]
        public static readonly string[] AudioExtensions = { ".mp3", ".3gp", ".aa", ".aac", ".m4a", ".flac", ".ogg", ".wav", ".wma" };

        //참고 : https://en.wikipedia.org/wiki/Video_file_format
        [NonSerialized]
        public static readonly string[] VideoExtensions = { ".mkv", ".flv", ".vob", ".avi", ".mov", "wmv", ".asf", ".mpeg", ".mp4" };

        [NonSerialized]
        public static readonly string[] ExeExtensions = { ".exe" };

        //참고 : https://www.computerhope.com/issues/ch001789.htm
        [NonSerialized]
        public static readonly string[] TextExtensions = { ".txt", ".json", ".xml", ".cpp", ".c", ".h", ".hpp", ".cs", ".java", ".sh", ".swift", ".vb", ".class" };

        //참고 : https://ko.wikipedia.org/wiki/%EC%95%95%EC%B6%95_%ED%8C%8C%EC%9D%BC
        [NonSerialized]
        public static readonly string[] ZipExtensions = { ".zip", ".7z", ".egg", ".rzip", "rar"};

        //참고 : https://en.wikipedia.org/wiki/Image_file_formats
        [NonSerialized]
        public static readonly string[] ImageExtensions = { ".jpeg", ".jpg", ".exif", ".tiff",  ".gif", ".bmp", ".png", ".svg"};

        //참고 : https://www.computerhope.com/issues/ch001789.htm
        [NonSerialized]
        public static readonly string[] PptExtensions = { ".ppt", ".pps", ".pptx" };
        [NonSerialized]
        public static readonly string[] ExcelExtensions = { ".ods", ".xlr", ".xls", ".xlsx" };
        [NonSerialized]
        public static readonly string[] PdfExtensions = { ".pdf" };
        [NonSerialized]
        public static readonly string[] WordExtensions = { ".doc", ".docx", ".hwp"};

        /* ================================  멤버 변수 ============================== */

        public string FullPath { get; set; }
        public PackIconKind PathPackIconKind { get; set; }
        public P2PPathType PathType { get; set; }
        public string FileName { get; set; }

        private P2PPath() {}
        public P2PPath(string path)
        {
            this.FullPath = path;
            var parsed = ParseDirectory(FullPath);
            this.PathPackIconKind = parsed.iconKind;
            this.PathType = parsed.pathType;
            this.FileName = Path.GetFileName(FullPath); ;
        }

        private (PackIconKind iconKind, P2PPathType pathType) ParseDirectory(string path)
        {
            PackIconKind iconKind = PackIconKind.FileOutline;
            P2PPathType pathType = P2PPathType.None;

            if (File.Exists(path) || Directory.Exists(path))
            {
                FileAttributes pathAttribute = File.GetAttributes(path);
                if ((pathAttribute & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    pathType = P2PPathType.Directory;
                    iconKind = PackIconKind.Folder;
                }
                else
                {
                    string extension = System.IO.Path.GetExtension(path).ToLower();

                    if (Array.Exists(AudioExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Audio;
                        iconKind = PackIconKind.Music;
                    }

                    else if (Array.Exists(VideoExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Video;
                        iconKind = PackIconKind.VideoOutline;
                    }

                    else if (Array.Exists(ExeExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Exe;
                        iconKind = PackIconKind.FilePresentationBox;
                    }

                    else if (Array.Exists(TextExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Text;
                        iconKind = PackIconKind.FileDocumentBoxOutline;
                    }

                    else if (Array.Exists(ZipExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Zip;
                        iconKind = PackIconKind.ZipBox;
                    }

                    else if (Array.Exists(ImageExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Image;
                        iconKind = PackIconKind.ImageOutline;
                    }

                    else if (Array.Exists(PptExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Ppt;
                        iconKind = PackIconKind.FilePowerpoint;
                    }

                    else if (Array.Exists(ExcelExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Excel;
                        iconKind = PackIconKind.FileExcel;
                    }

                    else if (Array.Exists(PdfExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Pdf;
                        iconKind = PackIconKind.FilePdf;
                    }

                    else if (Array.Exists(WordExtensions, x => x.Equals(extension)))
                    {
                        pathType = P2PPathType.Word;
                        iconKind = PackIconKind.FileWord;
                    }
                }
            }

            return (iconKind, pathType);
        }
        

        public static P2PPath GetPreviousPath()
        {
            P2PPath prevPath = new P2PPath();
            prevPath.FileName = "상위 디렉토리로 이동";
            prevPath.PathPackIconKind = PackIconKind.SubdirectoryArrowLeft;
            prevPath.PathType = P2PPathType.Previous;
            return prevPath;
        }
    }

}
