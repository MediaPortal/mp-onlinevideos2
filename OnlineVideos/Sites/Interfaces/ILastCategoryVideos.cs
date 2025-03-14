using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVideos.Sites
{
    public interface ILastCategoryVideos
    {
        /// <summary>
        /// This method is called to get the latest category videos of the site.<br/>
        /// </summary>
        /// <param name="dtLastCheck">Last time when the check was executed.</param>
        /// <param name="strLastVideoUrl">Latest video url from last call.</param>
        /// <param name="category">Category to search for latest videos.</param>
        /// <returns>A list of <see cref="VideoInfo"/> objects that are new on the site.</returns>
        List<VideoInfo> GetLatestVideos(DateTime dtLastCheck, string strLastVideoUrl, Category category);

        /// <summary>
        /// This method is called to get the latest category videos of the site.<br/>
        /// </summary>
        /// <param name="dtLastCheck">Last time when the check was executed.</param>
        /// <param name="strLastVideoUrl">Latest video url from last call.</param>
        /// <param name="strCategoryLink">Category link to search for latest videos.</param>
        /// <returns>A list of <see cref="VideoInfo"/> objects that are new on the site.</returns>
        List<VideoInfo> GetLatestVideos(DateTime dtLastCheck, string strLastVideoUrl, string strCategoryLink);
    }
}
