using System;
using System.Collections.Generic;
using System.Text;

namespace package_extractor
{
    class Global
    {
        public Config config = new Config();

        private static Global instance;

        private Global() { }

        public static Global Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Global();
                }
                return instance;
            }
        }
        
    }
}
