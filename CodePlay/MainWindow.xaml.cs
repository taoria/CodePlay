using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace CodePlay {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        void AmazingBox(Object objects) {
            MessageBox.Show( objects.ToString() );
        }
        public MainWindow() {
            InitializeComponent();
            Initializer inits = new Initializer( "config.xml" );
            this.Config = inits.LoadConfig();
            if(Config == null) {
                Config = inits.CreateInitializedConfig();
            }
            this.Left = SystemParameters.WorkArea.Width - Config.ShiftX - this.Width;
            this.Top = Config.ShiftY;
            LoadImage();
            if (Gits == null) {
                Gits= new GitUtility(Config,0);
            }
            Animer = new LittleAnimer();
   
        }
        public void LoadImage() {
            this.Height = Config.WindowHeight;
            this.Width = Config.WindowWidth;

            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.StreamSource = new FileStream( Config.ImageAdress, FileMode.Open );
            bi.EndInit();

            this.image.Source = bi;

            this.image.Height = bi.Height * Config.ImageScaledY;
            this.image.Width = bi.Width * Config.ImageScaledX;

        }
        private Config Config { get; set; }
        private GitUtility Gits { get; set; }
        private LittleAnimer Animer { get; set; }

        private void AddRepo(object sender, RoutedEventArgs e) {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            Config.RepoList.Add( m_Dir );
            Config.SaveConfig();

        }

        private void HiddenWindow(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        Dictionary<string, int> dc = new Dictionary<string, int>();
        private void MainWindow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
             List<string> ls =   Gits.Run();


            string[] parted;
            int countedTotal;
            foreach (string ss in Config.TypeCodeText) {
                foreach (string s in ls) {
                    parted = s.Trim().Split('\t');
                    if (parted.Length != 3) continue;
                    if (parted[2].Contains(ss)) {
                        try {
                            int ans = int.Parse( parted[0] ) - int.Parse( parted[1] );
                            if (!dc.ContainsKey(parted[2]))
                                dc.Add(parted[2], ans);
                            else
                                dc[parted[2]] += ans;
                        }
                        catch (Exception) {
                            continue;
                        }
                    }
                }
            }


            int result = 0;
            foreach (KeyValuePair<string, int> keyValuePair in dc) {
                result += keyValuePair.Value;
            }
            MessageBox.Show("Your total Coding number is " + result.ToString());

        }

        private void QuitApp(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("config.xml");
        }

        private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e) {
        
        }
    }
    public class LittleAnimer {
        public string AnimeAdress { get; set; }
        public float  Wait{ get; set; }
        public string OnHoverAnime;
        public string OnClickAnime;
        public string OnRightClickAnime;
        public bool IsGif;
        public bool []IsAnimeCycled;
        private int _point= 0 ;
        [XmlIgnoreAttribute]
        public Image Img { get; set; }
        [XmlIgnoreAttribute] public Dictionary<int, BitmapImage> dc = new Dictionary<int, BitmapImage>(); 
        public void Loaded() {
            if (!IsGif) {
                DirectoryInfo dif = new DirectoryInfo( AnimeAdress );

                foreach (FileInfo filei in dif.GetFiles()) {
                    string name = filei.Name;
                    string []part = name.Split('.');
                    int result = 0;

                    if (part[1].Equals("png") || part[1].Equals("jpg") || part[1].Equals("bmp")) {
                       
                    }

                    else {
                        MessageBox.Show("Find type:" + part[1] + "Image Type Not Support!");
                        continue;

                    }

                    try {
                        result = int.Parse(part[0]);
                    }
                    catch (Exception) {
                        result = -1;
                        MessageBox.Show( "ERROR ,LOAD FILES ERROR,ILLEGAL FILENAMES"+part[0]);
                    }

                    BitmapImage tempBitmapImage  = new BitmapImage();
                    tempBitmapImage.BeginInit();
                    tempBitmapImage.StreamSource = new FileStream(filei.FullName, FileMode.Open );
                    tempBitmapImage.EndInit();

                    dc[result] = tempBitmapImage;
                    

                }

            }
        }
        public void OnHover() {
            DispatcherTimer animeFramer = new DispatcherTimer();
            animeFramer.Interval = new TimeSpan(0,0,0,0,(int) (Wait*1000));
            animeFramer.Tick += (s, e) => {
                if (!IsGif) {
                    Img.Source = dc[_point++];
                }
            };
        }
    }
    public class GitUtility {
        public Config config;
        private static string EnvironmentVariable {
            get {
                string sPath = System.Environment.GetEnvironmentVariable( "Path" );
                var result = sPath.Split(';');
                for(int i = 0; i < result.Length; i++) {
                    if(result[i].Contains( "Git" )) {
                        return result[i];
                    }
                }
                return null;
            }
        }

        public GitUtility(Config config, int count) {
            this.config = config;
            this.count = count;
        }

        int count = 0;
        void p_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            if(e.Data != null)
                ++count;
        }
        public List<string> Run() {
            List<string> ls = new List<string>();
            string gitPath = System.IO.Path.Combine( EnvironmentVariable, "git.exe" );
            Process p = new Process();
            p.StartInfo.FileName = gitPath;
           p.StartInfo.Arguments = "log --author=\"taoria\" --pretty=tformat: --numstat";
            //p.StartInfo.Arguments = "rev-list head";
          MessageBox.Show( p.StartInfo.Arguments );
 
            p.StartInfo.CreateNoWindow = true;
            foreach (var pathes in config.RepoList) {
                p.StartInfo.WorkingDirectory = pathes;
               
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
        
                p.OutputDataReceived += p_OutputDataReceived;
                count = 0;
                p.Start();
                string strRst;
                while((strRst = p.StandardOutput.ReadLine())!= null){
                    ls.Add( strRst );

                }
            
            
                p.WaitForExit();

            }
    
            Console.WriteLine( count );
            return ls;
        }
    }

    [XmlRootAttribute( "Config", IsNullable = false )]
    public class Config {
        public Config() {

        }

        [XmlElementAttribute( "ImageAdress", IsNullable = false )]
        public string ImageAdress { get; set; }

        [XmlElementAttribute( "ImageScaledX" )]
        public float ImageScaledX { get; set; }

        [XmlElementAttribute( "ImageScaledY" )]
        public float ImageScaledY { get; set; }

        [XmlElementAttribute( "RepoList" )]
        public List<string> RepoList = new List<string>();
        [XmlElementAttribute( "TypeCodeText" )]
        public List<string> TypeCodeText = new List<string>();
        [XmlAttribute( "WindowHeight" )]
        public int WindowHeight { get; set; }

        [XmlAttribute( "WindowWidth" )]
        public int WindowWidth { get; set; }

        [XmlAttribute( "ShiftX" )]
        public int ShiftX { get; set; }

        [XmlAttribute( "ShiftY" )]
        public int ShiftY { get; set; }

        [XmlElementAttribute( "AnimerConfig" )]
        public string AnimeAdress { get; set; }

        public void SaveConfig() {
            XmlSerializer.SaveToXml( "config.xml", this, typeof( Config ), "Config" );
        }
    }
    public static class XmlSerializer {
        public static void SaveToXml(string filePath, object sourceObj, Type type, string xmlRootName) {
            if(!string.IsNullOrWhiteSpace( filePath ) && sourceObj != null) {
                type = type != null ? type : sourceObj.GetType();

                using(StreamWriter writer = new StreamWriter( filePath )) {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = string.IsNullOrWhiteSpace( xmlRootName ) ?

                        new System.Xml.Serialization.XmlSerializer( type ) :
                        new System.Xml.Serialization.XmlSerializer( type, new XmlRootAttribute( xmlRootName ) );
                    xmlSerializer.Serialize( writer, sourceObj );
                }
            }
        }

        public static object LoadFromXml(string filePath, Type type) {
            object result = null;

            if(File.Exists( filePath )) {
                using(StreamReader reader = new StreamReader( filePath )) {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer( type );
                    result = xmlSerializer.Deserialize( reader );
                }
            }

            return result;
        }
    }
    class Initializer {
        public string ConfigAddr { get; set; }
        public Config LoadConfig() {
            Config loadedConfig = (Config)XmlSerializer.LoadFromXml( ConfigAddr, typeof( Config ) );
            return loadedConfig;
        }

        public Config CreateInitializedConfig() {
            Config config = new Config();
            config.ImageAdress = "image.jpg";
            config.WindowWidth = 250;
            config.WindowHeight = 150;
            config.ShiftX = 100;
            config.ShiftY = 100;
            config.ImageScaledX = 1;
            config.ImageScaledY = 1;
            config.TypeCodeText.Add( ".cpp" );
            config.TypeCodeText.Add( ".h" );
            config.TypeCodeText.Add( ".cs" );
            config.TypeCodeText.Add( ".java" );
            XmlSerializer.SaveToXml( "config.xml", config, typeof( Config ), "Config" );
            return config;

        }
        public Initializer(string configAddr) {
            this.ConfigAddr = configAddr;
        }
    }
}
