// La libreria mxGraph viene caricata da CDN
let editor = null;
let graph = null;
let dotNetRef = null;

// Inizializza l'editor di diagrammi
window.umlEditorElement = {
    initialize: function (containerElementId, dotNetReference) {
        // Salva il riferimento DotNet per le callback
        dotNetRef = dotNetReference;

        try {
            // Verifica se mxGraph è disponibile
            if (typeof mxGraph === 'undefined') {
                console.warn('mxGraph not loaded. Loading from CDN...');
                dotNetRef.invokeMethodAsync('OnEditorError', 'mxGraph not loaded');
                return false; // Restituisce specificamente un booleano
            }

            // Resto del codice...

            // Alla fine, assicuriamoci di restituire un valore booleano valido
            dotNetRef.invokeMethodAsync('OnEditorInitialized');
            return true; // Ritorna esplicitamente true
        } catch (error) {
            console.error('Error initializing diagram editor:', error);
            dotNetRef.invokeMethodAsync('OnEditorError', error.toString());
            return false; // Ritorna esplicitamente false
        }
    },

    // Aggiunge un nodo al diagramma
    addNode: function (x, y, width, height, label, style) {
        if (!graph) return null;

        const parent = graph.getDefaultParent();

        // Inizia una transazione per aggiungere il nodo
        graph.getModel().beginUpdate();

        try {
            const vertex = graph.insertVertex(
                parent,
                null,
                label || 'Node',
                x, y,
                width || 120,
                height || 60,
                style || ''
            );
            return vertex.id;
        } finally {
            graph.getModel().endUpdate();
        }
    },

    // Aggiunge un collegamento tra due nodi
    addEdge: function (sourceId, targetId, label, style) {
        if (!graph) return null;

        const parent = graph.getDefaultParent();
        const sourceCell = graph.getModel().getCell(sourceId);
        const targetCell = graph.getModel().getCell(targetId);

        if (!sourceCell || !targetCell) {
            console.error('Source or target cell not found');
            return null;
        }

        // Inizia una transazione per aggiungere il collegamento
        graph.getModel().beginUpdate();

        try {
            const edge = graph.insertEdge(
                parent,
                null,
                label || '',
                sourceCell,
                targetCell,
                style || ''
            );
            return edge.id;
        } finally {
            graph.getModel().endUpdate();
        }
    },

    // Esporta il diagramma in formato XML
    exportToXml: function () {
        if (!graph) return '';

        const encoder = new mxCodec();
        const result = encoder.encode(graph.getModel());
        return mxUtils.getXml(result);
    },

    // Importa un diagramma da XML
    importFromXml: function (xml) {
        if (!graph) return false;

        try {
            const doc = mxUtils.parseXml(xml);
            const codec = new mxCodec(doc);

            // Imposta il modello del grafico
            graph.getModel().beginUpdate();

            try {
                codec.decode(doc.documentElement, graph.getModel());
                return true;
            } finally {
                graph.getModel().endUpdate();
            }
        } catch (error) {
            console.error('Error importing diagram from XML:', error);
            return false;
        }
    },

    // Esporta il diagramma come immagine PNG
    exportToPng: function () {
        if (!graph) return '';

        const xmlDoc = mxUtils.createXmlDocument();
        const root = xmlDoc.createElement('output');
        xmlDoc.appendChild(root);

        const xmlCanvas = new mxXmlCanvas2D(root);
        const imgExport = new mxImageExport();
        imgExport.drawState(graph.getView().getState(graph.getModel().getRoot()), xmlCanvas);

        const bounds = graph.getGraphBounds();
        const w = Math.round(bounds.x + bounds.width + 4);
        const h = Math.round(bounds.y + bounds.height + 4);

        const xml = mxUtils.getXml(root);
        const svg = '<svg xmlns="http://www.w3.org/2000/svg" width="' + w + '" height="' + h +
            '" version="1.1">' + xml + '</svg>';

        // Converti SVG in PNG utilizzando una tecnica di canvas
        const canvas = document.createElement('canvas');
        canvas.width = w;
        canvas.height = h;

        const ctx = canvas.getContext('2d');
        const img = new Image();

        // Crea un URL dati SVG
        const svgUrl = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);

        // Imposta l'immagine e disegnala sul canvas quando caricata
        img.onload = function () {
            ctx.drawImage(img, 0, 0);
            const pngUrl = canvas.toDataURL('image/png');
            dotNetRef.invokeMethodAsync('OnExportPngComplete', pngUrl);
        };

        img.src = svgUrl;
        return 'processing'; // Callback gestirà l'output effettivo
    },

    // Pulisce il diagramma
    clearDiagram: function () {
        if (!graph) return false;

        graph.getModel().beginUpdate();
        try {
            graph.removeCells(graph.getChildCells(graph.getDefaultParent(), true, true));
            return true;
        } finally {
            graph.getModel().endUpdate();
        }
    },

    // Imposta lo zoom del diagramma
    setZoom: function (zoomFactor) {
        if (!graph) return false;

        graph.zoomTo(zoomFactor, true);
        return true;
    },

    // Centra il contenuto
    centerContent: function () {
        if (!graph) return false;

        graph.fit();
        graph.center();
        return true;
    }
};