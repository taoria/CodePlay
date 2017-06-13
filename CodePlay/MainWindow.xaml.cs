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
            //Init initializer!
            Initializer inits = new Initializer( "config.xml" );



            //Load Config
            this.Config = inits.LoadConfig();
            if(Config == null) {
                Config = inits.CreateInitializedConfig();
            }



            //Locate Position 
            this.Left = SystemParameters.WorkArea.Width - Config.ShiftX - this.Width;
            this.Top = Config.ShiftY;



            if (Gits == null) {
                Gits= new GitUtility(Config,0);
            }

            //Load Anime System
            Animer = inits.LoadAnime(Config.AnimeAddress);
            if (Animer == null) {
                Animer = inits.CreateInitializedAnime();
            }


            Animer.Img = image;
            Animer.Loaded();

        }


        public void LoadImage() {


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

            Animer.Hovering();
            
        }

        private void MainWindow_OnMouseLeave(object sender, MouseEventArgs e) {
           Animer.ShowNormalAnime();
        }
    }
    [XmlRootAttribute( "Config", IsNullable = false )]
    public class LittleAnimer {
        [XmlElementAttribute( "AnimeAddress" )]
        public string AnimeAddress { get; set; }
        [XmlElementAttribute( "Wait" )]
        public float  Wait{ get; set; }
        [XmlElementAttribute( "OnHoverAnime" )]
        public string OnHoverAnime;
        [XmlElementAttribute( "OnClickAnime" )]
        public string OnClickAnime;
        [XmlElementAttribute( "OnRightClickAnime" )]
        public string OnRightClickAnime;
        [XmlElementAttribute( "NormalAnime" )]
        public string NormalAnime;
        [XmlElementAttribute( "IsGif" )]
        public bool IsGif;
        [XmlElementAttribute( "IsAnimeCycled" )]
        public bool []IsAnimeCycled;
        [XmlIgnoreAttribute]
        private int _point= 0 ;
        [XmlIgnoreAttribute]
        public Image Img { get; set; }

        
        [XmlIgnoreAttribute] public Dictionary<int, BitmapImage> dc = new Dictionary<int, BitmapImage>();
        [XmlIgnoreAttribute] public List<int> workingList = new List<int>();

        public void ClearWorkingList() {
            workingList.Clear();
        }

        public void Loaded() {
            if (!IsGif) {
                DirectoryInfo dif = new DirectoryInfo( AnimeAddress );

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
                    dc.Add(result,tempBitmapImage);


       
         

                }
            }
            animeFramer.Interval = new TimeSpan( 0, 0, 0, 0, (int)(Wait * 1000) );
            ShowNormalAnime();
            animeFramer.Tick += OnAnimeFramerOnTick;
            animeFramer.Start();
        }
        DispatcherTimer animeFramer = new DispatcherTimer();

        private void OnAnimeFramerOnTick(object s, EventArgs e) {
            if (!IsGif) {
            //    MessageBox.Show(workingList.Count.ToString());
                Img.Source = dc[workingList[_point = ((_point + 1) % workingList.Count)]];
                if (workingList.Count==2) {
                    //MessageBox.Show(_point.ToString());
                }

         //       Console.WriteLine( _point );
            }
        }

        private void OnWorking(string animeString) {
            string[] parted =animeString.Split(',');
            workingList.Clear();
            workingList= new List<int>(Array.ConvertAll( parted, int.Parse ));
        }

        public void Hovering() {
            OnWorking(OnHoverAnime);
         //   MessageBox.Show(workingList.Count.ToString());
        }

        public void ShowNormalAnime() {
            OnWorking(NormalAnime);
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

        [XmlElementAttribute( "ImageAddress", IsNullable = false )]
        public string ImageAddress { get; set; }

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
        public string AnimeAddress { get; set; }

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
        public LittleAnimer LoadAnime(string animeConfig) {
            LittleAnimer littleAnimer = (LittleAnimer)XmlSerializer.LoadFromXml( animeConfig, typeof(LittleAnimer ) );
            return littleAnimer;
        }
        public Config CreateInitializedConfig() {
            Config config = new Config();
            config.ImageAddress = "image.jpg";
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
            config.AnimeAddress = "animeconfig.xml";
            XmlSerializer.SaveToXml( "config.xml", config, typeof( Config ), "Config" );
            return config;

        }
        public LittleAnimer CreateInitializedAnime() {
            LittleAnimer littleAnimer = new LittleAnimer();
            littleAnimer.AnimeAddress = "images";
            littleAnimer.Wait = 0.1f;
            littleAnimer.IsGif = false;
            littleAnimer.OnHoverAnime = "1,2";
            littleAnimer.NormalAnime = "1";
            XmlSerializer.SaveToXml( "animeconfig.xml", littleAnimer, typeof( LittleAnimer), "Config" );
            return littleAnimer;

        }
        public Initializer(string configAddr) {
            this.ConfigAddr = configAddr;
        }
    }
}
