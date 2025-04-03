namespace devbuddy.common.Models
{
    public class BrowserInfo
    {
        public string UserAgent { get; set; }
        public string Platform { get; set; }
        public string Language { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int ColorDepth { get; set; }
        public string Timezone { get; set; }
        public int TouchPoints { get; set; }
        public bool CookiesEnabled { get; set; }
        public bool LocalStorage { get; set; }
        public bool SessionStorage { get; set; }
        public int MaxTouchPoints { get; set; }
        public bool CookieEnabled { get; set; }
        public string CanvasFingerprint { get; set; }
        public string InstalledFonts { get; set; }
        public string WebGLVendor { get; set; }
        public string WebGLRenderer { get; set; }
        public int HardwareConcurrency { get; set; }
        public float DeviceMemory { get; set; }
        public string Plugins { get; set; }
        public bool AudioContext { get; set; }
        public string CpuClass { get; set; }
        public bool Bluetooth { get; set; }
        public bool Credentials { get; set; }
        public string Connection { get; set; }
        public string DeviceTiming { get; set; }
    }
}
