using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineVideos.Helpers
{
    public class WebViewHelperInitializer : MarshalByRefObject
    {
        private Form _Form;

        public static WebViewHelperInitializer Instance;

        public WebViewHelperInitializer(Form form)
        {
            this._Form = form;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Execute()
        {
            if (this._Form.InvokeRequired)
                this._Form.Invoke(new MethodInvoker(() => this.Execute()));
            else
            {
                WebViewHelper wvh = null;
                try
                {
                    wvh = WebViewHelper.Instance;
                    if (wvh == null)
                        Log.Error("Error initializing WebViewHelper");
                    else
                    {
                        CrossDomain.OnlineVideosAppDomain.Domain.SetData(typeof(WebViewHelper).FullName, wvh);
                        Log.Debug("WebViewHelper created successfully");
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error initializing WebViewHelper: " + e.Message);
                }
            }
        }
    }
}
