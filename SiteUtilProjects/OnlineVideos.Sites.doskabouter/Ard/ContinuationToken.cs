using System.Collections.Generic;

namespace OnlineVideos.Sites.Ard
{
    public class ContinuationToken : Dictionary<string, object>
    {
        public ContinuationToken()
        {
        }

        public ContinuationToken(ContinuationToken otherToken) : base(otherToken)
        {
        }
    }
}
