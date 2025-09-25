using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpISOBMFF;
using SharpISOBMFF.Extensions;
using SharpMP4.Readers;
using SharpRTSPServerMulti;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace RTSPServerMultiApp;

internal class RTSPServerWorker : BackgroundService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;
    private RTSPServer _server;
    private int _videoRtpBaseTime;
    private int _audioRtpBaseTime;
    private IsoStream _isoStream;

    List<Timer> _Timers = new();

    private readonly object _syncRoot = new object();

    public RTSPServerWorker(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _loggerFactory = loggerFactory;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _logger = _loggerFactory.CreateLogger<RTSPServerWorker>();

        //var hostName = _configuration["RTSPServerApp:HostName"];
        //var port = ushort.Parse(_configuration["RTSPServerApp:Port"]);
        //var userName = _configuration["RTSPServerApp:UserName"];
        //var password = _configuration["RTSPServerApp:Password"];
        var fileName = _configuration["RTSPServerApp:FilePath"];

        var serverConfig = _configuration.GetSection("RTSPServerApp").Get<ServerConfig>();

        _server = new RTSPServer(serverConfig.Port, serverConfig.UserName, serverConfig.Password, _loggerFactory);

        foreach(var uri in serverConfig.URIs)
            NewMethod(_server, uri);

        _server.StartListen();

        _logger.LogInformation($"RTSP URL is rtsp://{serverConfig.UserName}:{serverConfig.Password}@{serverConfig.HostName}:{serverConfig.Port}");

        foreach(var kv in _Timers)
            kv.Start();

        return Task.CompletedTask;
    }

    private void NewMethod(RTSPServer server, StreamConfig streamConfig)
    {
        string URI = streamConfig.URI;
        string fileName = streamConfig.FilePath;

        ITrack rtspVideoTrack = null;
        ITrack rtspAudioTrack = null;

        if(Path.GetExtension(fileName).ToLowerInvariant() == ".mp4")
        {
            Stream inputFileStream = new BufferedStream(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            _isoStream = new IsoStream(inputFileStream);
            var fmp4 = new Container();
            fmp4.Read(_isoStream);

            VideoReader inputReader = new VideoReader();
            inputReader.Parse(fmp4);
            IEnumerable<SharpMP4.Tracks.ITrack> inputTracks = inputReader.GetTracks();
            IEnumerable<byte[]> videoUnits = null;

            foreach(var inputTrack in inputTracks)
            {
                if(inputTrack.HandlerType == HandlerTypes.Video)
                {
                    videoUnits = inputTrack.GetContainerSamples();

                    if(inputTrack is SharpMP4.Tracks.H264Track)
                    {
                        var h264Track = new SharpRTSPServerMulti.H264Track(URI);
                        h264Track.SetParameterSets(videoUnits.First(), videoUnits.Skip(1).First());
                        rtspVideoTrack = h264Track;
                    }
                    else if(inputTrack is SharpMP4.Tracks.H265Track)
                    {
                        var h265Track = new SharpRTSPServerMulti.H265Track(URI);
                        h265Track.SetParameterSets(videoUnits.First(), videoUnits.Skip(1).First(), videoUnits.Skip(2).First());
                        rtspVideoTrack = h265Track;
                    }
                    else if(inputTrack is SharpMP4.Tracks.H266Track)
                    {
                        var h266Track = new SharpRTSPServerMulti.H266Track(URI);
                        h266Track.SetParameterSets(null, null, videoUnits.First(), videoUnits.Skip(1).First(), null);
                        rtspVideoTrack = h266Track;
                    }
                    else if(inputTrack is SharpMP4.Tracks.AV1Track)
                    {
                        var av1Track = new SharpRTSPServerMulti.AV1Track(URI);
                        av1Track.SetOBUs(videoUnits.ToList());
                        rtspVideoTrack = av1Track;
                    }
                    else
                    {
                        continue;
                    }

                    server.AddTrack(rtspVideoTrack);

                    _videoRtpBaseTime = Random.Shared.Next();
                    var timer = new Timer(inputTrack.DefaultSampleDuration * 1000d / inputTrack.Timescale);
                    _Timers.Add(timer);
                    timer.Elapsed += (s, e) =>
                    {
                        lock(_syncRoot)
                        {
                            var sample = inputReader.ReadSample(inputTrack.TrackID);

                            if(sample == null)
                            {
                                foreach(var track in inputReader.Tracks)
                                {
                                    track.Value.SampleIndex = 0;
                                    track.Value.FragmentIndex = 0;
                                }
                                return;
                            }

                            IEnumerable<byte[]> units = inputReader.ParseSample(inputTrack.TrackID, sample.Data);
                            rtspVideoTrack.FeedInRawSamples((uint)unchecked(_videoRtpBaseTime + sample.PTS), units.ToList());
                        }
                    };

                    break;
                }
            }

            foreach(var inputTrack in inputTracks)
            {
                if(inputTrack.HandlerType == HandlerTypes.Sound)
                {
                    if(inputTrack is SharpMP4.Tracks.AACTrack aac)
                    {
                        rtspAudioTrack = new SharpRTSPServerMulti.AACTrack(URI, aac.AudioSpecificConfig.ToBytes(), (int)aac.SamplingRate, aac.ChannelCount);
                    }
                    else if(inputTrack is SharpMP4.Tracks.OpusTrack opus)
                    {
                        rtspAudioTrack = new SharpRTSPServerMulti.OpusTrack(URI);
                    }
                    else
                    {
                        continue;
                    }

                    server.AddTrack(rtspAudioTrack);

                    _audioRtpBaseTime = Random.Shared.Next();

                    var timer = new Timer(inputTrack.DefaultSampleDuration * 1000d / inputTrack.Timescale);
                    _Timers.Add(timer);
                    timer.Elapsed += (s, e) =>
                    {
                        lock(_syncRoot)
                        {
                            var sample = inputReader.ReadSample(inputTrack.TrackID);

                            if(sample == null)
                            {
                                foreach(var track in inputReader.Tracks)
                                {
                                    track.Value.SampleIndex = 0;
                                    track.Value.FragmentIndex = 0;
                                }
                                return;
                            }

                            IEnumerable<byte[]> units = inputReader.ParseSample(inputTrack.TrackID, sample.Data);
                            rtspAudioTrack.FeedInRawSamples((uint)unchecked(_audioRtpBaseTime + sample.PTS / 2), units.ToList());
                        }
                    };

                    break;
                }
            }
        }
        else
        {
            string[] jpgFiles = Directory.GetFiles(fileName, "*.jpg");
            int jpgFileIndex = 0;
            bool process = false;

            List<byte[]> freeVideo = new List<byte[]>() { new byte[] { 0 } };

            rtspVideoTrack = new SharpRTSPServerMulti.MJpegTrack(URI);
            server.AddTrack(rtspVideoTrack);

            const int period = 100;

            var timer = new Timer(period);
            _Timers.Add(timer);
            timer.Elapsed += (s, e) =>
            {
                if(process) return;
                try
                {
                    process = true;
                    jpgFileIndex++;
                    uint rtpTimestamp = (uint)jpgFileIndex * 1000 / period;
                    // First 2 sec show image
                    if(jpgFileIndex % (20/*sec*/ * 1000 / period) <= (3/*sec*/* 1000 / period))
                    {
                        rtspVideoTrack.FeedInRawSamples(rtpTimestamp, new List<byte[]>
                        {
                            File.ReadAllBytes(jpgFiles[jpgFileIndex % jpgFiles.Length])
                        });
                    }
                    else
                    {
                        if(jpgFileIndex % (5/*sec*/ * 1000 / period) == 0)
                            rtspVideoTrack.FeedInRawSamples(rtpTimestamp, new List<byte[]>
                            {
                                File.ReadAllBytes(jpgFiles[jpgFileIndex % jpgFiles.Length])
                            });
                    }
                }
                catch(Exception ex)
                {
                }
                finally
                {
                    process = false;
                }
            };

        }
    }

    public override void Dispose()
    {
        base.Dispose();

        foreach(var kv in _Timers)
        {
            kv.Stop();
            kv.Dispose();
        }


        _server?.Dispose();
    }
}

public class ServerConfig
{
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public List<StreamConfig> URIs { get; set; }
}

public class StreamConfig
{
    public string URI { get; set; }
    public string FilePath { get; set; }
}