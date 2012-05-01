using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Greenshot.Plugin;
using GreenshotPlugin.Controls;
using GreenshotPlugin.Core;
using IniFile;
using log4net;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GreenshotFogbugzPlugin
{
    public class FogbugzPlugin : IGreenshotPlugin
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(FogbugzPlugin));
        private ILanguage lang = Language.GetInstance();
        private IGreenshotPluginHost host;
        private ICaptureHost captureHost;
        private FogBugzConfiguration config;  
        public static PluginAttribute FogbugzPluginAttribute;

        public virtual bool Initialize(IGreenshotPluginHost pluginHost, ICaptureHost captureHost, PluginAttribute myAttributes)
        {
            this.host = pluginHost;
            this.captureHost = captureHost;
            FogbugzPlugin.FogbugzPluginAttribute = myAttributes;
            this.host.OnImageEditorOpen += new OnImageEditorOpenHandler(this.ImageEditorOpened);
            this.config = IniConfig.GetIniSection<FogBugzConfiguration>();
            return true;
        }

        public virtual void Shutdown()
        {
            FogbugzPlugin.LOG.Debug("FogBugz Plugin shutdown.");
            this.host.OnImageEditorOpen -= new OnImageEditorOpenHandler(this.ImageEditorOpened);
        }
        
        public virtual void Configure()
        {
            this.config.ShowConfigDialog();
        }

        public void Closing(object sender, FormClosingEventArgs e)
        {
            this.Shutdown();
        }

        private void ImageEditorOpened(object sender, ImageEditorOpenEventArgs eventArgs)
        {
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Image = GetIcon().ToBitmap();
            toolStripMenuItem.Text = this.lang.GetString(LangKey.upload_menu_item);
            toolStripMenuItem.Tag = eventArgs.ImageEditor;
            toolStripMenuItem.ShortcutKeys = (Keys.Control | Keys.F);
            toolStripMenuItem.Click += new EventHandler(this.EditMenuClick);
            PluginUtils.AddToFileMenu(eventArgs.ImageEditor, toolStripMenuItem);
        }

        public void EditMenuClick(object sender, EventArgs eventArgs)
        {
            // Get the image
            string encodedImage = string.Empty;
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            IImageEditor imageEditor = (IImageEditor)toolStripMenuItem.Tag;
            using (MemoryStream stream = new MemoryStream())
            {
                imageEditor.SaveToStream(stream, OutputFormat.png, 90);
                var data = stream.GetBuffer();
                encodedImage = System.Convert.ToBase64String(data);
            }

            Dictionary<string, string> postParams = new Dictionary<string,string>();
            postParams.Add("base64png1", System.Web.HttpUtility.UrlEncode(encodedImage));

            StringBuilder encodedParameters = new StringBuilder();
            foreach (var postParam in postParams)
            {
                encodedParameters.AppendFormat("{0}={1}&", postParam.Key, postParam.Value);
            }

            BackgroundForm backgroundForm = BackgroundForm.ShowAndWait(FogbugzPlugin.FogbugzPluginAttribute.Name, this.lang.GetString(LangKey.uploading_message));

            try
            {
                FogBugzWebClient client = new FogBugzWebClient();
                client.Headers["Content-type"] = "application/x-www-form-urlencoded";
                client.Headers["Accept-Encoding"] = "gzip, deflate";
                var loginResult = client.UploadString(this.config.Url, string.Format("sPerson={0}&sPassword={1}&pre=preLogon&dest=pg%3DpgAbout", this.config.Username, this.config.GetClearPassword()));
                client.Headers["Content-type"] = "application/x-www-form-urlencoded";
                client.Headers["Accept-Encoding"] = "";
                var result = client.UploadString(this.config.Url + "?pg=pgSubmitScreenshot&fSaveOnly=1&fEmail=0&fNewCase=1&cImageFragments=1", encodedParameters.ToString());
            
                // Parse and get the id
                var idMatch = Regex.Match(result, @"\>[\d]+\<");
                if (idMatch.Success)
                {
                    int id = Convert.ToInt32(idMatch.Value.Substring(1, idMatch.Value.Length - 2));
                    string finalUrl = string.Format("{0}?pg=pgSubmitScreenshot&fNewCase=1&fEmail=0&ixScreenshot={1}", this.config.Url, id);
                    System.Diagnostics.Process.Start(finalUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.lang.GetString(LangKey.upload_failure) + " " + ex.Message);
            }
            finally
            {
                backgroundForm.CloseDialog();
            }
        }

        public static Icon GetIcon()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GreenshotFogbugzPlugin.fogbugz.ico"))
            {
                if (stream == null)
                    return null;
                return new Icon(stream);
            }
        }
    }
}
