using OnlineVideos.Sites.Ard.Models;
using System;

internal static class MediumExtensions
{
    /// <summary>
    /// Returns the URL of the specified medium without any query string or fragment component.
    /// </summary>
    /// <param name="medium">The medium instance from which to extract the base URL. Must not be null, and its Url property must be a
    /// non-empty, valid absolute URL.</param>
    /// <returns>A string containing the base URL without query or fragment components, or null if the medium is null or its Url
    /// property is null or empty.</returns>
    public static string? GetUrlWithoutQuery(this Medium medium)
    {
        if (medium?.Url is { Length: > 0 } url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Path);
        }
        return null;
    }
}
