using LiveSplit.Model.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Model.Configurations
{
    public interface IConfiguration : ICloneable
    {
        string ConfigurationName { get; set; }
        Control ConfigurationControl { get; }

        /// <summary>
        /// Returns the XML serialization of the configuration's settings.
        /// </summary>
        /// <param name="document">The XML document.</param>
        /// <returns>Returns the XML serialization of the configuration's settings.</returns>
        XmlNode GetSettings(XmlDocument document);
        /// <summary>
        /// Sets the settings of the configuration based on the serialized version of the settings.
        /// </summary>
        /// <param name="settings">A serialized version of the settings that need to be set.</param>
        void SetSettings(XmlNode settings);
    }
}
