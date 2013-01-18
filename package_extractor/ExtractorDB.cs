using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using Devart.Data.PostgreSql;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace package_extractor
{
    public class ExtractorDB : ICommand
    {
        public static readonly ILog log = LogManager.GetLogger("log");
        
        public virtual void Execute()
        {
            log.Info("Start extracting");
            int count;
            int pageSize = Global.Instance.config.PageSize;
            try
            {
                PgSqlConnection connection1 = getConnection();
                string countCommandSql = "SELECT count(*) " + GetSqlBodyCommand();
                string selectCommandSql = "SELECT model.id, model.urldocument " + GetSqlBodyCommand() + " order by model.id asc";
                PgSqlCommand command = new PgSqlCommand(countCommandSql, connection1);
                command.Parameters.AddWithValue("startDate", Global.Instance.config.StartDate);
                command.Parameters.AddWithValue("endDate", Global.Instance.config.EndDate);
                command.Parameters.AddWithValue("model", Global.Instance.config.PacketPatterns[0].Model);
                command.Parameters.AddWithValue("pattern", Global.Instance.config.PacketPatterns[0].Pattern);
                using (connection1)
                using (command)
                {
                    connection1.Open();
                    count = Convert.ToInt32(command.ExecuteScalar());
                    log.Info("Total count:" + count.ToString());
                    Console.WriteLine("Total count:" + command.ExecuteScalar().ToString());
                }
                int j = 0;
                //for (int i = 0; i < count; i += pageSize) 
                int k = pageSize;
                int startFrom = 0;
                while (k == pageSize)
                {
                    j++;
                    log.Info("**start iteration: " + j);
                    Console.WriteLine("start iteration: " + j);
                    PgSqlConnection connection2 = getConnection();
                    command.CommandText = selectCommandSql;
                    command.Connection = connection2;
                    log.Info("****start fetch ");
                    List<packageInfo> fileList = new List<packageInfo>();
                    k = 0;
                    using (connection2)
                    using (command)
                    {
                        connection2.Open();
                        using (PgSqlDataReader reader = command.ExecutePageReader(System.Data.CommandBehavior.SingleResult, startFrom, pageSize))
                        {
                            while (reader.Read())
                            {
                                fileList.Add(new packageInfo(reader.GetValue(0).ToString(), reader.GetValue(1).ToString()));
                                k++;
                            }
                        }
                    }
                    startFrom += pageSize;
                    log.Info("****end fetch ");
                    log.Info("****start coping ");
                    foreach (packageInfo p in fileList)
                    {
                        extractPackages(p);
                    }
                    log.Info("****end coping ");
                    log.Info("**end iteration: " + j);
                    Console.WriteLine("end iteration: " + j);
                }
                log.Info("Finish extracting");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Console.WriteLine(ex);
            }
            
        }

        protected class packageInfo
        {
            public string uid { get; set; }
            public string url { get; set; }
            public string path
            {
                get
                {
                    return Path.Combine(Global.Instance.config.FileStore, url.StartsWith("fs:/") ? url.Remove(0, 4) : url).Replace("/", "\\");
                }
            }


            public packageInfo(string uid, string url)
            {
                this.uid = uid;
                this.url = url;
            }
        }

        PgSqlConnection getConnection()
        {
            PgSqlConnection con = new PgSqlConnection();
            con.UserId = Global.Instance.config.User;
            con.Password = Global.Instance.config.Password;
            con.Host = Global.Instance.config.Host;
            con.Port = Global.Instance.config.Port;
            con.Database = Global.Instance.config.Database;
            con.Schema = Global.Instance.config.Schema;
            con.Charset = Global.Instance.config.Charset;
            con.Unicode = Global.Instance.config.Unicode;
            con.ConnectionTimeout = Global.Instance.config.ConnectionTimeout;
            //con.ConnectionString = String.Format("HOST={0};PORT={1};PROTOCOL=3;DATABASE={2};USER ID={3};POOLING=True;Connection Lifetime=0;Min Pool Size=1;Max Pool Size=1024;INTEGRATED SECURITY=False;Password={4};Charset=UTF8",
            //    Global.Instance.config.Host, Global.Instance.config.Port, Global.Instance.config.Database, Global.Instance.config.User, Global.Instance.config.Password);
            return con;
        }

        protected void extractPackages(packageInfo p)
        {
            try
            {
                log.Info("Start extracting package " + p.path);
                string outDir = Global.Instance.config.OutPutDirectory;
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                byte[] zipBytes = File.ReadAllBytes(p.path);
                byte[] xtdd_file;
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
                    using (BinaryReader br = new BinaryReader(s1))
                        xtdd_file = br.ReadBytes((int)xtddEntry.Size);
                }
                string extacted_name = Path.Combine(outDir, p.uid + ".xtdd");
                File.WriteAllBytes(extacted_name, xtdd_file);
                Console.WriteLine(extacted_name);
                log.Info("Coping package to " + extacted_name);
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }



        string GetSqlBodyCommand()
        {

            return @"FROM ""поручениеобработкиотчетности"" as model 
	            inner join reportpackage report on
		            model.reportpackageid = report.id 
	            inner join информацияопакете info on
		            model.packageinfoid = info.id
	            where
		            model.актуальныйСтатус = 'IncomingNumberAssotiated' and
		            info.rootname like :pattern and
		            info.modelname = :model and
                    :startDate <= report.creationDate and
                    report.creationDate <= :endDate";
        }
    }
}
