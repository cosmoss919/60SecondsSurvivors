using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using _60SecondsSurvivors.Item;

namespace _60SecondsSurvivors.Editor
{
    public class ItemDataSyncEditor : EditorWindow
    {
        private Vector2 _scroll;
        private List<string> _log = new List<string>();
        private int _matched;
        private int _assigned;
        private int _unchanged;

        [MenuItem("Tools/60SecondsSurvivors/Sync Item Data")]
        public static void OpenWindow()
        {
            var w = GetWindow<ItemDataSyncEditor>("ItemData Sync");
            w.minSize = new Vector2(480, 300);
        }

        private void OnGUI()
        {
            GUILayout.Space(6);
            EditorGUILayout.HelpBox("프로젝트의 ItemData ScriptableObject와 ItemBase 프리팹을 자동으로 연결합니다.\n- prefab이 비어있으면 자동으로 할당\n- displayName이 비어있으면 prefab 이름으로 채움", MessageType.Info);
            GUILayout.Space(6);

            if (GUILayout.Button("Sync Now", GUILayout.Height(34)))
            {
                SyncItemDataWithPrefabs();
            }

            GUILayout.Space(8);

            EditorGUILayout.LabelField($"Matched: {_matched}  Assigned: {_assigned}  Unchanged: {_unchanged}");
            GUILayout.Space(6);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var line in _log)
                EditorGUILayout.LabelField(line);
            EditorGUILayout.EndScrollView();
        }

        private void SyncItemDataWithPrefabs()
        {
            _log.Clear();
            _matched = _assigned = _unchanged = 0;

            // 모든 ItemData 자산 로드
            var dataGuids = AssetDatabase.FindAssets("t:ItemData");
            var dataAssets = dataGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<ItemData>(path))
                .Where(d => d != null)
                .ToList();

            // 모든 프리팹 중 ItemBase 컴포넌트를 가진 프리팹만 수집 (ItemBase 컴포넌트 리스트)
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            var itemPrefabs = new List<ItemBase>();
            foreach (var g in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    var comp = go.GetComponent<ItemBase>();
                    if (comp != null)
                        itemPrefabs.Add(comp);
                }
            }

            // 이름 기준 인덱스(프리팹 이름 -> ItemBase)
            var prefabByName = itemPrefabs.ToDictionary(p => p.gameObject.name, p => p);

            // 매칭 로직:
            // 1) 이미 prefab이 지정되어 있으면 일단 matched 카운트 증가(변경 여부 체크)
            // 2) 지정되지 않은 경우 displayName 또는 asset filename으로 프리팹 이름과 매칭 시도
            // 3) displayName 비어있으면 prefab.name으로 채움
            Undo.RecordObjects(dataAssets.ToArray(), "Sync ItemData With Prefabs");

            foreach (var data in dataAssets)
            {
                bool changed = false;

                // 이미 연결된 prefab이 프로젝트에 존재하면 matched
                if (data.prefab != null)
                {
                    // prefab이 실제 프리팹 에셋인지 확인
                    var prefabExists = itemPrefabs.Any(p => p == data.prefab);
                    if (prefabExists)
                    {
                        _matched++;
                        _log.Add($"[Matched] {data.displayName ?? "(no displayName)"} -> {data.prefab.gameObject.name}");
                    }
                    else
                    {
                        // 연결된 prefab이 프로젝트에 없다면 초기화 후 재매칭 시도
                        data.prefab = null;
                        changed = true;
                    }
                }

                if (data.prefab == null)
                {
                    // 우선 displayName으로 매칭
                    if (!string.IsNullOrEmpty(data.displayName) && prefabByName.TryGetValue(data.displayName, out var found))
                    {
                        data.prefab = found;
                        _assigned++;
                        changed = true;
                        _log.Add($"[Assigned by displayName] {data.displayName} -> {found.gameObject.name}");
                    }
                    else
                    {
                        // asset 파일 이름(예: ItemData_SomeItem)에서 postfix 제거 또는 prefab 이름 포함 여부로 시도
                        var path = AssetDatabase.GetAssetPath(data);
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                        // 시도 1: fileName과 동일한 prefab 이름
                        if (prefabByName.TryGetValue(fileName, out found))
                        {
                            data.prefab = found;
                            _assigned++;
                            changed = true;
                            _log.Add($"[Assigned by fileName] {fileName} -> {found.gameObject.name}");
                        }
                        else
                        {
                            // 시도 2: fileName에 포함되는 프리팹 찾기 (부분 매칭)
                            var partial = itemPrefabs.FirstOrDefault(p => fileName.Contains(p.gameObject.name) || p.gameObject.name.Contains(fileName));
                            if (partial != null)
                            {
                                data.prefab = partial;
                                _assigned++;
                                changed = true;
                                _log.Add($"[Assigned by partial match] {fileName} -> {partial.gameObject.name}");
                            }
                            else
                            {
                                _log.Add($"[Unmatched] {fileName}");
                            }
                        }
                    }
                }

                // displayName이 비어있고 prefab이 있으면 prefab.name으로 채움
                if ((string.IsNullOrEmpty(data.displayName) || data.displayName.Trim().Length == 0) && data.prefab != null)
                {
                    data.displayName = data.prefab.gameObject.name;
                    changed = true;
                    _log.Add($"[DisplayName set] {data.displayName}");
                }

                if (changed)
                    EditorUtility.SetDirty(data);
                else
                    _unchanged++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _log.Insert(0, $"Sync finished. Data count: {dataAssets.Count}, Prefabs found: {itemPrefabs.Count}");
            _log.Insert(1, $"Assigned: {_assigned}, Matched(existing): {_matched}, Unchanged: {_unchanged}");
        }
    }
}