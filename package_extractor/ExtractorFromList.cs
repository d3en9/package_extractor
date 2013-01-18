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
    public class ExtractorFromList : ExtractorDB
    {
        public static readonly ILog log = LogManager.GetLogger("log");
        
        public override void Execute()
        {
            log.Info("Start extracting");
            
            try
            {
                string[] list = File.ReadAllLines("input.txt");
                foreach (string s in list)
                {
                    string[] line = s.Split(';');
                    packageInfo p = new packageInfo(line[0].Replace("\"",""),line[1].Replace("\"",""));
                    extractPackages(p);
                }
                log.Info("Finish extracting");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Console.WriteLine(ex);
            }
            log.Info("End extracting");
        }

        

    }
}
