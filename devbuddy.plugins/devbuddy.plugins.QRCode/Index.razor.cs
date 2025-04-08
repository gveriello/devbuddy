using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;


namespace devbuddy.plugins.QRCodeEncoder
{
    public partial class Index : PageComponentsBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private ToastService ToastService { get; set; }

        private string QrText { get; set; } = string.Empty;
        private int QrSize { get; set; } = 200;
        private string ErrorCorrectionLevel { get; set; } = "M";
        private string DarkColor { get; set; } = "#000000";
        private string LightColor { get; set; } = "#ffffff";
        private string QrCodeImageUrl { get; set; }

        private async Task GenerateQrCode()
        {
            if (string.IsNullOrWhiteSpace(QrText))
                return;

            // Creare una nuova istanza di QRCodeGenerator
            QRCodeGenerator qrGenerator = new QRCodeGenerator();

            // Determinare il livello di correzione degli errori
            QRCodeGenerator.ECCLevel eccLevel = ErrorCorrectionLevel switch
            {
                "L" => QRCodeGenerator.ECCLevel.L,
                "M" => QRCodeGenerator.ECCLevel.M,
                "Q" => QRCodeGenerator.ECCLevel.Q,
                "H" => QRCodeGenerator.ECCLevel.H,
                _ => QRCodeGenerator.ECCLevel.M
            };

            // Generare i dati del QR Code
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(QrText, eccLevel);

            // Convertire il colore HEX in oggetti Color di System.Drawing
            System.Drawing.Color darkColor = HexToColor(DarkColor);
            System.Drawing.Color lightColor = HexToColor(LightColor);

            // Creare il QR Code come immagine PNG
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(
                pixelsPerModule: QrSize / 25 // Calcola i pixel per modulo basandosi sulla dimensione desiderata
            );

            // Convertire i byte dell'immagine in stringa base64 per l'uso nell'elemento <img>
            QrCodeImageUrl = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";

            // Forzare il re-rendering del componente
            StateHasChanged();
        }

        private async Task DownloadQrCode()
        {
            if (QrCodeImageUrl == null)
                return;

            // Utilizza JavaScript per scaricare l'immagine
            await JSRuntime.InvokeVoidAsync("eval", @"
                const link = document.createElement('a');
                link.href = '" + QrCodeImageUrl + @"';
                link.download = 'qrcode-" + DateTime.Now.ToString("yyyyMMddHHmmss") + @".png';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            ");

            ToastService.Show("QR Code scaricato con successo", ToastLevel.Success);
        }

        private System.Drawing.Color HexToColor(string hex)
        {
            // Rimuove il carattere # se presente
            hex = hex.Replace("#", "");

            // Converte la stringa HEX in valori RGB
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}