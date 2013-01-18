using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Config;
using System.IO;

namespace package_extractor
{
    class Program
    {
        public static readonly ILog log = LogManager.GetLogger("log");

        static void Main(string[] args)
        {
            try
            {
                XmlConfigurator.Configure(new FileInfo("log.config"));
                if (args.Length > 1)
                    throw new Exception("Error parameters");
                else if (args.Length == 1)
                {
                    if (args[0].Equals("-generate_default_config"))
                        generateDefaultConfig();
                    if (args[0].Equals("-help"))
                        printHelp();
                }
                else
                {
                    Global.Instance.config.load();
                    ICommand command = new ExtractorFromList();
                    command.Execute();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }

        static void generateDefaultConfig()
        {
            Global.Instance.config.generateDefault();
        }

        static void printHelp()
        {
            Console.WriteLine("Parameters List:");
            Console.WriteLine("-generate_default_config         generating default config");
        }
    }
}
