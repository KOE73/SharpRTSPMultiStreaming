using System.Text;

namespace SharpRTSPServerMulti;

public static class StringBuilderExstension
{
    /// <summary>
    /// Appends a line terminated with CRLF (`\r\n`), as required by RFC 4566 for SDP formatting.
    /// </summary>
    public static StringBuilder AppendLineRfc(this StringBuilder sb, string line) => sb.Append(line).Append("\r\n");
}
