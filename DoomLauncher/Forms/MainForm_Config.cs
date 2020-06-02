﻿using DoomLauncher.DataSources;
using DoomLauncher.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DoomLauncher
{
    public partial class MainForm
    {
        private void HandleFormClosing()
        {
            if (DataSourceAdapter != null)
            {
                IEnumerable<IConfigurationData> config = DataSourceAdapter.GetConfiguration();

                if (WindowState != FormWindowState.Minimized) //too many problems when the form is minimized, not supported
                {
                    UpdateConfig(config, "SplitTopBottom", splitTopBottom.SplitterDistance.ToString());
                    UpdateConfig(config, "SplitLeftRight", splitLeftRight.SplitterDistance.ToString());

                    UpdateConfig(config, "AppWidth", Size.Width.ToString());
                    UpdateConfig(config, "AppHeight", Size.Height.ToString());
                    UpdateConfig(config, "AppX", Location.X.ToString());
                    UpdateConfig(config, "AppY", Location.Y.ToString());
                    UpdateConfig(config, "WindowState", WindowState.ToString());
                }

                if (GameFileViewFactory.IsUsingColumnView)
                    UpdateConfig(config, "ColumnConfig", BuildColumnConfig());
                UpdateConfig(config, ConfigType.AutoSearch.ToString("g"), chkAutoSearch.Checked.ToString());
                UpdateConfig(config, "ItemsPerPage", AppConfiguration.ItemsPerPage.ToString());
            }
        }

        private void UpdateColumnConfig()
        {
            IEnumerable<IConfigurationData> config = DataSourceAdapter.GetConfiguration();
            UpdateConfig(config, "ColumnConfig", BuildColumnConfig());
        }

        private void UpdateConfig(IEnumerable<IConfigurationData> config, string name, string value)
        {
            IConfigurationData configFind = config.FirstOrDefault(x => x.Name == name);

            if (configFind == null)
            {
                DataSourceAdapter.InsertConfiguration(new ConfigurationData
                {
                    Name = name,
                    Value = value,
                    UserCanModify = false
                });
            }
            else
            {
                configFind.Value = value;
                DataSourceAdapter.UpdateConfiguration(configFind);
            }
        }

        private string BuildColumnConfig()
        {
            if (m_tabHandler != null && GameFileViewFactory.IsUsingColumnView)
            {
                List<ColumnConfig> config = new List<ColumnConfig>();

                foreach (ITabView tab in m_tabHandler.TabViews)
                    config.AddRange(tab.GetColumnConfig());

                try
                {
                    StringWriter text = new StringWriter();
                    XmlSerializer xml = new XmlSerializer(typeof(ColumnConfig[]));
                    xml.Serialize(text, config.ToArray());
                    return text.ToString();
                }
                catch (Exception ex)
                {
                    Util.DisplayUnexpectedException(this, ex);
                }
            }

            return string.Empty;
        }
    }
}
