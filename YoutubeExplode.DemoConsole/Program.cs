using System;
using System.Threading.Tasks;
using YoutubeExplode.DemoConsole.Utils;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.DemoConsole
{
    // This demo prompts for video ID and downloads one media stream.
    // It's intended to be very simple and straight to the point.
    // For a more involved example - check out the WPF demo.

    public static class Program
    {
        public static async Task<int> Main()
        {


            var youtube = new YoutubeClient();

            int totalVideoResult = 1;
            await foreach (var result in youtube.Search.GetResultsAsync("NFT"))
            {
                // Use pattern matching to handle different results (videos, playlists, channels)
                switch (result)
                {
                    case VideoSearchResult videoResult:
                    {
                        var id = videoResult.Id;
                        var title = videoResult.Title;
                        var duration = videoResult.Duration;
                        var totalViews = videoResult.ViewCount;

                        Console.WriteLine(title);

                        totalVideoResult += 1;

                        break;
                    }
                    //case PlaylistSearchResult playlistResult:
                    //{
                    //    var id = playlistResult.Id;
                    //    var title = playlistResult.Title;
                    //    break;
                    //}
                    //case ChannelSearchResult channelResult:
                    //{
                    //    var id = channelResult.Id;
                    //    var title = channelResult.Title;
                    //    break;
                    //}
                }

            }


            Console.Title = "YoutubeExplode Demo";


            // Read the video ID
            Console.Write("Enter YouTube video ID or URL: ");
            var videoIdOrUrl = Console.ReadLine() ?? "";

            // Get available streams and choose the best muxed (audio + video) stream
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoIdOrUrl);
            var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
            if (streamInfo is null)
            {
                // Available streams vary depending on the video and
                // it's possible there may not be any muxed streams.
                Console.Error.WriteLine("This video has no muxed streams.");
                return 1;
            }

            // Download the stream
            Console.Write(
                $"Downloading stream: {streamInfo.VideoQuality.Label} / {streamInfo.Container.Name}... "
            );

            var fileName = $"{videoIdOrUrl}.{streamInfo.Container.Name}";

            using (var progress = new InlineProgress()) // display progress in console
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);

            Console.WriteLine($"Video saved to '{fileName}'");

            return 0;
        }
    }
}
