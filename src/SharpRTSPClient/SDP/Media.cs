﻿using System.Collections.Generic;
using System.Globalization;

namespace Rtsp.Sdp
{
    public class Media
    {
        public Media(string mediaString)
        {
            // Example is   'video 0 RTP/AVP 26;
            var parts = mediaString.Split(' ', 4);

            if (parts.Length >= 1)
            {
                MediaType = parts[0] switch
                {
                    "video" => MediaTypes.video,
                    "audio" => MediaTypes.audio,
                    "text" => MediaTypes.text,
                    "application" => MediaTypes.application,
                    "message" => MediaTypes.message,
                    _ => MediaTypes.unknown,// standard does allow for future types to be defined
                };
            }

            if (parts.Length >= 4)
            {
                if (int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int pt))
                {
                    PayloadType = pt;
                }
                else
                {
                    PayloadType = 0;
                }
            }
        }

        // RFC4566 Media Types
        public enum MediaTypes { video, audio, text, application, message, unknown };

        public IList<Connection> Connections { get; set; } = [];

        public IList<Bandwidth> Bandwidths { get; } = [];

        public MediaTypes MediaType { get; set; }

        public int PayloadType { get; set; }

        public IList<Attribut> Attributs { get; } = [];
    }
}