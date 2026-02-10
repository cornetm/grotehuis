using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadTile : MonoBehaviour
{
    [Header("Anchors")]
    public Transform entry;
    public Transform exit;

    [Header("Waypoints (auto-collected)")]
    public Transform waypointsRoot;
    public Transform[] waypoints;

    [ContextMenu("Auto Find References")]
    public void AutoFind()
    {
        // Entry/Exit by name (case-insensitive)
        entry = FindChildByName(transform, "Entry");
        exit = FindChildByName(transform, "Exit");

        waypointsRoot = FindChildByName(transform, "Waypoints");

        if (waypointsRoot != null)
        {
            // Pak alle children, sorteer op nummer in "WP_0, WP_1, ..."
            var list = waypointsRoot.GetComponentsInChildren<Transform>(true)
                .Where(t => t != waypointsRoot)
                .OrderBy(t => ExtractNumber(t.name))
                .ToList();

            waypoints = list.ToArray();
        }
        else
        {
            waypoints = Array.Empty<Transform>();
        }
    }

    void OnValidate()
    {
        if (entry == null || exit == null || waypointsRoot == null || waypoints == null || waypoints.Length == 0)
            AutoFind();
    }

    static Transform FindChildByName(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                return t;
        }
        return null;
    }

    static int ExtractNumber(string s)
    {
        // haalt laatste getal uit "WP_12" etc. Geen getal? -> grote waarde zodat die achteraan komt
        int num = 999999;
        string digits = new string(s.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrEmpty(digits) && int.TryParse(digits, out int parsed))
            num = parsed;
        return num;
    }
}
