// cookieManager.js
window.enableAnalytics = function () {
    // Codice per abilitare i cookie di analytics
    console.log("Analytics cookies enabled");

    // Esempio: inizializzazione di un servizio di analytics
    // initAnalytics();
};

// Nel file cookieManager.js
window.disableAnalytics = function () {
    // Disabilita tutti i tracking
    localStorage.setItem("analyticsEnabled", "false");

    // Rimuovi eventuali cookie di analytics esistenti
    document.cookie.split(";").forEach(function (c) {
        if (c.trim().startsWith("DevBuddy_Analytics=")) {
            document.cookie = c.trim().split("=")[0] + "=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;";
        }
    });

    // Disabilita l'invio di dati anonimi
    localStorage.setItem("anonymousDataSend", "false");
    
};

// Funzione per verificare il consenso dei cookie al caricamento della pagina
window.checkCookieConsent = async function () {
    try {
        const consent = await localStorage.getItem("CookieConsent");
        if (consent === "accepted") {
            window.enableAnalytics();
        } else if (consent === "rejected") {
            window.disableAnalytics();
        }
    } catch (error) {
        console.error("Error checking cookie consent:", error);
    }
};

// Controlla il consenso al caricamento
document.addEventListener("DOMContentLoaded", function () {
    window.checkCookieConsent();
});