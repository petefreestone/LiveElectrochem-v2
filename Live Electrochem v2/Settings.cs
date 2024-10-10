using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Live_Electrochem
{
    [Serializable]
    public class Settings
    {
        private string CurrentVersion_;
        public string CurrentVersion
        {
            get { return CurrentVersion_; }
            set { CurrentVersion_ = value; }
        }

        private string RecentFolder_;
        public string RecentFolder
        {
            get { return RecentFolder_; }
            set { RecentFolder_ = value; }
        }

        public bool IsExtensionFilterEnabled { get; set; }
        public string ExtensionFilterString { get; set; }
    }


}
