using devbuddy.common.Applications;
using devbuddy.common.Attributes;
using devbuddy.common.Enums;
using devbuddy.common.Services;
using devbuddy.plugins.UML.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace devbuddy.plugins.UML
{
    public partial class Index : AppComponentBase<UMLDataModel>
    {
        private string DiagramName { get; set; } = string.Empty;
        private string DiagramDescription { get; set; } = string.Empty;
        private string SelectedDiagramId { get; set; } = string.Empty;
        private string ExportFormat { get; set; } = "png";

        private List<SavedDiagram> SavedDiagrams => Model?.SavedDiagrams ?? new List<SavedDiagram>();

        // Modal references
        private ModalComponentBase saveModal;
        private ModalComponentBase loadModal;
        private ModalComponentBase exportModal;

        // JavaScript module reference
        private IJSObjectReference _jsModule;

        protected override async Task OnInitializedAsync()
        {
            // Inizializza il modello se necessario
            Model = DataModelService.ValueByKey<UMLDataModel>(nameof(UML));

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Carica lo script direttamente con JSRuntime invece di usare import
                await JSRuntime.InvokeVoidAsync("eval", @"
                // Definisci l'oggetto umlEditor direttamente
                window.umlEditor = {
                    initialize: function(containerId, paletteId) {
                        console.log('UML Editor initialized with:', containerId, paletteId);
                        
                        // Implementazione basilare per testare
                        const paletteContainer = document.getElementById(paletteId);
                        if (paletteContainer) {
                            paletteContainer.innerHTML = '<div class=""uml-palette-item"">Test Shape</div>';
                        }
                        
                        const diagramContainer = document.getElementById(containerId);
                        if (diagramContainer) {
                            diagramContainer.innerHTML = '<div style=""padding: 20px;"">UML Editor Canvas</div>';
                        }
                        
                        return true;
                    },
                    
                    // Aggiungi metodi stub per le altre funzioni richieste
                    newDiagram: function() { return true; },
                    saveDiagram: function() { return '<xml></xml>'; },
                    loadDiagram: function() { return true; },
                    exportDiagram: function() { return true; },
                    zoomIn: function() { return true; },
                    zoomOut: function() { return true; },
                    resetZoom: function() { return true; }
                };
            ");

                try
                {
                    // Ora chiama il metodo initialize
                    var result = await JSRuntime.InvokeAsync<bool>("umlEditor.initialize", "uml-diagram", "uml-palette");
                    if (!result)
                    {
                        ToastService.Show("Errore nell'inizializzazione dell'editor UML", ToastLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    ToastService.Show($"Errore nell'inizializzazione dell'editor UML: {ex.Message}", ToastLevel.Error);
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task NewDiagram()
        {
            try
            {
                await _jsModule.InvokeVoidAsync("umlEditor.newDiagram");
                DiagramName = string.Empty;
                DiagramDescription = string.Empty;
                ToastService.Show("Nuovo diagramma creato", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore nella creazione di un nuovo diagramma: {ex.Message}", ToastLevel.Error);
            }
        }

        private void SaveDiagram()
        {
            saveModal.Show();
        }

        private async Task ConfirmSave()
        {
            if (string.IsNullOrWhiteSpace(DiagramName))
            {
                ToastService.Show("Inserisci un nome per il diagramma", ToastLevel.Warning);
                return;
            }

            try
            {
                // Ottieni i dati del diagramma dall'editor
                var diagramXml = await _jsModule.InvokeAsync<string>("umlEditor.saveDiagram");

                // Genera un ID univoco o aggiorna un diagramma esistente
                string diagramId = Guid.NewGuid().ToString();
                var existingDiagram = SavedDiagrams.Find(d => d.Name == DiagramName);

                if (existingDiagram != null)
                {
                    // Aggiorna il diagramma esistente
                    existingDiagram.Content = diagramXml;
                    existingDiagram.Description = DiagramDescription;
                    existingDiagram.LastModified = DateTime.Now;
                }
                else
                {
                    // Crea un nuovo diagramma
                    var savedDiagram = new SavedDiagram
                    {
                        Id = diagramId,
                        Name = DiagramName,
                        Description = DiagramDescription,
                        Content = diagramXml,
                        LastModified = DateTime.Now
                    };

                    Model.SavedDiagrams.Add(savedDiagram);
                }

                // Salva nel data model service
                await DataModelService.AddOrUpdateAsync(nameof(UML), Model);

                ToastService.Show("Diagramma salvato con successo", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante il salvataggio del diagramma: {ex.Message}", ToastLevel.Error);
            }
        }

        private void LoadDiagram()
        {
            if (SavedDiagrams.Count == 0)
            {
                ToastService.Show("Nessun diagramma salvato", ToastLevel.Info);
                return;
            }

            SelectedDiagramId = SavedDiagrams[0].Id;
            loadModal.Show();
        }

        private async Task ConfirmLoad()
        {
            if (string.IsNullOrWhiteSpace(SelectedDiagramId))
            {
                ToastService.Show("Seleziona un diagramma da caricare", ToastLevel.Warning);
                return;
            }

            try
            {
                var selectedDiagram = SavedDiagrams.Find(d => d.Id == SelectedDiagramId);
                if (selectedDiagram != null)
                {
                    DiagramName = selectedDiagram.Name;
                    DiagramDescription = selectedDiagram.Description;

                    // Carica il diagramma nell'editor
                    var result = await _jsModule.InvokeAsync<bool>(
                        "umlEditor.loadDiagram", selectedDiagram.Content);

                    if (result)
                    {
                        ToastService.Show("Diagramma caricato con successo", ToastLevel.Success);
                    }
                    else
                    {
                        ToastService.Show("Errore durante il caricamento del diagramma", ToastLevel.Error);
                    }
                }
                else
                {
                    ToastService.Show("Diagramma selezionato non trovato", ToastLevel.Error);
                }
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante il caricamento del diagramma: {ex.Message}", ToastLevel.Error);
            }
        }

        private void ExportDiagram()
        {
            exportModal.Show();
        }

        private async Task ConfirmExport()
        {
            try
            {
                await _jsModule.InvokeVoidAsync("umlEditor.exportDiagram", ExportFormat);
                ToastService.Show($"Diagramma esportato in formato {ExportFormat.ToUpper()}", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante l'esportazione del diagramma: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task ZoomIn()
        {
            try
            {
                await _jsModule.InvokeVoidAsync("umlEditor.zoomIn");
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante lo zoom in: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task ZoomOut()
        {
            try
            {
                await _jsModule.InvokeVoidAsync("umlEditor.zoomOut");
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante lo zoom out: {ex.Message}", ToastLevel.Error);
            }
        }

        private async Task ResetZoom()
        {
            try
            {
                await _jsModule.InvokeVoidAsync("umlEditor.resetZoom");
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante il reset dello zoom: {ex.Message}", ToastLevel.Error);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_jsModule != null)
                {
                    await _jsModule.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore durante il dispose del componente UML: {ex}");
            }
        }
    }
}