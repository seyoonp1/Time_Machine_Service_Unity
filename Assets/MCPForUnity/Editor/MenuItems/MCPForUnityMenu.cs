using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;
using MCPForUnity.Editor.Setup;
using MCPForUnity.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.MenuItems
{
    public static class MCPForUnityMenu
    {
        [MenuItem("Window/MCP For Unity/Check MCP connection status", priority = 0)]
        public static void CheckConnectionStatus()
        {
            var bridge = MCPServiceLocator.Bridge;
            bool isRunning = bridge.IsRunning;
            var mode = bridge.ActiveMode;
            int port = bridge.CurrentPort;
            string modeStr = mode?.ToString() ?? "Unknown";
            string status = isRunning
                ? $"Connected\nMode: {modeStr}\nPort: {port}"
                : "Not connected\nUnity Bridge is stopped.";
            EditorUtility.DisplayDialog("MCP for Unity – Connection", status, "OK");
        }

        [MenuItem("Window/MCP For Unity/Toggle MCP Window %#m", priority = 1)]
        public static void ToggleMCPWindow()
        {
            if (MCPForUnityEditorWindow.HasAnyOpenWindow())
            {
                MCPForUnityEditorWindow.CloseAllOpenWindows();
            }
            else
            {
                MCPForUnityEditorWindow.ShowWindow();
            }
        }

        [MenuItem("Window/MCP For Unity/Local Setup Window", priority = 2)]
        public static void ShowSetupWindow()
        {
            SetupWindowService.ShowSetupWindow();
        }


        [MenuItem("Window/MCP For Unity/Edit EditorPrefs", priority = 3)]
        public static void ShowEditorPrefsWindow()
        {
            EditorPrefsWindow.ShowWindow();
        }
    }
}
