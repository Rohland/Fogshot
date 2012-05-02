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
            if (!this.config.IsValid())
            {
                MessageBox.Show(this.lang.GetString(LangKey.configuration_error), this.lang.GetString(LangKey.configuration_error_caption), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

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

            var caseSelectionResult = CasePrompt.ShowDialog(this.lang.GetString(LangKey.case_selection), this.lang.GetString(LangKey.case_selection_caption));
            BackgroundForm backgroundForm = BackgroundForm.ShowAndWait(FogbugzPlugin.FogbugzPluginAttribute.Name, this.lang.GetString(LangKey.uploading_message));

            try
            {
                this.UploadImage(caseSelectionResult, encodedImage);
            }
            catch (Exception ex)
            {
                LOG.Error("An exception occured attempting to upload the image", ex);
                MessageBox.Show(this.lang.GetString(LangKey.upload_failure) + " " + ex.Message, this.lang.GetString(LangKey.upload_failure_caption), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                backgroundForm.CloseDialog();
            }
        }

        private void UploadImage(CaseSelectionResult caseSelection, string base64Image)
        {
            string postParams = "base64png1=" + System.Web.HttpUtility.UrlEncode(base64Image);
            FogBugzWebClient client = new FogBugzWebClient();
            client.Headers["Content-type"] = "application/x-www-form-urlencoded";
            client.Headers["Accept-Encoding"] = "gzip, deflate";

            // Login
            var loginResult = client.UploadString(this.config.Url, string.Format("sPerson={0}&sPassword={1}&pre=preLogon&dest=pg%3DpgAbout", this.config.Username, this.config.GetClearPassword()));

            // Upload screnshot
            client.Headers["Content-type"] = "application/x-www-form-urlencoded";
            client.Headers["Accept-Encoding"] = "";
            var result = client.UploadString(this.config.Url + GetUploadParameters(caseSelection.IsNewCase, caseSelection.CaseId), postParams.ToString());
            
            // Parse and get the id then launch browser at location
            var idMatch = Regex.Match(result, @"\>[\d]+\<");
            if (idMatch.Success)
            {
                int id = Convert.ToInt32(idMatch.Value.Substring(1, idMatch.Value.Length - 2));
                string finalUrl = string.Format("{0}?{1}", this.config.Url, GetLaunchParameters(caseSelection.IsNewCase, caseSelection.CaseId, id));
                System.Diagnostics.Process.Start(finalUrl);
            }
            else
            {
                LOG.Error("FogBugz didn't return a valid response for the upload command. The result was: " + result);
                MessageBox.Show(this.lang.GetString(LangKey.upload_failure), this.lang.GetString(LangKey.upload_failure_caption), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private static string GetUploadParameters(bool isNewCase, int caseId)
        {
            if (isNewCase)
            {
                return "?pg=pgSubmitScreenshot&fSaveOnly=1&fEmail=0&fNewCase=1&cImageFragments=1";
            }
            else
            {
                return string.Format("?pg=pgSubmitScreenshot&fSaveOnly=1&fEmail=0&fNewCase=0&ixBug={0}&cImageFragments=1", caseId);
            }
        }

        private static string GetLaunchParameters(bool isNewCase, int caseId, int ixScreenshotId)
        {
            if (isNewCase)
            {
                return string.Format("pg=pgSubmitScreenshot&fNewCase=1&fEmail=0&ixScreenshot={0}", ixScreenshotId);
            }
            else
            {
                return string.Format("pg=pgSubmitScreenshot&fNewCase=0&ixBug={0}&fEmail=0&ixScreenshot={1}", caseId, ixScreenshotId);
            }
        }
    }
}
