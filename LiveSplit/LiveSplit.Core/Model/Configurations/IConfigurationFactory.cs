using LiveSplit.Model.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Model.Configurations
{
    public interface IConfigurationFactory
    {
        string Name { get; }
        
        IConfiguration Create(XmlNode settings = null);
    }
}
