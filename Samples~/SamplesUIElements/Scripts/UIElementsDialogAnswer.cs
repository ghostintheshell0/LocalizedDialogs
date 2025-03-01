using System;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Samples.UIElements
{
    public class UIElementsDialogAnswer : VisualElement
    {
        public event Action<UIElementsDialogAnswer> Clicked;
        public int Id;
        public string HoverClassName;
        public Label Message;

        public UIElementsDialogAnswer(VisualTreeAsset tree, string hoverClassName)
        {
            HoverClassName = hoverClassName;
            RegisterCallback<PointerDownEvent>(ClickListener);
            RegisterCallback<MouseEnterEvent>(PointerEnterListener);
            RegisterCallback<MouseLeaveEvent>(PointerExitListener);
            var clone = tree.CloneTree();
            Add(clone[0]);
        }

        private void PointerExitListener(MouseLeaveEvent evt)
        {
            RemoveFromClassList(HoverClassName);
        }

        private void PointerEnterListener(MouseEnterEvent evt)
        {
            if(Interactable)
            {
                AddToClassList(HoverClassName);
            }
        }

        private void ClickListener(PointerDownEvent evt)
        {
            if(enabledSelf)
            {
                Clicked?.Invoke(this);
            }
        }

        public bool Interactable
        {
            get
            {
                return enabledSelf;
            }
            set
            {
                this.pickingMode = value ? PickingMode.Position : PickingMode.Ignore;
                focusable = value;
                SetEnabled(value);
            }
        }
    }
}