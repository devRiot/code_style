using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;


namespace Corgi.UI
{
    [CustomEditor(typeof(CharacterCombatModelController), true)]
	public class CharacterCombatModelControllerEditor : Editor
	{
        CharacterCombatModelController _charModelController = null;
        string _stopTimeText = null;

		void OnEnable()
		{
            if ((target as CharacterCombatModelController) == null) { return; }

            _charModelController = (target as CharacterCombatModelController);
            if (_charModelController != null)
            {
                _stopTimeText = _charModelController.StopTimeMinute.ToString();
            }
		}

		public override void OnInspectorGUI()
		{
            if (_charModelController != null)
			{				
				if (Application.isPlaying)
				{
                    string timeText = null;
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Play"))
					{
                        _charModelController.PlayCharacterModelOnEditor();
					}
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("PlayFromTime"))
                    {
                        float stopTime = 0f;
                        float.TryParse(timeText, out stopTime);
                        _charModelController.PlayCharacterModelOnEditor(stopTime);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("StopTime(sec) : ", GUILayout.Width(120f));
                    timeText = GUILayout.TextField(_stopTimeText);
                    if (_stopTimeText != timeText)
                    {
                        float stopTime = 0;
                        _stopTimeText = timeText;

                        if (float.TryParse(timeText, out stopTime) && _charModelController != null)
                        {
                            _charModelController.StopTimeMinute = stopTime;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Stop"))
					{
                        float stopTime = 0f;
                        float.TryParse(timeText, out stopTime);
                        _charModelController.StopCharacterModelOnEditor(stopTime);
					}
                    EditorGUILayout.EndHorizontal();
				}
			}

			DrawDefaultInspector();
		}
	}
}