using System.Runtime.InteropServices;

namespace devbuddy.common.Models
{
    public class DesktopInfo
    {
        public string? ProcessorId { get; internal set; }
        public string? BiosSerialNumber { get; internal set; }
        public string? Product { get; internal set; }
        public string? MotherBoardSerialNumber { get; internal set; }
        public string? DiskSerialNumber { get; internal set; }
        public string MachineName { get; internal set; }
        public int ProcessCount { get; internal set; }
        public string OSDescription { get; internal set; }
        public Architecture ProcessArchitecture { get; internal set; }
    }
}
