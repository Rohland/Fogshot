using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GreenshotPlugin.Core;

namespace GreenshotFogbugzPlugin
{
    public class Language : LanguageContainer, ILanguage
    {
        private static ILanguage uniqueInstance;
        public static ILanguage GetInstance()
        {
            if (Language.uniqueInstance == null)
            {
                Language.uniqueInstance = new LanguageContainer();
                Language.uniqueInstance.LanguageFilePattern = "language_fogbugzplugin-*.xml";
                Language.uniqueInstance.Load();
                Language.uniqueInstance.SetLanguage(Thread.CurrentThread.CurrentUICulture.Name);
            }
            return Language.uniqueInstance;
        }
    }

    public enum LangKey
    {
        upload_menu_item,
        uploading_message,
        upload_failure,
        upload_failure_caption,
        configuration_error,
        configuration_error_caption,
        case_selection,
        case_selection_caption
    }
}
