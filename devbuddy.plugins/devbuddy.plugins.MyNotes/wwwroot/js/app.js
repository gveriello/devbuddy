// Funzioni per il plugin MyNotes

window.initializeEditor = function () {
    // Ridimensiona l'area di testo in base al contenuto
    const resizeTextarea = function (textarea) {
        if (!textarea) return;

        // Auto-resize per l'altezza
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';
    };

    // Applica l'auto-resize a tutti i textarea esistenti
    document.querySelectorAll('.content-editor').forEach(textarea => {
        resizeTextarea(textarea);

        textarea.addEventListener('input', function () {
            resizeTextarea(this);
        });
    });

    // Evidenzia la sintassi per gli snippet di codice
    const highlightCode = function () {
        // Nota: questo è solo un esempio. Per una vera evidenziazione della sintassi,
        // dovresti integrare una libreria come Highlight.js o Prism.js
        console.log('Syntax highlighting should be applied here');
    };

    highlightCode();
};

// Gestione delle shortcuts da tastiera
window.setupMyNotesKeyboardShortcuts = function () {
    document.addEventListener('keydown', function (e) {
        // Ctrl+N = Nuova Nota
        if (e.ctrlKey && e.key === 'n' && document.querySelector('.mynotes-container')) {
            e.preventDefault();
            document.querySelector('.notes-actions button').click();
        }

        // Ctrl+S = Salva (già gestito dal browser ma potrebbe essere utile)
        if (e.ctrlKey && e.key === 's' && document.querySelector('.mynotes-container')) {
            e.preventDefault();
            // Qui potresti richiamare una funzione di salvataggio esplicita se necessario
            console.log('Save shortcut detected');
        }
    });
};

// Funzioni per la gestione degli appunti
window.myNotesClipboard = {
    // Copia il contenuto negli appunti
    copyToClipboard: function (text) {
        navigator.clipboard.writeText(text)
            .then(() => console.log('Content copied to clipboard'))
            .catch(err => console.error('Could not copy text: ', err));
    },

    // Legge dagli appunti
    readFromClipboard: async function () {
        try {
            return await navigator.clipboard.readText();
        } catch (err) {
            console.error('Could not read from clipboard: ', err);
            return null;
        }
    }
};