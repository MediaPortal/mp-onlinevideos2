using Newtonsoft.Json.Linq;
using OnlineVideos.Sites.Zdf;
using System.Collections.Generic;
using System.Linq;

namespace OnlineVideos.Sites.Ard
{
    //ArdFilmDeserialize (ArdFilmInfoDto) -> ArdVideoInfoJsonDeserializer (ArdVideoDTO) --> ArdMediaArrayToDownloadUrlsConverter ( Map<Qualities, URL> )

    internal class ArdMediaArrayConverter
    {
        private static readonly string ELEMENT_MEDIA_ARRAY = "_mediaArray";
        private static readonly string ELEMENT_STREAM = "_stream";
        private static readonly string ELEMENT_MEDIA_STREAM_ARRAY = "_mediaStreamArray";

        private static readonly string ELEMENT_PLUGIN = "_plugin";
        private static readonly string ELEMENT_QUALITY = "_quality";
        private static readonly string ELEMENT_SORT_ARRAY = "_sortierArray";


        public IEnumerable<DownloadDetailsDto> ParseVideoUrls(/*DownloadDto dto,*/ JObject jsonElement)
        {
            var pluginValue = GetPluginValue(jsonElement);
            var mediaArray = jsonElement?[ELEMENT_MEDIA_ARRAY] as JArray;
            return ParseMediaArray(pluginValue, mediaArray);
        }

        private static int GetPluginValue(JObject jsonElement)
        {
            var pluginArray = jsonElement?[ELEMENT_SORT_ARRAY] as JArray;
            return pluginArray?.Values<int>()?.FirstOrDefault() ?? 1;
        }

        private IEnumerable<DownloadDetailsDto> ParseMediaArray(int pluginValue, JArray mediaArray)
        {
            foreach (var element in mediaArray.Where(item => item.Value<int>(ELEMENT_PLUGIN) == pluginValue).Select(item => item[ELEMENT_MEDIA_STREAM_ARRAY]))
            {
                foreach (var downloadDetail in ParseMediaStreamArray(element as JArray))
                {
                    yield return downloadDetail;
                }
            }
        }

        private IEnumerable<DownloadDetailsDto> ParseMediaStreamArray(JArray mediaStreamArray)
        {
            foreach (var videoElement in mediaStreamArray)
            {
                var quality = ParseVideoQuality(videoElement);
                foreach (var downloadDetail in ParseMediaStreamStream(videoElement, quality))
                {
                    yield return downloadDetail;
                }
            }
        }

        private Qualities ParseVideoQuality(JToken quality)
        {
            string ardQuality = quality?[ELEMENT_QUALITY].ToString();
            var qualityValue = ardQuality switch
            {
                "0" => Qualities.Small,
                "1" => Qualities.Small,
                "2" => Qualities.Normal,
                "3" => Qualities.High,
                "4" => Qualities.HD,
                _ => Qualities.Small,
            };
            return qualityValue;
        }

        private static IEnumerable<DownloadDetailsDto> ParseMediaStreamStream(JToken videoElement, Qualities quality)
        {
            var videoObject = videoElement as JObject;
            var streamObject = videoObject?[ELEMENT_STREAM];
            if (streamObject != null)
            {
                if (streamObject.Type == JTokenType.String)
                {
                    var url = streamObject.Value<string>();
                    yield return new DownloadDetailsDto(quality, url);
                }
                else if (streamObject.Type == JTokenType.Array)
                {
                    // TODO: Take first of same quality
                    var url = streamObject.First.Value<string>();
                    yield return new DownloadDetailsDto(quality, url);
                }
            }
        }
    }
}
