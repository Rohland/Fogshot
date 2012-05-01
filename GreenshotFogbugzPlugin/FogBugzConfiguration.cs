using IniFile;
using System.Windows.Forms;

namespace GreenshotFogbugzPlugin
{
    [IniSection("FogBugz", Description = "Greenshot FogBugz Plugin configuration")]
    public class FogBugzConfiguration : IniSection
    {
        [IniProperty("Url", Description = "Url to FogBugz instance.", DefaultValue = "https://INSTANCE.fogbugz.com/default.asp")]
        public string Url;

        [IniProperty("Username", Description = "FogBugz username", DefaultValue = "")]
        public string Username;

        [IniProperty("Password", Description = "FogBugz Password", DefaultValue = "")]
        public string Password;

        private static string key = "Ce555578^ff03@4874&9943!a4Abe1c5323E";

        public bool ShowConfigDialog()
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.Url = this.Url;
            settingsForm.Email = this.Username;
            settingsForm.Password = string.IsNullOrEmpty(this.Password) ? "" : SymmetricEncryptionService.Decrypt(key, this.Password);
            settingsForm.Icon = FogbugzPlugin.GetIcon();
            DialogResult dialogResult = settingsForm.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                this.Url = settingsForm.Url;
                this.Username = settingsForm.Email;
                this.Password = SymmetricEncryptionService.Encrypt(key, settingsForm.Password);
                IniConfig.Save();
                return true;
            }
            return false;
        }

        public string GetClearPassword()
        {
            return SymmetricEncryptionService.Decrypt(key, this.Password);
        }
    }
}