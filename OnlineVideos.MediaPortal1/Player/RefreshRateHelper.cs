using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace OnlineVideos.MediaPortal1.Player
{
    internal static class RefreshRateHelper
    {
        internal static List<double> fpsList = null;

        internal static double MatchConfiguredFPS(double dProbedFps)
        {
            if (fpsList == null)
            {
                fpsList = new List<double>();
                NumberFormatInfo provider = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                Settings xmlreader = new MPSettings();
                for (int i = 1; i < 100; i++)
                {
                    string strName = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_name", string.Empty);
                    if (string.IsNullOrEmpty(strName)) 
                        continue;

                    string strFps = xmlreader.GetValueAsString("general", strName + "_fps", string.Empty);
                    string[] fpsArray = strFps.Split(';');
                    foreach (string strFpsItem in fpsArray)
                    {
                        double dFps = -1;
                        double.TryParse(strFpsItem, NumberStyles.AllowDecimalPoint, provider, out dFps);
                        if (dFps > 0) 
                            fpsList.Add(dFps);
                    }
                }

                fpsList = fpsList.Distinct().ToList();
                fpsList.Sort();
            }

            if (fpsList != null && fpsList.Count > 0)
            {
                double dResult, dDiff;
                double[] diffs = new double[] { 0, 0.024, 0.03, 0.06, 1 };
                int i = 0;
                while (i < diffs.Length)
                {
                    dDiff = diffs[i++];
                    dResult = fpsList.FirstOrDefault(d => Math.Abs(d - dProbedFps) <= dDiff);
                    if (dResult > 0)
                        return dResult;
                }
            }

            return default;
        }

        internal static void ChangeRefreshRateToMatchedFps(double matchedFps, string file)
        {
            Log.Instance.Info("Changing RefreshRate for matching configured FPS: {0}", matchedFps);
            RefreshRateChanger.SetRefreshRateBasedOnFPS(matchedFps, file, RefreshRateChanger.MediaType.Video);
            if (RefreshRateChanger.RefreshRateChangePending)
            {
                TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
                if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX)
                {
                    Log.Instance.Info("RefreshRateChanger.DelayedRefreshrateChanger: waited {0}s for refreshrate change, but it never took place (check your config). Proceeding with playback.", RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
                    RefreshRateChanger.ResetRefreshRateState();
                }
                else
                {
                    Log.Instance.Info("RefreshRateChanger.DelayedRefreshrateChanger: waited {0}s for refreshrate change. Proceeding with playback.", ts.TotalSeconds);
                }
            }
        }
    }
}
