using System;
using UnityEngine;
using UnityEngine.UI;

namespace LocalizedDialogs.Samples.Canvas
{
    public class CanvasDialogAnswer : MonoBehaviour
    {
        public event Action<CanvasDialogAnswer> Clicked;
        public Text TextField;
        public Button Button;
        public GameObject InteractableLocker;
        public int Id;

        public void Show(in LocalizedDialogAnswer answer, int id)
        {
            Id = id;
            TextField.text = answer.Text.GetLocalizedString();
            if(InteractableLocker != default)
            {
                InteractableLocker.SetActive(!answer.Interactable);
            }
            Button.interactable = answer.Interactable;
        }

        private void OnEnable()
        {
            Button.onClick.AddListener(ClickListener);
        }

        private void OnDisable()
        {
            if(Button != default)
            {
                Button.onClick.RemoveListener(ClickListener);
            }
        }

        private void ClickListener()
        {
            Clicked?.Invoke(this);
        }

        public bool Interactable
        {
            get
            {
                return Button.interactable;
            }
            set
            {
                Button.interactable = value;
                if(InteractableLocker != default)
                {
                    InteractableLocker.SetActive(!value);
                }
            }
        }
    }
}
