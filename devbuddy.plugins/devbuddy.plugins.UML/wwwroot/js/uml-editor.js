// uml-editor.js - Editor UML basato su mxGraph

let editor = null;
let graph = null;
let toolbar = null;
let outline = null;
let undoManager = null;

export const umlEditor = (function () {

    function initialize(containerId, paletteId) {
        try {
            console.log("Inizializzazione editor UML con mxGraph...");

            // Verifica che mxGraph sia caricato
            if (typeof mxClient === 'undefined') {
                // Carica dinamicamente mxGraph se non è già caricato
                console.log("mxGraph non trovato, caricamento in corso...");
                loadMxGraph().then(() => {
                    initializeMxGraph(containerId, paletteId);
                }).catch((error) => {
                    console.error("Errore nel caricamento di mxGraph:", error);
                    return false;
                });
            } else {
                return initializeMxGraph(containerId, paletteId);
            }

            return true;
        } catch (error) {
            console.error("Errore nell'inizializzazione dell'editor UML:", error);
            return false;
        }
    }

    function loadMxGraph() {
        return new Promise((resolve, reject) => {
            // Carica lo script di mxGraph
            const script = document.createElement('script');
            script.src = '/_content/devbuddy.plugins.UML/js/mxgraph/mxClient.js';
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);

            // Carica il CSS di mxGraph
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.type = 'text/css';
            link.href = '/_content/devbuddy.plugins.UML/js/mxgraph/mxgraph.css';
            document.head.appendChild(link);
        });
    }

    function initializeMxGraph(containerId, paletteId) {
        try {
            // Disabilita avvisi browser
            mxClient.NO_FO = true;

            // Configura la lingua
            mxResources.add('/_content/devbuddy.plugins.UML/js/mxgraph/resources/editor');
            mxResources.add('/_content/devbuddy.plugins.UML/js/mxgraph/resources/graph');
            mxResources.main = 'it';

            // Crea l'editor
            editor = new mxEditor();
            graph = editor.graph;

            // Configura il container del diagramma
            const container = document.getElementById(containerId);
            if (!container) {
                console.error("Container del diagramma non trovato:", containerId);
                return false;
            }

            // Crea il container dell'editor
            mxEvent.disableContextMenu(container);

            // Configura il grafo
            graph.setEnabled(true);
            graph.setPanning(true);
            graph.setTooltips(true);
            graph.setConnectable(true);
            graph.setCellsEditable(true);
            graph.setAllowDanglingEdges(false);
            graph.setHtmlLabels(true);
            graph.setDropEnabled(true);

            // Abilita la selezione multipla con il tasto SHIFT
            new mxRubberband(graph);

            // Configura lo stile predefinito
            const defaultStyle = graph.getStylesheet().getDefaultVertexStyle();
            defaultStyle[mxConstants.STYLE_FONTCOLOR] = '#000000';
            defaultStyle[mxConstants.STYLE_FILLCOLOR] = '#FFFFFF';
            defaultStyle[mxConstants.STYLE_STROKECOLOR] = '#000000';
            defaultStyle[mxConstants.STYLE_STROKEWIDTH] = '1';

            // Configura lo stile predefinito per i collegamenti
            const defaultEdgeStyle = graph.getStylesheet().getDefaultEdgeStyle();
            defaultEdgeStyle[mxConstants.STYLE_STROKECOLOR] = '#000000';
            defaultEdgeStyle[mxConstants.STYLE_FONTCOLOR] = '#000000';
            defaultEdgeStyle[mxConstants.STYLE_STROKEWIDTH] = '1';

            // Configura la griglia
            graph.gridSize = 10;
            graph.gridEnabled = true;

            // Abilita lo snap to grid
            mxGraphHandler.prototype.guidesEnabled = true;
            mxGraphHandler.prototype.useGrid = true;

            // Abilita il disegno degli snap lines
            mxGraphHandler.prototype.guidesEnabled = true;

            // Crea la palette
            const paletteContainer = document.getElementById(paletteId);
            if (!paletteContainer) {
                console.error("Container della palette non trovato:", paletteId);
                return false;
            }

            // Crea le palette con le forme UML
            createUMLPalette(paletteContainer);

            // Configura l'undo/redo manager
            undoManager = new mxUndoManager();
            const listener = function (sender, evt) {
                undoManager.undoableEditHappened(evt.getProperty('edit'));
            };
            graph.getModel().addListener(mxEvent.UNDO, listener);
            graph.getView().addListener(mxEvent.UNDO, listener);

            // Associa scorciatoie da tastiera
            registerKeyBindings();

            console.log("Inizializzazione UML editor completata con successo");
            return true;
        } catch (error) {
            console.error("Errore nell'inizializzazione dell'editor mxGraph:", error);
            return false;
        }
    }

    function createUMLPalette(container) {
        // Crea la palette UML
        const umlPalette = new mxPalette(container);
        umlPalette.showTooltips = true;

        // Aggiunge i vari gruppi e forme alla palette
        createBasicShapes(umlPalette);
        createUMLClassShapes(umlPalette);
        createUMLRelationships(umlPalette);
        createUseCaseShapes(umlPalette);
        createComponentShapes(umlPalette);
        createTableShapes(umlPalette);
        createMiscShapes(umlPalette);
    }

    function createBasicShapes(palette) {
        addPaletteHeader(palette, "Forme di base");

        // Rettangolo
        addPaletteItem(palette, 'Rectangle', 'Rettangolo', 100, 40, '');

        // Rettangolo arrotondato
        addPaletteItem(palette, 'RoundedRectangle', 'Rettangolo arrotondato', 100, 40,
            'rounded=1;arcSize=10;');

        // Ellisse
        addPaletteItem(palette, 'Ellipse', 'Ellisse', 40, 40, '');

        // Rombo
        addPaletteItem(palette, 'Rhombus', 'Rombo', 40, 40, '');

        // Parallelogramma
        addPaletteItem(palette, 'Parallelogram', 'Parallelogramma', 80, 40,
            'shape=parallelogram;perimeter=parallelogramPerimeter;');

        // Esagono
        addPaletteItem(palette, 'Hexagon', 'Esagono', 60, 40, 'shape=hexagon;perimeter=hexagonPerimeter;');

        // Triangolo
        addPaletteItem(palette, 'Triangle', 'Triangolo', 40, 40, 'shape=triangle;perimeter=trianglePerimeter;');

        // Cilindro
        addPaletteItem(palette, 'Cylinder', 'Cilindro', 40, 60, 'shape=cylinder;');

        // Nuvola
        addPaletteItem(palette, 'Cloud', 'Nuvola', 80, 40, 'shape=cloud;');

        // Documento
        addPaletteItem(palette, 'Document', 'Documento', 40, 60, 'shape=document;');

        // Nota adesiva
        addPaletteItem(palette, 'Note', 'Nota', 60, 40, 'shape=note;');
    }

    function createUMLClassShapes(palette) {
        addPaletteHeader(palette, "Diagramma di classe");

        // Classe UML
        const classStyle = 'shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;';
        const classVertex = addPaletteItem(palette, 'Class', 'Classe', 160, 90, classStyle);
        graph.insertVertex(classVertex, null, 'Attributi', 0, 26, 160, 26, 'text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(classVertex, null, 'Metodi', 0, 52, 160, 26, 'text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');

        // Interfaccia UML
        const interfaceStyle = 'shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;fillColor=#e1d5e7;strokeColor=#9673a6;';
        const interfaceVertex = addPaletteItem(palette, 'Interface', 'Interfaccia', 160, 70, interfaceStyle);
        graph.insertVertex(interfaceVertex, null, 'Metodi', 0, 26, 160, 26, 'text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');

        // Package UML
        const packageStyle = 'shape=folder;fontStyle=1;tabWidth=80;tabHeight=20;tabPosition=left;spacingLeft=10;align=left;verticalAlign=top;fillColor=#dae8fc;strokeColor=#6c8ebf;';
        addPaletteItem(palette, 'Package', 'Package', 100, 60, packageStyle);
    }

    function createUMLRelationships(palette) {
        addPaletteHeader(palette, "Relazioni");

        // Associazione
        addEdgePaletteItem(palette, 'Association', 'Associazione',
            'endArrow=none;html=1;');

        // Ereditarietà
        addEdgePaletteItem(palette, 'Inheritance', 'Ereditarietà',
            'endArrow=block;endFill=0;endSize=12;html=1;');

        // Implementazione
        addEdgePaletteItem(palette, 'Implementation', 'Implementazione',
            'endArrow=block;dashed=1;endFill=0;endSize=12;html=1;');

        // Dipendenza
        addEdgePaletteItem(palette, 'Dependency', 'Dipendenza',
            'endArrow=open;dashed=1;endSize=12;html=1;');

        // Aggregazione
        addEdgePaletteItem(palette, 'Aggregation', 'Aggregazione',
            'endArrow=diamondThin;endFill=0;endSize=24;html=1;');

        // Composizione
        addEdgePaletteItem(palette, 'Composition', 'Composizione',
            'endArrow=diamondThin;endFill=1;endSize=24;html=1;');

        // Freccia
        addEdgePaletteItem(palette, 'Arrow', 'Freccia',
            'endArrow=classic;html=1;');

        // Freccia bidirezionale
        addEdgePaletteItem(palette, 'BiArrow', 'Freccia bidirezionale',
            'endArrow=classic;startArrow=classic;html=1;');
    }

    function createUseCaseShapes(palette) {
        addPaletteHeader(palette, "Casi d'uso");

        // Attore
        addPaletteItem(palette, 'Actor', 'Attore', 30, 60,
            'shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;');

        // Caso d'uso
        addPaletteItem(palette, 'UseCase', "Caso d'uso", 120, 40,
            'ellipse;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;');

        // Sistema
        const systemStyle = 'shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;fillColor=#f5f5f5;strokeColor=#666666;fontColor=#333333;';
        addPaletteItem(palette, 'System', 'Sistema', 160, 90, systemStyle);

        // Boundary
        addPaletteItem(palette, 'Boundary', 'Boundary', 120, 80,
            'shape=umlBoundary;whiteSpace=wrap;html=1;');
    }

    function createComponentShapes(palette) {
        addPaletteHeader(palette, "Componenti");

        // Componente
        addPaletteItem(palette, 'Component', 'Componente', 120, 60,
            'shape=component;align=center;spacingLeft=36;');

        // Interfaccia fornita (ball)
        addPaletteItem(palette, 'ProvidedInterface', 'Interfaccia fornita', 20, 20,
            'shape=ellipse;');

        // Interfaccia richiesta (socket)
        addPaletteItem(palette, 'RequiredInterface', 'Interfaccia richiesta', 20, 20,
            'shape=requiredInterface;verticalLabelPosition=bottom;sketch=0;');

        // Nodo
        addPaletteItem(palette, 'Node', 'Nodo', 120, 80,
            'shape=cube;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=10;');

        // Artefatto
        addPaletteItem(palette, 'Artifact', 'Artefatto', 110, 70,
            'shape=note;size=15;align=left;spacingLeft=10;html=1;whiteSpace=wrap;');
    }

    function createTableShapes(palette) {
        addPaletteHeader(palette, "Tabelle");

        // Tabella standard
        const tableStyle = 'shape=table;startSize=30;container=1;collapsible=0;childLayout=tableLayout;fixedRows=1;rowLines=0;fontStyle=1;';
        const tableVertex = addPaletteItem(palette, 'Table', 'Tabella', 180, 120, tableStyle);

        // Aggiungi l'intestazione
        const headerRow = graph.insertVertex(tableVertex, null, '', 0, 0, 180, 30, 'shape=partialRectangle;collapsible=0;dropTarget=0;pointerEvents=0;fillColor=none;top=0;left=0;bottom=1;right=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(headerRow, null, 'Titolo', 0, 0, 120, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;pointerEvents=1;');
        graph.insertVertex(headerRow, null, 'H1', 120, 0, 60, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;pointerEvents=1;');

        // Aggiungi una riga di dati
        const dataRow1 = graph.insertVertex(tableVertex, null, '', 0, 30, 180, 30, 'shape=partialRectangle;collapsible=0;dropTarget=0;pointerEvents=0;fillColor=none;top=0;left=0;bottom=0;right=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(dataRow1, null, 'Valore 1', 0, 0, 120, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');
        graph.insertVertex(dataRow1, null, 'V1', 120, 0, 60, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');

        // Aggiungi una seconda riga di dati
        const dataRow2 = graph.insertVertex(tableVertex, null, '', 0, 60, 180, 30, 'shape=partialRectangle;collapsible=0;dropTarget=0;pointerEvents=0;fillColor=none;top=0;left=0;bottom=0;right=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(dataRow2, null, 'Valore 2', 0, 0, 120, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');
        graph.insertVertex(dataRow2, null, 'V2', 120, 0, 60, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');

        // Tabella entità
        const entityTableStyle = 'shape=table;startSize=30;container=1;collapsible=1;childLayout=tableLayout;fixedRows=1;rowLines=0;fontStyle=1;align=center;resizeLast=1;fillColor=#d5e8d4;strokeColor=#82b366;';
        const entityTableVertex = addPaletteItem(palette, 'EntityTable', 'Tabella entità', 160, 120, entityTableStyle);

        // Aggiungi l'intestazione
        const entityHeader = graph.insertVertex(entityTableVertex, null, 'Entità', 0, 0, 160, 30, 'shape=partialRectangle;collapsible=0;dropTarget=0;pointerEvents=0;fillColor=none;top=0;left=0;bottom=1;right=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(entityHeader, null, 'Nome entità', 0, 0, 130, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');
        graph.insertVertex(entityHeader, null, 'PK', 130, 0, 30, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');

        // Aggiungi il campo chiave primaria
        const pkRow = graph.insertVertex(entityTableVertex, null, '', 0, 30, 160, 30, 'shape=partialRectangle;collapsible=0;dropTarget=0;pointerEvents=0;fillColor=none;top=0;left=0;bottom=0;right=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;');
        graph.insertVertex(pkRow, null, 'ID', 0, 0, 130, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;');
        graph.insertVertex(pkRow, null, 'x', 130, 0, 30, 30, 'shape=partialRectangle;html=1;whiteSpace=wrap;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;overflow=hidden;align=center;');
    }

    function createMiscShapes(palette) {
        addPaletteHeader(palette, "Varie");

        // Nota adesiva
        addPaletteItem(palette, 'StickyNote', 'Nota adesiva', 80, 60,
            'shape=note;whiteSpace=wrap;html=1;backgroundOutline=1;darkOpacity=0.05;fillColor=#f5f5f5;strokeColor=#666666;fontColor=#333333;');

        // Commento
        addPaletteItem(palette, 'Comment', 'Commento', 120, 60,
            'shape=callout;whiteSpace=wrap;html=1;perimeter=calloutPerimeter;fillColor=#fff2cc;strokeColor=#d6b656;');

        // Linea tratteggiata
        addEdgePaletteItem(palette, 'DashedLine', 'Linea tratteggiata',
            'endArrow=none;dashed=1;html=1;');

        // Linea continua
        addEdgePaletteItem(palette, 'SolidLine', 'Linea continua',
            'endArrow=none;html=1;');

        // Freccia bidirezionale
        addEdgePaletteItem(palette, 'BidirectionalArrow', 'Freccia bidirezionale',
            'startArrow=classic;endArrow=classic;html=1;');
    }

    // Helper per aggiungere un'intestazione alla palette
    function addPaletteHeader(palette, title) {
        const header = document.createElement('div');
        header.className = 'uml-palette-header';
        header.textContent = title;
        palette.container.appendChild(header);
    }

    // Helper per aggiungere una forma alla palette
    function addPaletteItem(palette, name, label, width, height, style) {
        const vertex = new mxCell(label, new mxGeometry(0, 0, width, height), style);
        vertex.setVertex(true);

        addPaletteEntry(palette, vertex, name);
        return vertex;
    }

    // Helper per aggiungere una connessione alla palette
    function addEdgePaletteItem(palette, name, label, style) {
        const edge = new mxCell(label, new mxGeometry(0, 0, 0, 0), style);
        edge.setEdge(true);
        edge.geometry.setTerminalPoint(new mxPoint(0, 0), true);
        edge.geometry.setTerminalPoint(new mxPoint(100, 0), false);
        edge.geometry.relative = true;

        addPaletteEntry(palette, edge, name);
        return edge;
    }

    // Helper per aggiungere un elemento alla palette
    function addPaletteEntry(palette, cell, name) {
        const gridSize = 10;
        const tb = palette.addTemplate(name, cell);
        tb.className = 'uml-palette-item';
        tb.setAttribute('title', name);
    }

    // Registra scorciatoie da tastiera
    function registerKeyBindings() {
        // Undo (Ctrl+Z)
        const undoHandler = function (evt) {
            if (evt.keyCode === 90 && mxEvent.isControlDown(evt) && !mxEvent.isShiftDown(evt)) {
                undoManager.undo();
                mxEvent.consume(evt);
            }
        };

        // Redo (Ctrl+Y o Ctrl+Shift+Z)
        const redoHandler = function (evt) {
            if ((evt.keyCode === 89 && mxEvent.isControlDown(evt)) ||
                (evt.keyCode === 90 && mxEvent.isControlDown(evt) && mxEvent.isShiftDown(evt))) {
                undoManager.redo();
                mxEvent.consume(evt);
            }
        };

        // Delete (Delete o Backspace)
        const deleteHandler = function (evt) {
            if (evt.keyCode === 46 || evt.keyCode === 8) {
                const cells = graph.getSelectionCells();
                if (cells.length > 0) {
                    graph.removeCells(cells);
                    mxEvent.consume(evt);
                }
            }
        };

        // Aggiungi gli event listener
        mxEvent.addListener(document, 'keydown', undoHandler);
        mxEvent.addListener(document, 'keydown', redoHandler);
        mxEvent.addListener(document, 'keydown', deleteHandler);
    }

    // Funzioni pubbliche per l'API UML Editor

    function newDiagram() {
        graph.getModel().clear();
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

        const svgNode = mxUtils.getSvg(graph, background, scale, border, null, true);

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

        const svgNode = mxUtils.getSvg(graph, background, scale, border, null, true);
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