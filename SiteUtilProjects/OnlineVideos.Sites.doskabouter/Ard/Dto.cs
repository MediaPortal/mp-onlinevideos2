using OnlineVideos.Sites.Zdf;

using System;

namespace OnlineVideos.Sites.Ard
{

    public class DownloadDetailsDto : IEquatable<DownloadDetailsDto>
    {
        public DownloadDetailsDto(Qualities quality, string url)
        {
            Quality = quality;
            //var uriBuilder = new UriBuilder(new Uri(url, true))
            //{
            //    Scheme = Uri.UriSchemeHttps,
            //    Port = -1, //default port of scheme
            //};
            //Url = uriBuilder.ToString();
            Url = url;
        }

        public string MimeType { get; set; }

        public string Language { get; set; }

        public Qualities Quality { get; }

        public string Url { get; }

        public string SubtitleUrl { get; set; }

        public bool Equals(DownloadDetailsDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return /*string.Equals(Language, other.Language, StringComparison.OrdinalIgnoreCase)*/
                   //&& string.Equals(MimeType, other.MimeType, StringComparison.OrdinalIgnoreCase)
                   /*&&*/ Quality == other.Quality;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((DownloadDetailsDto)obj);
        }

        public override int GetHashCode()
        {
            return Quality.GetHashCode();
        }

        public static bool operator ==(DownloadDetailsDto left, DownloadDetailsDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DownloadDetailsDto left, DownloadDetailsDto right)
        {
            return !Equals(left, right);
        }
    }


    public abstract class ArdInformationDtoBase
    {
        public string Id { get; }

        protected ArdInformationDtoBase(string id) => Id = id;

        public string Title { get; set; }
        public string Description { get; set; }

        public string TargetUrl { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ArdVideoInfoDto : ArdInformationDtoBase, IEquatable<ArdVideoInfoDto> //extends CrawlerUrlDTO
    {
        public int NumberOfClips { get; }

        public DateTime? AirDate { get; set; }
        public DateTime? AvailableUntilDate { get; set; }
        public int? Duration { get; set; }
        public bool IsGeoBlocked { get; set; }
        public bool IsFskBlocked { get; set; }


        public ArdVideoInfoDto(string id, int numberOfClips, string url = null) : base(id)
        {
            TargetUrl = url ?? ArdConstants.CreateItemUrl(id).AbsoluteUri;
            NumberOfClips = numberOfClips;
        }

        public bool Equals(ArdVideoInfoDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TargetUrl == other.TargetUrl && Id == other.Id && NumberOfClips == other.NumberOfClips;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArdVideoInfoDto)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TargetUrl != null ? TargetUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ NumberOfClips;
                return hashCode;
            }
        }

        public static bool operator ==(ArdVideoInfoDto left, ArdVideoInfoDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ArdVideoInfoDto left, ArdVideoInfoDto right)
        {
            return !Equals(left, right);
        }
    }


    public class ArdCategoryInfoDto : ArdInformationDtoBase, IEquatable<ArdCategoryInfoDto>
    {
        public ArdCategoryInfoDto(string id, string navigationUrl) : base(id)
        {
            //Url = ArdConstants.ITEM_URL + id;
            TargetUrl = navigationUrl;
            //Id = id;
            HasSubCategories = false;
        }

        public bool HasSubCategories { get; set; }

        public PaginationDto Pagination { get; set; }

        public bool Equals(ArdCategoryInfoDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TargetUrl == other.TargetUrl && Id == other.Id && Title == other.Title;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArdCategoryInfoDto)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TargetUrl != null ? TargetUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ArdCategoryInfoDto left, ArdCategoryInfoDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ArdCategoryInfoDto left, ArdCategoryInfoDto right)
        {
            return !Equals(left, right);
        }
    }



    public class PaginationDto
    {
        public PaginationDto(int pageSize, int totalElements, int pageNumber = 0)
        {
            PageSize = pageSize;
            TotalElements = totalElements;
            PageNumber = pageNumber;
        }

        public int PageNumber { get; set; } = 0;    
        public int PageSize { get; set; }
        public int TotalElements { get; set; }
    }

}
