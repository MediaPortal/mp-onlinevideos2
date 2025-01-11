using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OnlineVideos.MediaPortal1
{
    public partial class PreferredListControl : UserControl
    {
        private Type _Type;

        public PreferredListControl()
        {
            this.InitializeComponent();
        }

        public void Init<TEnum>(IEnumerable<TEnum> list)
        {
            this._Type = typeof(TEnum);

            if (list != null)
            {
                foreach (TEnum item in list)
                {
                    string str = item.ToString();
                    if (str.Equals("default", StringComparison.OrdinalIgnoreCase))
                        continue;

                    this.add(str);
                }
            }

            this.reloadCombobox();
            if (this.comboBox.Items.Count > 0)
                this.comboBox.SelectedIndex = 0;
        }

        public TEnum[] GetList<TEnum>()
        {
            TEnum[] result = new TEnum[this.listBox.Items.Count];
            for (int i = 0; i < this.listBox.Items.Count; i++)
                result[i] = (TEnum)Enum.Parse(typeof(TEnum), (string)this.listBox.Items[i]);
            return result;
        }

        private bool add(string item)
        {
            if (!this.listBox.Items.Contains(item))
            {
                this.listBox.Items.Add(item);
                return true;
            }

            return false;
        }

        private void reloadCombobox()
        {
            int iSel = this.comboBox.SelectedIndex;
            this.comboBox.Items.Clear();

            string[] names = Enum.GetNames(this._Type);
            for (int i = 0; i < names.Length; i++)
            {
                string str = names[i];
                if (str.Equals("default", StringComparison.OrdinalIgnoreCase) || this.listBox.Items.Contains(str))
                    continue;


                this.comboBox.Items.Add(str);
            }

            if (this.comboBox.Items.Count == 0)
                this.comboBox.Enabled = false;
            else
            {
                if (iSel >= 0 && iSel < this.comboBox.Items.Count)
                    this.comboBox.SelectedIndex = iSel;
                else
                    this.comboBox.SelectedIndex = 0;

                this.comboBox.Enabled = true;
            }
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            if (this.comboBox.SelectedIndex >= 0 && this.add((string)this.comboBox.Items[this.comboBox.SelectedIndex]))
                this.reloadCombobox();
                
        }

        private void toolStripButtonRemove_Click(object sender, EventArgs e)
        {
            if (this.listBox.SelectedItem != null)
            {
                this.listBox.Items.Remove(this.listBox.SelectedItem);
                this.reloadCombobox();
            }    
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            if (this.listBox.SelectedItem != null)
            {
                int iIdx = this.listBox.Items.IndexOf(this.listBox.SelectedItem);
                if (iIdx > 0)
                {
                    object o = this.listBox.SelectedItem;
                    this.listBox.Items.RemoveAt(iIdx--);
                    this.listBox.Items.Insert(iIdx, o);
                    this.listBox.SelectedIndex = iIdx;
                }
            }
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            if (this.listBox.SelectedItem != null)
            {
                int iIdx = this.listBox.Items.IndexOf(this.listBox.SelectedItem);
                if (iIdx < this.listBox.Items.Count - 1)
                {
                    object o = this.listBox.SelectedItem;
                    this.listBox.Items.RemoveAt(iIdx++);
                    this.listBox.Items.Insert(iIdx, o);
                    this.listBox.SelectedIndex = iIdx;
                }
            }
        }
    }
}
