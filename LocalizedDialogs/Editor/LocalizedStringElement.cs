using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Editor
{
    public class LocalizedStringElement : VisualElement
    {
        public event System.Action<LocalizedStringElement> Changed;
        private IMGUIContainer _imgui;
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedProperty;
        public LocalizedString LocalizedString {get; private set;}

        private LocalizedStringElement(LocalizedString localizedString) : base()
        {
            LocalizedString = localizedString;
            _imgui = new IMGUIContainer(OnGuiHandler);
            Add(_imgui);
        }

        private void ObjectChangeListener(SerializedObject @object)
        {
            Changed?.Invoke(this);
        }

        private void OnGuiHandler()
        {
            if(_serializedObject != default)
            {
                _serializedObject.Update();
                EditorGUILayout.PropertyField(_serializedProperty);
            }
            else
            {
                Remove(_imgui);
            }
            
        }

        public static LocalizedStringElement Create(SerializedObject serializedTarget, SerializedProperty property)
        {
            var localizedString = property.boxedValue as LocalizedString;
            var el = new LocalizedStringElement(localizedString)
            {
                _serializedObject = serializedTarget,
                _serializedProperty = property,
            };
            el._imgui.TrackSerializedObjectValue(serializedTarget, el.ObjectChangeListener);

            return el;
        }

        internal void OnDestroy()
        {
            _serializedObject = default;
            Remove(_imgui);
        }
    }
    
    public class LocalizedElement<T> : VisualElement where T : LocalizedReference
    {
        public event System.Action<LocalizedElement<T>> Changed;
        private IMGUIContainer _imgui;
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedProperty;
        public LocalizedReference LocalizedReference {get; private set;}

        private LocalizedElement(T reference) : base()
        {
            LocalizedReference = reference;
            _imgui = new IMGUIContainer(OnGuiHandler);
            Add(_imgui);
        }

        private void ObjectChangeListener(SerializedObject @object)
        {
            Changed?.Invoke(this);
        }

        private void OnGuiHandler()
        {
            if(_serializedObject != default)
            {
                _serializedObject.Update();
                EditorGUILayout.PropertyField(_serializedProperty);
            }
            else
            {
                Remove(_imgui);
            }
        }

        public static LocalizedElement<T> Create(SerializedObject serializedTarget, SerializedProperty property)
        {
            var reference = property.boxedValue as T;
            var el = new LocalizedElement<T>(reference)
            {
                _serializedObject = serializedTarget,
                _serializedProperty = property,
            };
            el._imgui.TrackSerializedObjectValue(serializedTarget, el.ObjectChangeListener);

            return el;
        }
    }
}