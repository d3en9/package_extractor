using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using Devart.Data.PostgreSql;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;

namespace package_extractor
{
    public class ExtractorFile : ICommand
    {
        public static readonly ILog log = LogManager.GetLogger("log");
        
        public void Execute()
        {
            log.Info("Start extracting");
            
            try
            {
                DateTime startDate = Global.Instance.config.StartDate;
                DateTime endDate = Global.Instance.config.EndDate;
                List<DateTime> listDate = new List<DateTime>();
                if (startDate >= endDate)
                    throw new Exception("Bad date interval");
                while (startDate <= endDate)
                {
                    listDate.Add(startDate);
                    startDate = startDate.AddDays(1);
                }
                foreach (DateTime d in listDate)
                {
                    extractDate(d);
                }
                log.Info("Finish extracting");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Console.WriteLine(ex);
            }
            
        }
                
        void extractDate(DateTime d)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            try
            {
                log.Info("Start Extracting for date: " + d.ToString());
                Console.WriteLine("Start Extracting for date: " + d.ToString());
                string outDir = Global.Instance.config.OutPutDirectory;
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                string path = Path.Combine(Global.Instance.config.FileStore, "document");
                path = Path.Combine(path, d.Year.ToString());
                path = Path.Combine(path, (d.Month -1) .ToString());
                path = Path.Combine(path, d.Day.ToString());
                foreach (string dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        
                        string documentUrl = Path.Combine(dir, "document.dat");
                        log.Info("Start Extracting for file: " + documentUrl);
                        byte[] zipBytes = File.ReadAllBytes(documentUrl);
                        using (var s = new MemoryStream(zipBytes))
                        using (ZipFile zip = new ZipFile(s))
                        {
                            ZipEntry xtddEntry = null;
                            foreach (ZipEntry z in zip)
                            {
                                if (Path.GetExtension(z.Name).ToLower() == ".xtdd")
                                {
                                    if (xtddEntry == null)
                                        xtddEntry = z;
                                    else
                                        throw new Exception("More than one files in archiev");
                                }
                            }
                            if (xtddEntry == null)
                                throw new Exception("Non report into archive");
                            using (Stream s1 = zip.GetInputStream(xtddEntry))
                            
                            {
                                XmlReader reader = XmlReader.Create(s1, settings);
                                reader.Read();
                                reader.Read();
                                foreach (var pattern in Global.Instance.config.PacketPatterns)
                                {
                                    if (reader.LocalName.Contains(pattern.Pattern.Replace("%", "")))
                                    {
                                        using (Stream s2 = zip.GetInputStream(xtddEntry))
                                        using (BinaryReader br = new BinaryReader(s2))
                                        {
                                            string extacted_name = Path.Combine(outDir, Path.GetFileName(dir) + ".xtdd");
                                            File.WriteAllBytes(extacted_name, br.ReadBytes((int)xtddEntry.Size));
                                            log.Info("Extracted to: " + extacted_name);
                                            Console.WriteLine(extacted_name);
                                        }
                                        break;
                                    }
                                }
                               
                            }
                        }
                        log.Info("End Extracting for file");
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }

                
                log.Info("End Extracting for date: " + d.ToString());
                Console.WriteLine("End Extracting for date: " + d.ToString());
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

    }
}
