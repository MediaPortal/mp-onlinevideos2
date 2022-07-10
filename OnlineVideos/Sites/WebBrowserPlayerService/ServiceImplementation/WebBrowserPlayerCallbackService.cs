﻿using OnlineVideos.Sites.Interfaces.WebBrowserPlayerService;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CoreWCF;

namespace OnlineVideos.Sites.WebBrowserPlayerService.ServiceImplementation
{
    /// <summary>
    /// The callback service implementation - track subscribers and send the callback to these
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, 
                        InstanceContextMode = InstanceContextMode.PerSession
                        /*,UseSynchronizationContext = false*/),
        System.ServiceModel.CallbackBehavior(ConcurrencyMode = System.ServiceModel.ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class WebBrowserPlayerCallbackService : IWebBrowserPlayerCallbackService
    {
        private static readonly List<IWebBrowserPlayerCallback> _subscribers = new List<IWebBrowserPlayerCallback>();

        /// <summary>
        /// New client is subscribing to the callback service
        /// </summary>
        /// <returns></returns>
        public bool Subscribe()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IWebBrowserPlayerCallback>();
                if (!_subscribers.Contains(callback))
                    _subscribers.Add(callback);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Client is unsubscribing from the callback service
        /// </summary>
        /// <returns></returns>
        public bool Unsubscribe()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IWebBrowserPlayerCallback>();
                if (_subscribers.Contains(callback))
                    _subscribers.Remove(callback);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Let the client know we're closing
        /// </summary>
        public static void OnBrowserClosing()
        {
            try
            {
                GetActiveCallbacks().ForEach(delegate(IWebBrowserPlayerCallback callback)
                {
                    callback.OnClosing();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Request that the client logs an error
        /// </summary>
        /// <param name="exceptionToLog"></param>
        public static void LogError(Exception exceptionToLog)
        {
            try
            {
                GetActiveCallbacks().ForEach(delegate(IWebBrowserPlayerCallback callback)
                {
                    callback.LogException(exceptionToLog);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Request that the client logs an info message
        /// </summary>
        /// <param name="message"></param>
        public static void LogInfo(string message)
        {
            try
            {
                GetActiveCallbacks().ForEach(delegate(IWebBrowserPlayerCallback callback)
                {
                    callback.LogInfo(message);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Send the key press to all active callback listeners
        /// </summary>
        /// <param name="keyPressed"></param>
        public static void SendKeyPress(int keyPressed)
        {
            try
            {
                GetActiveCallbacks().ForEach(delegate(IWebBrowserPlayerCallback callback)
                {
                    callback.OnKeyPress(keyPressed);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Send the wndproc to all active callback listeners, return true if any listener returned true
        /// </summary>
        /// <param name="msg"></param>
        public static bool SendWndProc(Message msg)
        {
            var result = false;
            try
            {

                GetActiveCallbacks().ForEach(delegate(IWebBrowserPlayerCallback callback)
                {
                    result = result || callback.OnWndProc(msg);
                });

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return result;
        }

        /// <summary>
        /// Maintain the list of active subscribers
        /// </summary>
        /// <returns></returns>
        private static List<IWebBrowserPlayerCallback> GetActiveCallbacks()
        {
            try
            {
                _subscribers.ForEach(delegate(IWebBrowserPlayerCallback callback)
                {
                    if (((ICommunicationObject)callback).State != CommunicationState.Opened)
                        _subscribers.Remove(callback);
                });

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return _subscribers;
        }

    }
}
