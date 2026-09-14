using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RomainUTR.SLToolbox;

namespace RomainUTR.SLToolbox.Editor
{
    [InitializeOnLoad]
    public static class SceneSwitcherTrigger
    {
        static SceneSwitcherTrigger()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(EntityId entityId, Rect selectionRect)
        {
            GameObject go = EditorUtility.EntityIdToObject(entityId) as GameObject;

            if (go == null)
            {
                Event currentEvent = Event.current;

                Rect sceneNameRect = selectionRect;
                sceneNameRect.xMin += 20;
                sceneNameRect.width = 200;

                if (sceneNameRect.Contains(currentEvent.mousePosition))
                {
                    EditorGUIUtility.AddCursorRect(sceneNameRect, MouseCursor.Link);

                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        PopupWindow.Show(sceneNameRect, new SceneSwitcherContent());
                    }
                }
            }
        }

        [MenuItem("SL Toolbox/Scene Switcher %#S", priority = 0)]
        public static void ShowSceneBrowserPopUp()
        {
            Rect rect = new Rect(200, 200, 1, 1);
            PopupWindow.Show(rect, new SceneSwitcherContent());
        }
    }
}