using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GerenciadorGrade))]
public class GerenciadorGradeEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GerenciadorGrade gerenciador = (GerenciadorGrade)target;

        if (GUILayout.Button("Gerar Nova Grade")) {
            gerenciador.GerarNovaGrade();
        }
    }
}
