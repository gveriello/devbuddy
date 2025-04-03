export async function getBrowserFingerprint() {
    // Funzione per generare un canvas fingerprint
    function getCanvasFingerprint() {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        canvas.width = 240;
        canvas.height = 140;

        // Testo con caratteristiche specifiche
        ctx.textBaseline = "alphabetic";
        ctx.fillStyle = "#f60";
        ctx.fillRect(125, 1, 62, 20);

        // Aggiunge un testo con un font particolare
        ctx.fillStyle = "#069";
        ctx.font = "11pt 'Segoe UI'";
        ctx.fillText("Browser Fingerprint", 2, 15);
        ctx.fillStyle = "rgba(102, 204, 0, 0.7)";
        ctx.font = "18pt Arial";
        ctx.fillText("Unique", 4, 45);

        // Aggiunge alcuni elementi grafici
        ctx.globalCompositeOperation = "multiply";
        ctx.fillStyle = "rgb(255,0,255)";
        ctx.beginPath();
        ctx.arc(50, 50, 50, 0, Math.PI * 2, true);
        ctx.closePath();
        ctx.fill();

        return canvas.toDataURL().hashCode();
    }

    // Funzione per ottenere la lista dei font installati
    async function getInstalledFonts() {
        const fontList = [
            'Arial', 'Arial Black', 'Arial Unicode MS', 'Courier New',
            'Georgia', 'Tahoma', 'Times New Roman', 'Verdana',
            'Segoe UI', 'Helvetica', 'Ubuntu', 'Roboto'
        ];

        const installedFonts = [];
        const testString = 'mmmmmmmmmmlli';
        const testSize = '72px';
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');

        for (const font of fontList) {
            ctx.font = `${testSize} ${font}, Arial`;
            const metrics = ctx.measureText(testString);
            installedFonts.push(`${font}:${metrics.width}`);
        }

        return installedFonts.join(',');
    }

    // Funzione per ottenere le caratteristiche WebGL
    function getWebGLFingerprint() {
        try {
            const canvas = document.createElement('canvas');
            const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            if (!gl) return null;

            const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
            return {
                vendor: gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL),
                renderer: gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL)
            };
        } catch (e) {
            return null;
        }
    }

    // Calcola un hash dalla stringa
    String.prototype.hashCode = function () {
        let hash = 0;
        for (let i = 0; i < this.length; i++) {
            const char = this.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }
        return hash.toString(36);
    }

    const webGLInfo = getWebGLFingerprint();
    const installedFonts = await getInstalledFonts();
    const canvasHash = getCanvasFingerprint();

    return {
        userAgent: navigator.userAgent,
        platform: navigator.platform,
        language: navigator.language,
        screenWidth: window.screen.width,
        screenHeight: window.screen.height,
        colorDepth: window.screen.colorDepth,
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        touchPoints: navigator.maxTouchPoints,
        cookiesEnabled: navigator.cookieEnabled,
        localStorage: !!window.localStorage,
        sessionStorage: !!window.sessionStorage,
        // Informazioni più uniche
        canvasFingerprint: canvasHash,
        installedFonts: installedFonts,
        webGLVendor: webGLInfo?.vendor || 'unknown',
        webGLRenderer: webGLInfo?.renderer || 'unknown',
        hardwareConcurrency: navigator.hardwareConcurrency,
        deviceMemory: navigator.deviceMemory,
        plugins: Array.from(navigator.plugins, p => `${p.name}:${p.filename}`).join(','),
        // Audio context fingerprint
        audioContext: !!window.AudioContext || !!window.webkitAudioContext,
        // CPU class
        cpuClass: navigator.cpuClass,
        // Presenza di specifiche API
        bluetooth: !!navigator.bluetooth,
        credentials: !!navigator.credentials,
        connection: navigator.connection?.type || 'unknown',
        // Performance capabilities
        deviceTiming: performance.now().toString().slice(0, 8)
    };
}