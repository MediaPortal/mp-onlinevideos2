using System;
using System.Runtime.InteropServices;
using System.Security;
using DirectShowLib;

namespace OnlineVideos.MediaPortal1.Player
{
    public class LavSplitterSourceInterfaces
    {
        [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
        Guid("C8FF17F9-5365-4F32-8AD5-6C550342C2F7"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IURLSourceFilterLAV
        {
            // Load a URL with the specified user agent and referrer
            // UserAgent and Referrer are optional, and either, both or none can be specified
            [PreserveSig]
            int LoadURL(string pszURL, string pszUserAgent, string pszReferrer);
        }

        [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
        Guid("46070104-1318-4A82-8822-E99AB7CD15C1"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IBufferInfo
        {
            // Number of Buffers
            [PreserveSig]
            uint GetCount();

            // Get Info about Buffer "i" (0-based index up to count)
            // samples: number of frames in the buffer
            // size: total size in bytes of the buffer
            [PreserveSig]
            uint GetStatus(uint i, out uint samples, out uint size);

            // Get priority of the demuxing thread
            [PreserveSig]
            uint GetPriority();
        }

        public static int GetLAVSplitterTotalBufferSize(IBaseFilter filter)
        {
            if (filter != null)
            {
                filter.GetClassID(out Guid g);
                if (g.ToString().Equals("B98D13E7-55DB-4385-A33D-09FD1BA26338", StringComparison.CurrentCultureIgnoreCase))
                {
                    IBufferInfo buffer = (IBufferInfo)filter;
                    uint wCnt = buffer.GetCount();
                    uint wTotal = 0;
                    for (uint i = 0; i < wCnt - 1; i++)
                    {
                        if (buffer.GetStatus(i, out _, out uint wSize) != 0)
                            return -1;

                        wTotal += wSize;
                    }

                    return (int)wTotal;
                }
            }

            return -1;
        }
    }
}
