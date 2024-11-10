using System;
using System.Collections.Generic;
using System.IO;

namespace OnlineVideos
{
    [Serializable]
    public class SubtitleList
    {
        private Dictionary<string, string> list;

        public SubtitleList()
        {
            list = null;
        }

        public SubtitleList(string subtitleText)
        {
            list = new Dictionary<string, string>() { { "unk", subtitleText } };
        }

        public bool HasItems { get { return list != null && list.Count > 0; } }

        public void Add(string language, string subtitleText)
        {
            if (list == null) list = new Dictionary<string, string>();
            var nkey = language;
            int n = 0;
            while (list.ContainsKey(nkey))
            {
                n++;
                nkey = language + n.ToString();
            }
            list.Add(nkey, subtitleText);
        }

        public void SaveSubtitles(string targetFolder, string fileName)
        {
            if (list == null) return;

            foreach (var sub in list)
            {
                Directory.CreateDirectory(targetFolder);
                var subFile = Path.Combine(targetFolder, fileName);
                File.WriteAllText(subFile + "." + sub.Key + ".srt", sub.Value, System.Text.Encoding.UTF8);
            }
        }
    }
}
