using Newtonsoft.Json.Linq;
using OnlineVideos.Sites.Ard.Models;
using OnlineVideos.Sites.Zdf;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OnlineVideos.Sites.Ard.Json
{
    internal class ArdMediaStreamsV6Deserializer : ArdWidgetTeaserDeserializerBase
    {
        protected static readonly string ELEMENT_MEDIACOLLECTION = "mediaCollection";

        public IEnumerable<DownloadDetailsDto> ParseWidgets(JToken jsonElement)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);
            var firstWidget = TrySelectWidgetElements(widgetsElement, 1).FirstOrDefault();

            var mediaCollection = GetMediaCollectionObject(firstWidget);

            if (mediaCollection is null)
            {
                return Enumerable.Empty<DownloadDetailsDto>();
            }

            var videosStandard = ExtractVideoStreams(mediaCollection, "main", "standard", "video/mp4", "deu");
            var videosAdaptive = ExtractVideoStreams(mediaCollection, "main", "standard", "application/vnd.apple.mpegurl", "deu");
            var videosAD = ExtractVideoStreams(mediaCollection, "main", "audio-description", "video/mp4", "deu");
            var videosDGS = ExtractVideoStreams(mediaCollection, "sign-language", "standard", "video/mp4", "deu");
            var videosOV = ExtractVideoStreams(mediaCollection, "main", "standard", "video/mp4", "OV");
            var subtitle = ExtractVttSubtitle(mediaCollection);

            return videosAdaptive
                .Concat(videosStandard)
                .Concat(videosAD)
                .Concat(videosDGS)
                .Concat(videosOV)
                .Select(m => new DownloadDetailsDto(GetVideoQuality(m), m.GetUrlWithoutQuery()) { MimeType = m.MimeType, SubtitleUrl = subtitle?.Url });
        }


        public static int? GetDuration(JToken widgetElement)
        {
            var mediaCollection = GetMediaCollectionObject(widgetElement as JObject);
            return mediaCollection?.Metadata?.DurationSeconds;
        }

        private static Embedded GetMediaCollectionObject(JObject playerPageObject)
        {
            var mediaCollectionToken = playerPageObject?[ELEMENT_MEDIACOLLECTION];
            if (mediaCollectionToken?.HasValues is true)
            {
                return mediaCollectionToken.ToObject<MediaCollectionV6>()?.Embedded;
            }
            return null;
        }

        private List<Medium> ExtractVideoStreams(Embedded mediaCollection, string streamType, string audioType, string mimeType, string languageType)
        {
            if (mediaCollection?.Streams is not { Length: > 0 } streams)
            {
                return new List<Medium>();
            }

            var mediums = new List<Medium>();
            foreach (var streamsCategory in streams.Where(s => StreamHasMedia(s) && StreamHasValidType(s, streamType)))
            {
                var media = streamsCategory.Media;
                foreach (var video in media.Where(v => v.MimeType is { Length: > 0 } mimeType && string.Equals(mimeType, mimeType, StringComparison.OrdinalIgnoreCase)))
                {
                    if (video.Url is { Length: > 0 } && video.MaxHResolutionPx is { } && VideoHasAudioOfType(video, audioType, out var langCode) && LanguageCodeMatches(langCode, languageType))
                    {
                        mediums.Add(video);
                    }
                }
            }

            return mediums;

            static bool StreamHasMedia(Stream stream)
            {
                return stream?.Media is { Length: > 0 };
            }

            static bool StreamHasValidType(Stream stream, string streamType)
            {
                return stream?.Kind is { } streamKind && string.Equals(streamType, streamKind, StringComparison.OrdinalIgnoreCase);
            }

            static bool VideoHasAudioOfType(Medium video, string audioType, out string languageCode)
            {
                var audio = video.Audios?.FirstOrDefault(a => a.Kind is { Length: > 0 } audioKind && string.Equals(audioType, audioKind, StringComparison.OrdinalIgnoreCase));
                languageCode = audio?.LanguageCode;
                return audio is { };
            }

            static bool LanguageCodeMatches(string languageCode, string expectedLanguageCode)
            {
                return languageCode is { } &&
                    (string.Equals(languageCode, expectedLanguageCode, StringComparison.OrdinalIgnoreCase)
                    || (string.Equals("*", expectedLanguageCode, StringComparison.Ordinal) && !string.Equals(languageCode, "deu", StringComparison.OrdinalIgnoreCase)));
            }
        }

        private SubtitleSource? ExtractVttSubtitle(Embedded mediaCollection)
        {
            foreach (var subtitle in mediaCollection.Subtitles ?? Enumerable.Empty<Subtitle>())
            {
                var source = subtitle.Sources.FirstOrDefault();
                if (source?.Kind.Contains("vtt") is true)
                {
                    return source;
                }
            }
            return null;
        }

        private static Qualities GetVideoQuality(Medium? medium) => medium?.MaxHResolutionPx switch
        {
            >= 2160 => Qualities.UHD,
            >= 1280 => Qualities.HD,
            >= 720 => Qualities.Normal,
            _ => Qualities.Small
        };

    }
}
