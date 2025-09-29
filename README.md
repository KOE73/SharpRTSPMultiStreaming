

# SharpRTSP wrapper

This project is based on [SharpRealTimeStreaming](https://github.com/jimm98y/SharpRealTimeStreaming),  
which itself is a thin wrapper around the fantastic [SharpRTSP](https://github.com/ngraziano/SharpRTSP).

The main addition here is **SharpRTSPServerMulti** —  
a simple RTSP server that supports multiple streams, each mapped to its own URI.

## Components
### New
- **SharpRTSPServerMulti** — RTSP server with multi-URI support (multiple streams), e.g. rtsp://ip/Cam1, rtsp://ip/Cam2 etc.
- **RTSPServerMultiApp** — Demo.
 
### From SharpRealTimeStreaming
- **SharpRTSPClient** — Simple RTSP client with support for H264, H265, H266, AV1 video and AAC, Opus, PCMU, PCMA audio.
- **SharpRTSPServer** — Simple RTSP server (single stream).
- **FFmpeg RTSP Server** — Sample RTSP server for FFmpeg RTP streams (configurable via `appsettings.json`).
- **Pcap RTSP Server** — Proof-of-concept for replaying RTSP from a Wireshark PcapNg file.

## Credits

- Most of the heavy lifting is done by [SharpRTSP](https://github.com/ngraziano/SharpRTSP).  
- The original wrapper is [SharpRealTimeStreaming](https://github.com/jimm98y/SharpRealTimeStreaming).  
- This fork adds **SharpRTSPServerMulti** for multi-stream URI support.
