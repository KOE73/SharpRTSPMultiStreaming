using System;
using System.Linq;

namespace SharpRTSPServerMulti;

public interface IRtpSender
{
    void FeedInRawRTP(string uri, int streamType, uint rtpTimestamp, List<Memory<byte>> rtpPackets);
    bool CanAcceptNewSamples();
}
