using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GreenshotFogbugzPlugin
{
    public static class CasePrompt
    {
        public static CaseSelectionResult ShowDialog(string instruction, string caption)
        {
            CasePromptForm prompt = new CasePromptForm();
            prompt.Text = caption;
            prompt.Icon = FogbugzPlugin.GetIcon();
            prompt.Instruction = instruction;
            prompt.ShowDialog();
            return new CaseSelectionResult(prompt.CaseId);
        }
    }

    public class CaseSelectionResult
    {
        public bool IsNewCase
        {
            get
            {
                return this.CaseId == 0;
            }
        }

        public int CaseId { get; private set; }

        public CaseSelectionResult(string text)
        {
            if (Regex.IsMatch(text, @"\d+"))
            {
                this.CaseId = Convert.ToInt32(text);
            }
        }
    }
}
