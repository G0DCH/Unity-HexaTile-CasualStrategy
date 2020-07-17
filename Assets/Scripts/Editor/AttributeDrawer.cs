[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
{

    public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
    {
        return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
    {
        bool disabled = true;
        switch (((ReadOnlyAttribute)attribute).runtimeOnly)
        {
            case EReadOnlyType.FULLY_DISABLED:
                disabled = true;
                break;
            case EReadOnlyType.EDITABLE_RUNTIME:
                disabled = !UnityEngine.Application.isPlaying;
                break;
            case EReadOnlyType.EDITABLE_EDITOR:
                disabled = UnityEngine.Application.isPlaying;
                break;
        }

        using (var scope = new UnityEditor.EditorGUI.DisabledGroupScope(disabled))
        {
            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        }
    }

}

[UnityEditor.CustomPropertyDrawer(typeof(BeginReadOnlyAttribute))]
public class BeginReadOnlyGroupDrawer : UnityEditor.DecoratorDrawer
{

    public override float GetHeight()
    {
        return 0;
    }

    public override void OnGUI(UnityEngine.Rect position)
    {
        UnityEditor.EditorGUI.BeginDisabledGroup(true);
    }

}

[UnityEditor.CustomPropertyDrawer(typeof(EndReadOnlyAttribute))]
public class EndReadOnlyGroupDrawer : UnityEditor.DecoratorDrawer
{

    public override float GetHeight()
    {
        return 0;
    }

    public override void OnGUI(UnityEngine.Rect position)
    {
        UnityEditor.EditorGUI.EndDisabledGroup();
    }

}