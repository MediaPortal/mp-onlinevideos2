namespace OnlineVideos.Sites.Ard
{

    public class Result<T> //where T : ArdInformationDtoBase
    {
        public T Value { get; set; }
        public ContinuationToken ContinuationToken { get; set; }
    }
}
