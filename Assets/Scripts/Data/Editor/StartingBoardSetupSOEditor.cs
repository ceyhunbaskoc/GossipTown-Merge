using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Data.Editor
{
    [CustomEditor(typeof(StartingBoardSetupSO))]
    public class StartingBoardSetupSOEditor : UnityEditor.Editor
    {
        private StartingBoardSetupSO _targetSO;
        
        private Vector2Int _selectedBoardPos = new Vector2Int(-1, -1);
        private Vector2Int _selectedBackpackPos = new Vector2Int(-1, -1);
        private bool _showBoardEditor = true;
        private bool _showBackpackEditor = true;

        private void OnEnable()
        {
            _targetSO = (StartingBoardSetupSO)target;
            
            if (_targetSO.InitialCells == null) _targetSO.GetType().GetProperty("InitialCells")?.SetValue(_targetSO, new List<CellSetupData>());
            if (_targetSO.BackpackCells == null) _targetSO.GetType().GetProperty("BackpackCells")?.SetValue(_targetSO, new List<CellSetupData>());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<BoardWidth>k__BackingField"), new GUIContent("Board Width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<BoardHeight>k__BackingField"), new GUIContent("Board Height"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<BackpackWidth>k__BackingField"), new GUIContent("Backpack Width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<BackpackHeight>k__BackingField"), new GUIContent("Backpack Height"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            DrawSeparator();

            _showBoardEditor = EditorGUILayout.Foldout(_showBoardEditor, "Main Board Editor", true, EditorStyles.foldoutHeader);
            if (_showBoardEditor)
            {
                DrawGrid(_targetSO.BoardWidth, _targetSO.BoardHeight, _targetSO.InitialCells, ref _selectedBoardPos);
                DrawCellInspector(_targetSO.InitialCells, _selectedBoardPos, "Main Board Cell");
            }

            DrawSeparator();

            _showBackpackEditor = EditorGUILayout.Foldout(_showBackpackEditor, "Backpack Editor", true, EditorStyles.foldoutHeader);
            if (_showBackpackEditor)
            {
                DrawGrid(_targetSO.BackpackWidth, _targetSO.BackpackHeight, _targetSO.BackpackCells, ref _selectedBackpackPos);
                DrawCellInspector(_targetSO.BackpackCells, _selectedBackpackPos, "Backpack Cell");
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_targetSO);
            }
        }

        private void DrawGrid(int width, int height, List<CellSetupData> dataList, ref Vector2Int selectedPos)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                for (int x = 0; x < width; x++)
                {
                    Vector2Int currentPos = new Vector2Int(x, y);
                    CellSetupData cellData = GetCellData(dataList, currentPos);
                    
                    Color originalColor = GUI.backgroundColor;
                    if (currentPos == selectedPos) GUI.backgroundColor = Color.yellow;
                    else if (cellData.IsLocked) GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
                    else if (cellData.ItemDef != null) GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                    
                    string buttonText = cellData.ItemDef != null ? "I" : (cellData.IsLocked ? "L" : ".");
                    if (GUILayout.Button(buttonText, GUILayout.Width(35), GUILayout.Height(35)))
                    {
                        selectedPos = selectedPos == currentPos ? new Vector2Int(-1, -1) : currentPos;
                        GUI.FocusControl(null);
                    }
                    
                    GUI.backgroundColor = originalColor;
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCellInspector(List<CellSetupData> dataList, Vector2Int selectedPos, string title)
        {
            if (selectedPos.x == -1)
            {
                EditorGUILayout.HelpBox("Select a cell from the grid above to edit.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.window);
            EditorGUILayout.LabelField($"{title} Details: ({selectedPos.x}, {selectedPos.y})", EditorStyles.boldLabel);

            CellSetupData cellData = GetCellData(dataList, selectedPos);

            Undo.RecordObject(_targetSO, "Modified Cell Setup");

            cellData.IsLocked = EditorGUILayout.Toggle("Is Locked", cellData.IsLocked);

            cellData.ItemDef = (BaseItemDefinitionSO)EditorGUILayout.ObjectField("Item Definition", cellData.ItemDef, typeof(BaseItemDefinitionSO), false);
            
            if (cellData.ItemDef != null)
            {
                cellData.ItemLevel = EditorGUILayout.IntSlider("Item Level", cellData.ItemLevel, 1, 10);
            }
            else
            {
                cellData.ItemLevel = 0;
            }

            SetCellData(dataList, cellData);

            if (GUILayout.Button("Clear Cell Info"))
            {
                SetCellData(dataList, new CellSetupData { Position = selectedPos, IsLocked = false });
            }

            EditorGUILayout.EndVertical();
        }


        private CellSetupData GetCellData(List<CellSetupData> dataList, Vector2Int pos)
        {
            int index = dataList.FindIndex(c => c.Position == pos);
            if (index >= 0) return dataList[index];
            
            return new CellSetupData { Position = pos, IsLocked = false };
        }

        private void SetCellData(List<CellSetupData> dataList, CellSetupData cellData)
        {
            int index = dataList.FindIndex(c => c.Position == cellData.Position);
            
            if (!cellData.IsLocked && cellData.ItemDef == null)
            {
                if (index >= 0) dataList.RemoveAt(index);
                return;
            }

            if (index >= 0) dataList[index] = cellData;
            else dataList.Add(cellData);
        }

        private void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 2f);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
            EditorGUILayout.Space(5);
        }
    }
}