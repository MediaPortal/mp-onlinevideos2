using System;
using Newtonsoft.Json;

namespace OnlineVideos.Sites.Ard.Models
{
    internal class MediaCollectionV6
    {
        [JsonProperty("embedded")]
        public Embedded Embedded { get; set; }
        [JsonProperty("href")]
        public string Href { get; set; }
    }


    public class Embedded
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("isGeoBlocked")]
        public bool IsGeoBlocked { get; set; }
        [JsonProperty("meta")]
        public Metadata Metadata { get; set; }
        [JsonProperty("streams")]
        public Stream[] Streams { get; set; }
        [JsonProperty("subtitles")]
        public Subtitle[] Subtitles { get; set; }
    }

    public class Stream
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("kindName")]
        public string KindName { get; set; }
        [JsonProperty("media")]
        public Medium[] Media { get; set; }
        [JsonProperty("videoLanguageCode")]
        public string VideoLanguageCode { get; set; }
    }

    public class Medium
    {
        [JsonProperty("aspectRatio")]
        public string AspectRatio { get; set; }
        [JsonProperty("audios")]
        public Audio[] Audios { get; set; }
        [JsonProperty("forcedLabel")]
        public string ForcedLabel { get; set; }
        [JsonProperty("hasEmbeddedSubtitles")]
        public bool HasEmbeddedSubtitles { get; set; }
        [JsonProperty("isAdaptiveQualitySelectable")]
        public bool IsAdaptiveQualitySelectable { get; set; }
        [JsonProperty("isHbbtv")]
        public bool IsHbbtv { get; set; }
        [JsonProperty("isHighDynamicRange")]
        public bool IsHighDynamicRange { get; set; }
        [JsonProperty("isProtocolRelative")]
        public bool IsProtocolRelative { get; set; }
        [JsonProperty("maxHResolutionPx")]
        public int? MaxHResolutionPx { get; set; }
        [JsonProperty("maxVResolutionPx")]
        public int? MaxVResolutionPx { get; set; }
        [JsonProperty("mimeType")]
        public string MimeType { get; set; }
        //[JsonProperty("subtitles")]
        //public object[] Subtitles { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("videoCodec")]
        public string VideoCodec { get; set; }
    }

    public class Audio
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }
    }

    public class Subtitle
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }
        [JsonProperty("sources")]
        public SubtitleSource[] Sources { get; set; }
    }

    public class SubtitleSource
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("availableToDateTime")]
        public DateTimeOffset AvailableToDateTime { get; set; }
        [JsonProperty("broadcastedOnDateTime")]
        public DateTimeOffset BroadcastedOnDateTime { get; set; }
        [JsonProperty("clipSourceName")]
        public string ClipSourceName { get; set; }
        [JsonProperty("durationSeconds")]
        public int DurationSeconds { get; set; }
        [JsonProperty("images")]
        public Image[] Images { get; set; }
        [JsonProperty("maturityContentRating")]
        public ContentRating MaturityContentRating { get; set; }
        [JsonProperty("publicationService")]
        public PublicationService PublicationService { get; set; }
        [JsonProperty("seriesTitle")]
        public string SeriesTitle { get; set; }
        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class ContentRating
    {
        [JsonProperty("age")]
        public int Age { get; set; }
        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }
        [JsonProperty("kind")]
        public string Kind { get; set; }
    }

    public class PublicationService
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("partner")]
        public string Partner { get; set; }
    }

    public class Image
    {
        [JsonProperty("alt")]
        public string Alt { get; set; }
        [JsonProperty("aspectRatio")]
        public string AspectRatio { get; set; }
        [JsonProperty("imageSourceName")]
        public string ImageSourceName { get; set; }
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }


}
