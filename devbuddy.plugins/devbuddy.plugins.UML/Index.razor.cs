using devbuddy.common.Applications;
using devbuddy.common.Services;
using devbuddy.plugins.UML.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace devbuddy.plugins.UML
{
    public partial class Index : AppComponentBase<UmlDesignerDataModel>
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }

        private ElementReference EditorElement;
        private UmlDiagram CurrentDiagram = new();
        private UmlDiagram DiagramToDelete;

        private bool IsEditorActive = false;
        private bool IsLoading = false;

        // Modali
        private ModalComponentBase OpenDiagramModal;
        private ModalComponentBase SaveDiagramModal;
        private ModalComponentBase DeleteDiagramModal;
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        protected override async Task OnInitializedAsync()
        {
            Model = DataModelService.ValueByKey<UmlDesignerDataModel>(nameof(UML));
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _ = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/devbuddy.plugins.UML/js/diagram-editor.js").AsTask();

                // Registra globalmente l'elemento editor per poterlo utilizzare con mxGraph
                await JSRuntime.InvokeVoidAsync("window.umlEditorElement", EditorElement);

                // Inizializza mxGraph se necessario
                await JSRuntime.InvokeVoidAsync("initMxGraph");
            }
        }

        private string GetDiagramTypeName(string type)
        {
            return type switch
            {
                "class" => "Diagramma delle Classi",
                "sequence" => "Diagramma di Sequenza",
                "activity" => "Diagramma di Attività",
                "usecase" => "Diagramma dei Casi d'Uso",
                "er" => "Entity Relationship",
                _ => type
            };
        }

        private async Task NewDiagram()
        {
            IsLoading = true;
            IsEditorActive = true;
            CurrentDiagram = new UmlDiagram();
            StateHasChanged();

            // Inizializza l'editor
            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("initUmlDiagram", CurrentDiagram.DiagramType);

            IsLoading = false;
            StateHasChanged();
        }

        private async Task DiagramTypeChanged()
        {
            if (IsEditorActive)
            {
                IsLoading = true;
                StateHasChanged();

                // Salva il contenuto corrente prima di cambiare tipo
                CurrentDiagram.XmlContent = await JSRuntime.InvokeAsync<string>("getUmlDiagramXml");

                // Cambia il tipo di diagramma
                await Task.Delay(100);
                await JSRuntime.InvokeVoidAsync("changeUmlDiagramType", CurrentDiagram.DiagramType);

                IsLoading = false;
                StateHasChanged();
            }
        }

        private async Task AddElement(string elementType)
        {
            if (IsEditorActive)
            {
                await JSRuntime.InvokeVoidAsync("addUmlElement", elementType);
            }
        }

        private async Task AddConnection(string connectionType)
        {
            if (IsEditorActive)
            {
                await JSRuntime.InvokeVoidAsync("addUmlConnection", connectionType);
            }
        }

        private void OpenModal()
        {
            OpenDiagramModal.Show();
        }

        private async Task OnDiagramTypeChange(ChangeEventArgs e)
        {
            if (e.Value is string newType)
            {
                CurrentDiagram.DiagramType = newType;
                await DiagramTypeChanged();
            }
        }

        private async Task LoadDiagram(UmlDiagram diagram)
        {
            IsLoading = true;
            IsEditorActive = true;
            CurrentDiagram = new UmlDiagram
            {
                Id = diagram.Id,
                Name = diagram.Name,
                Description = diagram.Description,
                DiagramType = diagram.DiagramType,
                XmlContent = diagram.XmlContent,
                CreatedDate = diagram.CreatedDate,
                ModifiedDate = diagram.ModifiedDate
            };
            OpenDiagramModal.Close();
            StateHasChanged();

            // Inizializza l'editor con il tipo di diagramma
            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("initUmlDiagram", CurrentDiagram.DiagramType);

            // Carica il contenuto XML del diagramma
            if (!string.IsNullOrEmpty(CurrentDiagram.XmlContent))
            {
                await JSRuntime.InvokeVoidAsync("loadUmlDiagramXml", CurrentDiagram.XmlContent);
            }

            IsLoading = false;
            StateHasChanged();
        }

        private void SaveDiagram()
        {
            SaveDiagramModal.Show();
        }

        private async Task SaveDiagramConfirm()
        {
            if (string.IsNullOrWhiteSpace(CurrentDiagram.Name))
            {
                CurrentDiagram.Name = "Untitled Diagram";
            }

            try
            {
                // Ottieni il contenuto XML del grafico
                CurrentDiagram.XmlContent = await JSRuntime.InvokeAsync<string>("getUmlDiagramXml");
                CurrentDiagram.ModifiedDate = DateTime.Now;

                // Se è un nuovo diagramma, aggiungi alla lista
                var existingDiagram = Model.SavedDiagrams.FirstOrDefault(d => d.Id == CurrentDiagram.Id);
                if (existingDiagram == null)
                {
                    Model.SavedDiagrams.Add(CurrentDiagram);
                }
                else
                {
                    // Aggiorna il diagramma esistente
                    var index = Model.SavedDiagrams.IndexOf(existingDiagram);
                    Model.SavedDiagrams[index] = CurrentDiagram;
                }

                await DataModelService.AddOrUpdateAsync(nameof(UML), Model);
                SaveDiagramModal.Close();
                ToastService.Show("Diagramma salvato con successo", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show($"Errore durante il salvataggio: {ex.Message}", ToastLevel.Error);
            }
        }

        private void DeleteDiagram(UmlDiagram diagram)
        {
            DiagramToDelete = diagram;
            DeleteDiagramModal.Show();
        }

        private async Task DeleteDiagramConfirm()
        {
            if (DiagramToDelete != null)
            {
                Model.SavedDiagrams.Remove(DiagramToDelete);
                await DataModelService.AddOrUpdateAsync(nameof(UML), Model);
                ToastService.Show("Diagramma eliminato", ToastLevel.Success);
                DiagramToDelete = null;
            }
        }

        private async Task ExportAsPng()
        {
            if (IsEditorActive)
            {
                await JSRuntime.InvokeVoidAsync("exportUmlDiagramAsPng", CurrentDiagram.Name);
                ToastService.Show("Diagramma esportato come immagine", ToastLevel.Success);
            }
        }

        private async Task ExportAsXml()
        {
            if (IsEditorActive)
            {
                var xml = await JSRuntime.InvokeAsync<string>("getUmlDiagramXml");
                await JSRuntime.InvokeVoidAsync("downloadTextFile", $"{CurrentDiagram.Name}.xml", "application/xml", xml);
                ToastService.Show("Diagramma esportato come XML", ToastLevel.Success);
            }
        }

        protected override async Task OnModelChangedAsync()
        {
            await DataModelService.AddOrUpdateAsync(nameof(UML), Model);
            await base.OnModelChangedAsync();
        }
    }
}