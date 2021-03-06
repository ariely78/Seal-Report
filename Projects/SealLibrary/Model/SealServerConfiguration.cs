﻿//
// Copyright (c) Seal Report, Eric Pfirsch (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Data.OleDb;
using System.Data;
using System.ComponentModel;
using Seal.Converter;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.IO;
using Seal.Helpers;
using System.Text.RegularExpressions;
using Seal.Forms;
using DynamicTypeDescriptor;
using System.Windows.Forms.Design;

namespace Seal.Model
{
    public class SealServerConfiguration : RootComponent
    {
        [XmlIgnore]
        public string FilePath;

        [XmlIgnore]
        public Repository Repository;

        #region Editor

        protected override void UpdateEditorAttributes()
        {
            if (_dctd != null)
            {
                //Disable all properties
                foreach (var property in Properties) property.SetIsBrowsable(false);
                //Then enable
                GetProperty("DefaultConnectionString").SetIsBrowsable(!ForPublication);
                GetProperty("TaskFolderName").SetIsBrowsable(!ForPublication);
                GetProperty("DefaultCulture").SetIsBrowsable(!ForPublication);
                GetProperty("IsLocal").SetIsBrowsable(!ForPublication);
                GetProperty("LogoName").SetIsBrowsable(!ForPublication);
                GetProperty("WebProductName").SetIsBrowsable(!ForPublication);

                GetProperty("WebApplicationPoolName").SetIsBrowsable(ForPublication);
                GetProperty("WebApplicationName").SetIsBrowsable(ForPublication);
                GetProperty("WebPublicationDirectory").SetIsBrowsable(ForPublication);

                TypeDescriptor.Refresh(this);
            }
        }

        #endregion

        [XmlIgnore]
        public bool ForPublication = false;


        string _defaultConnectionString = "Provider=SQLOLEDB;data source=localhost;initial catalog=adb;Integrated Security=SSPI;";
        [Category("Server Settings"), DisplayName("Default Connection String"), Description("The OLE DB Default Connection String used when a new Data Source is created. The string can contain the keyword " + Repository.SealRepositoryKeyword + " to specify the repository root folder."), Id(1, 1)]
        public string DefaultConnectionString
        {
            get { return _defaultConnectionString; }
            set { _defaultConnectionString = value; }
        }

        string _taskFolderName = Repository.SealRootProductName + " Report";
        [Category("Server Settings"), DisplayName("Task Folder Name"), Description("The name of the Task Scheduler folder containg the schedules of the reports. Warning: Changing this name will affect all existing schedules !"), Id(2, 1)]
        public string TaskFolderName
        {
            get { return _taskFolderName; }
            set { _taskFolderName = value; }
        }

        string _defaultCulture = "";
        [Category("Server Settings"), DisplayName("Default Culture"), Description("The name of the default culture used when a report is created. If not specified, the current culture of the server is used."), Id(3, 1)]
        [TypeConverter(typeof(Seal.Converter.CultureInfoConverter))]
        public string DefaultCulture
        {
            get { return _defaultCulture; }
            set { _defaultCulture = value; }
        }

        bool _isLocal = false;
        [Category("Server Settings"), DisplayName("Server is local (No internet)"), Description("If true, the programs will not access to Internet for external resources. All JavaScript's will be loaded locally (no use of CDN path)."), Id(4, 1)]
        public bool IsLocal
        {
            get { return _isLocal; }
            set { _isLocal = value; }
        }

        string _logoName = "logo.jpg";
        [Category("Server Settings"), DisplayName("Logo file name"), Description("The logo file name used by the report templates. The file must be located in the folders 'C:\\ProgramData\\eAdvisor Repository\\Views\\Images' and 'C:\\inetpub\\wwwroot\\Seal\\Images'."), Id(5, 1)]
        public string LogoName
        {
            get { return _logoName; }
            set { _logoName = value; }
        }

        string _webProductName = "Seal Report";
        [Category("Web Server Settings"), DisplayName("Web Product Name"), Description("The name of the product displayed on the Web site."), Id(1, 2)]
        public string WebProductName
        {
            get { return _webProductName; }
            set { _webProductName = value; }
        }

        string _webApplicationPoolName = Repository.SealRootProductName + " Application Pool";
        [Category("Web Server IIS Publication"), DisplayName("Application Pool Name"), Description("The name of the IIS Application pool used by the web application."), Id(2, 1)]
        public string WebApplicationPoolName
        {
            get { return _webApplicationPoolName; }
            set { _webApplicationPoolName = value; }
        }
        string _webApplicationName = "/Seal";
        [Category("Web Server IIS Publication"), DisplayName("Application Name"), Description("The name of the IIS Web application."), Id(1, 1)]
        public string WebApplicationName
        {
            get { return _webApplicationName; }
            set { _webApplicationName = value; }
        }

        string _webPublicationDirectory = "";
        [Category("Web Server IIS Publication"), DisplayName("Publication directory"), Description("The directory were the web site files are published."), Id(3, 1)]
        [EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string WebPublicationDirectory
        {
            get { return _webPublicationDirectory; }
            set { _webPublicationDirectory = value; }
        }

        [XmlIgnore]
        public DateTime LastModification;
        static public SealServerConfiguration LoadFromFile(string path, bool ignoreException)
        {
            SealServerConfiguration result = null;
            try
            {
                StreamReader sr = new StreamReader(path);
                XmlSerializer serializer = new XmlSerializer(typeof(SealServerConfiguration));
                result = (SealServerConfiguration)serializer.Deserialize(sr);
                sr.Close();
                result.FilePath = path;
                result.LastModification = File.GetLastWriteTime(path);
            }
            catch (Exception ex)
            {
                if (!ignoreException) throw new Exception(string.Format("Unable to read the configuration file '{0}'.\r\n{1}", path, ex.Message));
            }
            return result;
        }


        public void SaveToFile()
        {
            SaveToFile(FilePath);
        }

        public void SaveToFile(string path)
        {
            //Check last modification
            if (LastModification != DateTime.MinValue && File.Exists(path))
            {
                DateTime lastDateTime = File.GetLastWriteTime(path);
                if (LastModification != lastDateTime)
                {
                    throw new Exception("Unable to save the Server Configuration file. The file has been modified by another user.");
                }
            }
            var xmlOverrides = new XmlAttributeOverrides();
            XmlAttributes attrs = new XmlAttributes();
            attrs.XmlIgnore = true;
            xmlOverrides.Add(typeof(RootComponent), "Name", attrs);
            xmlOverrides.Add(typeof(RootComponent), "GUID", attrs);

            XmlSerializer serializer = new XmlSerializer(typeof(SealServerConfiguration), xmlOverrides);
            StreamWriter sw = new StreamWriter(path);
            serializer.Serialize(sw, (SealServerConfiguration)this);
            sw.Close();
            FilePath = path;
            LastModification = File.GetLastWriteTime(path);
        }
    }
}
