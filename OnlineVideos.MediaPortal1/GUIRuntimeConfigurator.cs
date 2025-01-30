using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;

namespace OnlineVideos.MediaPortal1
{
    public class GUIRuntimeConfigurator
    {
        #region Types
        abstract class ItemBase
        {
            public ItemBase Parent { get; private set; }

            public ItemBase(ItemBase parent)
            {
                this.Parent = parent;
            }
        }

        class ItemGroup : ItemBase
        {
            
            private List<ItemBase> _Childs = new List<ItemBase>();

            public string Name { get; private set; }
            public ItemBase this[int iIdx] => this._Childs[iIdx];

            public int Count => this._Childs.Count;


            public ItemGroup(string strName, ItemBase parent) 
                : base(parent)
            {
                this.Name = strName;
            }

            public ItemGroup GetGroup(string strPath, char[] splitChars)
            {
                return this.getGroup(strPath.Split(splitChars, StringSplitOptions.RemoveEmptyEntries), 0);
            }

            public void AddProperty(PropertyInfo prop, RuntimeConfigAttribute attr, object o)
            {
                this._Childs.Add(new ItemProperty(prop, this, attr, o));
            }

            public void ForEach(Action<ItemBase> action)
            {
                for (int i = 0; i < this._Childs.Count; i++)
                    action(this._Childs[i]);
            }

            public override string ToString()
            {
                return this.Name;
            }


            private ItemGroup getGroup(string[] path, int iIdx)
            {
                ItemGroup gr = (ItemGroup)this._Childs.Find(item => item is ItemGroup g && g.Name == path[iIdx]);
                if (gr == null)
                {
                    gr = new ItemGroup(path[iIdx], this);
                    this._Childs.Add(gr);
                }

                if (++iIdx < path.Length)
                    return gr.getGroup(path, iIdx);
                else
                    return gr;
            }
        }

        class ItemProperty : ItemBase
        {
            public RuntimeConfigAttribute Attribute { get; private set; }
            public PropertyInfo Property { get; private set; }
            public object ObjectInstance { get; private set; }

            public ItemProperty(PropertyInfo prop, ItemBase parent, RuntimeConfigAttribute attr, object o) 
                : base(parent)
            {
                this.Property = prop;
                this.Attribute = attr;
                this.ObjectInstance = o;
            }

            public override string ToString()
            {
                return (this.Attribute.Name ?? this.Property.Name) + ": " + this.Property.GetValue(this.ObjectInstance).ToString();
            }
        }
        #endregion

        #region Private Fields
        private ItemGroup _Root = new ItemGroup("Root", null);
        private object _ObjectInstance;
        private GUIDialogMenu _Dialog;
        #endregion

        #region ctor
        public GUIRuntimeConfigurator(object o)
        {
            this._ObjectInstance = o ?? throw new NullReferenceException("[GUIRuntimeConfigurator][ctor] Object instance is null.");

            char[] splitChars = new char[] { '\\', '.', '/' };
            foreach (PropertyInfo pi in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                RuntimeConfigAttribute attr = pi.GetCustomAttribute<RuntimeConfigAttribute>();
                if (attr != null)
                {
                    if (string.IsNullOrWhiteSpace(attr.Category))
                        this._Root.AddProperty(pi, attr, o);
                    else
                        this._Root.GetGroup(attr.Category, splitChars).AddProperty(pi, attr, o);
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Show GUI configuration dialog.
        /// </summary>
        /// <param name="strHeader">Initial dialog's header.</param>
        /// <param name="dlg">GUI dialog to use.</param>
        public void ShowDialog(string strHeader = "Menu", GUIDialogMenu dlg = null)
        {
            if (dlg == null)
            {
                dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                if (dlg == null)
                    throw new Exception("[GUIRuntimeConfigurator] Failed to create GUIDialogMenu.");
            }

            this._Dialog = dlg;
            this.showDialog(strHeader, this._Root);
        }
        #endregion

        #region Private Methods
        private void showDialog(string strHeader, ItemGroup group)
        {
            int iDialogSelIdx = 1;
            while (true)
            {
                this._Dialog.Reset();
                group.ForEach(item => this._Dialog.Add(item.ToString()));
                this._Dialog.SetHeading(strHeader);
                this._Dialog.selectOption(iDialogSelIdx.ToString());
                this._Dialog.DoModal(GUIWindowManager.ActiveWindow);
                iDialogSelIdx = this._Dialog.SelectedId;

                if (iDialogSelIdx < 1)
                    return;

                ItemBase it = group[iDialogSelIdx - 1];
                if (it is ItemGroup g)
                    this.showDialog(this._Dialog.SelectedLabelText, g);
                else
                {
                    ItemProperty itemProp = (ItemProperty)it;
                    PropertyInfo pi = itemProp.Property;
                    if (pi.PropertyType == typeof(bool))
                        pi.SetValue(this._ObjectInstance, !(bool)pi.GetValue(this._ObjectInstance));
                    else if (pi.PropertyType.BaseType == typeof(Enum))
                    {
                        //Enum dialog
                        this._Dialog.Reset();
                        this._Dialog.SetHeading(itemProp.ToString());

                        string strValCurr = pi.GetValue(this._ObjectInstance).ToString();
                        int iCnt = 0;
                        foreach (string strValue in Enum.GetNames(pi.PropertyType))
                        {
                            this._Dialog.Add(strValue);

                            iCnt++;

                            if (strValCurr == strValue)
                                this._Dialog.selectOption(iCnt.ToString());
                        }

                        this._Dialog.DoModal(GUIWindowManager.ActiveWindow);

                        if (this._Dialog.SelectedId > 0)
                            pi.SetValue(this._ObjectInstance, Enum.Parse(pi.PropertyType, this._Dialog.SelectedLabelText));
                    }
                    else
                    {
                        //Keyboard dialog
                        VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
                        if (keyBoard != null)
                        {
                            keyBoard.Reset();
                            keyBoard.Label = itemProp.ToString();
                            keyBoard.Text = pi.GetValue(this._ObjectInstance).ToString();
                            keyBoard.Password = false;
                            keyBoard.DoModal(GUIWindowManager.ActiveWindow);
                            if (keyBoard.IsConfirmed)
                            {
                                try
                                {
                                    pi.SetValue(this._ObjectInstance, Convert.ChangeType(keyBoard.Text, pi.PropertyType));
                                }
                                catch
                                {
                                    Log.Instance.Error("[GUIRuntimeConfigurator] Invalid conversion to type {0}: {1}", pi.PropertyType, keyBoard.Text);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
