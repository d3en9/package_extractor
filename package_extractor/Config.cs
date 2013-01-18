using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using log4net;

namespace package_extractor
{
    public class Config
    {
        public static readonly ILog log = LogManager.GetLogger("log");

        const string fileName = "config.xml";
        const string outputDirectory = "out";

        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Charset { get; set; }
        public bool Unicode { get; set; }
        public int ConnectionTimeout { get; set; }
        public int PageSize { get; set; }

        public string FileStore { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<PacketPattern> PacketPatterns { get; set; }

        public string OutPutDirectory { get; set; }


        static string _appPath = null;
        
        [XmlIgnore]
        public static string AppPath
        {
            get
            {
                if (_appPath == null)
                    _appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                return _appPath;
            }
        }

        public Config()
        {
            PacketPatterns = new List<PacketPattern>();
        }

        public void generateDefault()
        {
            defaultInitial();
            save();
        }

        void defaultInitial()
        {
            User = "root";
            Password = "12345";
            Host = "localhost";
            Port = 5432;
            Database = "modelbase";
            Schema = "public";
            Unicode = true;
            ConnectionTimeout = 60;
            PageSize = 100;

            FileStore = @"D:\Java\Roopus-3.2\Filestore";

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            PacketPatterns.Add(new PacketPattern() { Model = "PURCB", Pattern = "ПакетСООУРЦБ1408%" });

            OutPutDirectory = Path.Combine(AppPath, outputDirectory);
        }

        public bool save()
        {
            log.Info("Config save");
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Config));
                using (Stream s = File.Create(Path.Combine(AppPath, fileName)))
                    xs.Serialize(s, this);
                log.Info("Config save - OK");
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }

        }

        void setConfig(Config config)
        {
            User = config.User;
            Password = config.Password;
            Host = config.Host;
            Port = config.Port;
            Database = config.Database;
            Schema = config.Schema;
            Charset = config.Charset;
            Unicode = config.Unicode;
            ConnectionTimeout = config.ConnectionTimeout;
            PageSize = config.PageSize;

            FileStore = config.FileStore;

            StartDate = config.StartDate;
            EndDate = config.EndDate;

            PacketPatterns = config.PacketPatterns;

            OutPutDirectory = config.OutPutDirectory;
        }

        public bool load()
        {
            log.Info("Config load");
            try
            {
                string filePath = Path.Combine(AppPath, fileName);
                if (!File.Exists(filePath))
                    generateDefault();
                else
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Config));
                    using (Stream s = File.OpenRead(filePath))
                         setConfig((Config)xs.Deserialize(s));
                }
                log.Info("Config load - OK");
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }
    }

    public struct PacketPattern
    {
        public string Model;
        public string Pattern;
    }
}
