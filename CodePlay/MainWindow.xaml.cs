using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
using System.Xml.Serialization;
using MessageBox = System.Windows.MessageBox;

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
            LoadImage();
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
    }

    [XmlRootAttribute( "Config", IsNullable = false )]
    public class Config {
        public Config() {
            this.ImageScaledX = 1;
            this.ImageScaledY = 1;
        }

        [XmlElementAttribute( "ImageAdress", IsNullable = false )]
        public string ImageAdress { get; set; }

        [XmlElementAttribute( "ImageScaledX" )]
        public float ImageScaledX { get; set; }

        [XmlElementAttribute( "ImageScaledY" )]
        public float ImageScaledY { get; set; }

        [XmlElementAttribute( "RepoList" )]
        public List<string> RepoList = new List<string>();

        [XmlAttribute( "WindowHeight" )]
        public int WindowHeight { get; set; }

        [XmlAttribute( "WindowWidth" )]
        public int WindowWidth { get; set; }



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
            XmlSerializer.SaveToXml( "config.xml", config, typeof( Config ), "Config" );
            return config;
        }
        public Initializer(string configAddr) {
            this.ConfigAddr = configAddr;
        }
    }
}
