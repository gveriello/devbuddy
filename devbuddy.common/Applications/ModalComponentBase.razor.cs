using Microsoft.AspNetCore.Components;

namespace devbuddy.common.Applications
{
    public sealed partial class ModalComponentBase
    {
        [Parameter] public string Title { get; set; }
        [Parameter] public string ModalSize { get; set; } = "modal-md";
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public RenderFragment FooterContent { get; set; }

        [Parameter] public bool ShowHeader { get; set; } = true;
        [Parameter] public bool ShowFooter { get; set; } = true;
        [Parameter] public bool ShowCancelButton { get; set; } = true;

        [Parameter] public string CancelButtonText { get; set; } = "Annulla";
        [Parameter] public string ConfirmButtonText { get; set; } = "OK";

        [Parameter] public EventCallback OnConfirmCallback { get; set; }
        [Parameter] public EventCallback OnCloseCallback { get; set; }


        private ElementReference _refModalElement;
        private string modalDisplay = "none";
        private string modalClass = string.Empty;
        private bool showBackdrop = false;

        public void Show()
        {
            modalDisplay = "block";
            modalClass = "show";
            showBackdrop = true;
        }

        public void Close(bool skipCallback = false)
        {
            modalDisplay = "none";
            modalClass = string.Empty;
            showBackdrop = false;
            if (OnCloseCallback.HasDelegate && !skipCallback)
            {
                OnCloseCallback.InvokeAsync();
            }
        }

        private void OnConfirm()
        {
            if (OnConfirmCallback.HasDelegate)
            {
                OnConfirmCallback.InvokeAsync();
            }
            Close(true);
        }
    }
}
