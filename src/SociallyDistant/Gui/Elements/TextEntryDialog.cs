using System;

namespace SociallyDistant.Gui.Elements
{
    public class TextEntryDialog
    {
        private ModalDialog _modal;

        public TextEntryDialog(ModalDialog dialog)
        {
            _modal = dialog;

            _modal.HasTextEntry = true;
            
            _modal.AddButton("OK", HandleTextSubmit);
            _modal.AddButton("Cancel", Cancel);

            _modal.TextSubmittedAction = HandleTextSubmit;
        }

        private void HandleTextSubmit()
        {
            var text = _modal.UserText;
            
            _modal.RemoveFromParent();
            
            TextEntered?.Invoke(text);
        }

        private void Cancel()
        {
            _modal.RemoveFromParent();

            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        public event Action<string> TextEntered;
        public event EventHandler Cancelled;
    }
}