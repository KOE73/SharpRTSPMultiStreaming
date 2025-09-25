using Rtsp.Messages;
using System;
using System.Linq;

namespace SharpRTSPServerMulti;

public static class RtspUriUtils
{
    public static string? GetBasePath(this RtspRequest message)
    {
        return message.RtspUri?.GetBasePath();
    }

    public static string GetBasePath(this Uri uri)
    {
        var path = uri.AbsolutePath;
        var index = path.IndexOf("/trackID=", StringComparison.OrdinalIgnoreCase);
        var basePath = index > 0 ? path.Substring(0, index) : path;
        return basePath.TrimStart('/');
    }

    public static int? GetTrackId(this RtspRequest message)
    {
        return message.RtspUri?.GetTrackId();
    }
    public static int? GetTrackId(this Uri uri)
    {
        var path = uri.AbsolutePath;
        var index = path.IndexOf("/trackID=", StringComparison.OrdinalIgnoreCase);
        if(index < 0) return null;

        var idStr = path.Substring(index + "/trackID=".Length);
        return int.TryParse(idStr, out int id) ? id : null;
    }
}