// uml-editor.js - Non usare export/import
window.umlEditor = (function () {
    let editor = null;
    let graph = null;
    let toolbar = null;
    let outline = null;
    let undoManager = null;

    function initialize(containerId, paletteId) {
        try {
            console.log("Inizializzazione editor UML con mxGraph...");

            // Verifica che mxGraph sia caricato
            if (typeof mxClient === 'undefined') {
                console.error("mxGraph non trovato. Assicurati che mxClient.js sia caricato.");
                return false;
            }

            if (!mxClient.isBrowserSupported()) {
                // Displays an error message if the browser is not supported.
                console.error('Browser is not supported!');
                return;
            }

            // Disabilita avvisi browser
            mxClient.NO_FO = true;

            // Inizializza mxGraph
            editor = new mxEditor();
            graph = editor.graph;

            // Configura il container del diagramma
            const container = document.getElementById(containerId);
            if (!container) {
                console.error("Container del diagramma non trovato:", containerId);
                return false;
            }

            // Imposta il container per il grafo
            editor.setGraphContainer(container);

            // Configura il grafo
            graph.setEnabled(true);
            graph.setPanning(true);
            graph.setTooltips(true);
            graph.setConnectable(true);
            graph.setCellsEditable(true);
            graph.setAllowDanglingEdges(false);
            graph.setHtmlLabels(true);

            // Crea la palette
            createPalette(paletteId);

            // Configura lo stile predefinito
            const defaultStyle = graph.getStylesheet().getDefaultVertexStyle();
            defaultStyle[mxConstants.STYLE_FILLCOLOR] = '#FFFFFF';
            defaultStyle[mxConstants.STYLE_STROKECOLOR] = '#000000';
            defaultStyle[mxConstants.STYLE_FONTCOLOR] = '#000000';

            // Abilita la selezione multipla con il tasto SHIFT
            new mxRubberband(graph);

            // Configura la griglia
            graph.setGridEnabled(true);
            graph.setGridSize(10);

            console.log("Inizializzazione UML editor completata con successo");
            return true;
        } catch (error) {
            console.error("Errore nell'inizializzazione dell'editor mxGraph:", error);
            return false;
        }
    }

    function createPalette(paletteId) {
        const paletteContainer = document.getElementById(paletteId);
        if (!paletteContainer) {
            console.error("Container della palette non trovato:", paletteId);
            return;
        }

        // Utilizziamo un approccio più semplice invece di mxPalette
        // Crea elementi della palette manualmente
        const shapes = [
            { name: "Classe", type: "class" },
            { name: "Interfaccia", type: "interface" },
            { name: "Nota", type: "note" },
            { name: "Relazione", type: "relation" },
            { name: "Attore", type: "actor" },
            { name: "Caso d'uso", type: "usecase" }
        ];

        shapes.forEach(shape => {
            const div = document.createElement("div");
            div.className = "uml-palette-item";
            div.textContent = shape.name;
            div.setAttribute("data-shape-type", shape.type);

            div.addEventListener("click", function () {
                // Quando si fa clic su un elemento della palette, aggiungilo al diagramma
                const pt = graph.getPointForEvent(event);
                addShapeToGraph(shape.type, pt.x, pt.y);
            });

            paletteContainer.appendChild(div);
        });
    }

    function addShapeToGraph(shapeType, x, y) {
        let cell;

        switch (shapeType) {
            case "class":
                cell = createClassShape(x, y);
                break;
            case "interface":
                cell = createInterfaceShape(x, y);
                break;
            case "note":
                cell = createNoteShape(x, y);
                break;
            case "actor":
                cell = createActorShape(x, y);
                break;
            case "usecase":
                cell = createUseCaseShape(x, y);
                break;
            case "relation":
                // Per le relazioni mostra un messaggio
                alert("Seleziona due elementi da collegare");
                return;
            default:
                console.error("Tipo di forma sconosciuto:", shapeType);
                return;
        }

        graph.addCell(cell);
        graph.setSelectionCell(cell);
    }

    function createClassShape(x, y) {
        // Crea una forma di classe UML
        const style = 'shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;';
        const parent = graph.getDefaultParent();
        const classVertex = graph.insertVertex(parent, null, 'NomeClasse', x, y, 160, 90, style);

        graph.insertVertex(classVertex, null, 'Attributi', 0, 26, 160, 26, 'text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(classVertex, null, 'Metodi', 0, 52, 160, 26, 'text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');

        return classVertex;
    }

    function createInterfaceShape(x, y) {
        // Crea una forma di interfaccia UML
        const style = 'shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;fillColor=#e1d5e7;strokeColor=#9673a6;';
        const parent = graph.getDefaultParent();
        const interfaceVertex = graph.insertVertex(parent, null, 'NomeInterfaccia', x, y, 160, 70, style);

        graph.insertVertex(interfaceVertex, null, 'Metodi', 0, 26, 160, 26, 'text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');

        return interfaceVertex;
    }

    function createNoteShape(x, y) {
        // Crea una nota UML
        const style = 'shape=note;whiteSpace=wrap;html=1;size=15;fillColor=#fff2cc;strokeColor=#d6b656;';
        const parent = graph.getDefaultParent();
        return graph.insertVertex(parent, null, 'Nota', x, y, 120, 60, style);
    }

    function createActorShape(x, y) {
        // Crea un attore UML
        const style = 'shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;';
        const parent = graph.getDefaultParent();
        return graph.insertVertex(parent, null, 'Attore', x, y, 30, 60, style);
    }

    function createUseCaseShape(x, y) {
        // Crea un caso d'uso UML
        const style = 'ellipse;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;';
        const parent = graph.getDefaultParent();
        return graph.insertVertex(parent, null, "Caso d'uso", x, y, 120, 40, style);
    }

    function newDiagram() {
        const parent = graph.getDefaultParent();
        graph.removeCells(graph.getChildCells(parent));
        return true;
    }

    function saveDiagram() {
        try {
            const encoder = new mxCodec();
            const node = encoder.encode(graph.getModel());
            return mxUtils.getXml(node);
        } catch (error) {
            console.error("Errore durante il salvataggio del diagramma:", error);
            return null;
        }
    }

    function loadDiagram(xmlData) {
        try {
            graph.getModel().beginUpdate();
            try {
                graph.getModel().clear();
                const doc = mxUtils.parseXml(xmlData);
                const codec = new mxCodec(doc);
                codec.decode(doc.documentElement, graph.getModel());
                return true;
            } finally {
                graph.getModel().endUpdate();
            }
        } catch (error) {
            console.error("Errore durante il caricamento del diagramma:", error);
            return false;
        }
    }

    function exportDiagram(format) {
        try {
            format = format || 'png';

            switch (format.toLowerCase()) {
                case 'png':
                    exportAsPng();
                    break;
                case 'xml':
                    exportAsXml();
                    break;
                case 'svg':
                    exportAsSvg();
                    break;
                default:
                    console.error("Formato di esportazione non supportato:", format);
                    return false;
            }

            return true;
        } catch (error) {
            console.error("Errore durante l'esportazione del diagramma:", error);
            return false;
        }
    }

    function exportAsPng() {
        const background = '#ffffff';
        const scale = 1;
        const border = 10;

        const bounds = graph.getGraphBounds();
        const width = Math.max(1, Math.ceil(bounds.width + 2 * border));
        const height = Math.max(1, Math.ceil(bounds.height + 2 * border));

        const svgNode = graph.getSvg(background, scale, border);

        // Crea una nuova immagine e imposta l'attributo src con il SVG
        const image = new Image();
        image.src = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(mxUtils.getXml(svgNode));

        // Quando l'immagine è caricata, disegnala su un canvas e scarica come PNG
        image.onload = function () {
            const canvas = document.createElement('canvas');
            canvas.width = width;
            canvas.height = height;

            const ctx = canvas.getContext('2d');
            ctx.fillStyle = background;
            ctx.fillRect(0, 0, width, height);
            ctx.drawImage(image, 0, 0);

            // Crea un link per il download
            const link = document.createElement('a');
            link.download = 'diagramma-uml.png';
            link.href = canvas.toDataURL('image/png');

            // Simula un click sul link per avviare il download
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        };
    }

    function exportAsXml() {
        const xmlData = saveDiagram();

        // Crea un Blob con i dati XML
        const blob = new Blob([xmlData], { type: 'application/xml' });

        // Crea un URL per il Blob
        const url = URL.createObjectURL(blob);

        // Crea un link per il download
        const link = document.createElement('a');
        link.download = 'diagramma-uml.xml';
        link.href = url;

        // Simula un click sul link per avviare il download
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Libera l'URL
        URL.revokeObjectURL(url);
    }

    function exportAsSvg() {
        const background = '#ffffff';
        const scale = 1;
        const border = 10;

        const svgNode = graph.getSvg(background, scale, border);
        const svgData = mxUtils.getXml(svgNode);

        // Crea un Blob con i dati SVG
        const blob = new Blob([svgData], { type: 'image/svg+xml' });

        // Crea un URL per il Blob
        const url = URL.createObjectURL(blob);

        // Crea un link per il download
        const link = document.createElement('a');
        link.download = 'diagramma-uml.svg';
        link.href = url;

        // Simula un click sul link per avviare il download
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Libera l'URL
        URL.revokeObjectURL(url);
    }

    function zoomIn() {
        graph.zoomIn();
        return true;
    }

    function zoomOut() {
        graph.zoomOut();
        return true;
    }

    function resetZoom() {
        graph.zoomActual();
        return true;
    }

    // Esporta l'API pubblica
    return {
        initialize: initialize,
        newDiagram: newDiagram,
        saveDiagram: saveDiagram,
        loadDiagram: loadDiagram,
        exportDiagram: exportDiagram,
        zoomIn: zoomIn,
        zoomOut: zoomOut,
        resetZoom: resetZoom
    };
})();