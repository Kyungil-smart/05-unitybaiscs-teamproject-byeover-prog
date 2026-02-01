using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabIconBaker
{
    [MenuItem("Jobs/Generate Icons for Prefabs")]
    public static void GenerateIcons()
    {
        // 선택된 모든 객체에 대해 실행
        foreach (Object obj in Selection.objects)
        {
            // 프리팹인지 확인
            if (obj is GameObject prefab)
            {
                // 1. 유니티 내부 미리보기 가져오기 (해상도 128x128 등)
                Texture2D previewTexture = AssetPreview.GetAssetPreview(prefab);

                // 로딩이 덜 되었을 경우를 대비해 잠시 대기
                int timeout = 0;
                while (previewTexture == null && timeout < 100)
                {
                    System.Threading.Thread.Sleep(10);
                    previewTexture = AssetPreview.GetAssetPreview(prefab);
                    timeout++;
                }

                if (previewTexture != null)
                {
                    // 2. 텍스처를 PNG 바이트로 변환
                    byte[] bytes = previewTexture.EncodeToPNG();

                    // 3. 파일로 저장 (프리팹 이름_Icon.png)
                    string path = AssetDatabase.GetAssetPath(prefab);
                    string dir = Path.GetDirectoryName(path);
                    string fileName = Path.GetFileNameWithoutExtension(path) + ".png";
                    string finalPath = Path.Combine(dir, fileName);

                    File.WriteAllBytes(finalPath, bytes);
                    Debug.Log($"아이콘 저장 완료: {finalPath}");
                }
                else
                {
                    Debug.LogWarning($"미리보기를 가져올 수 없습니다: {prefab.name} (다시 시도해보세요)");
                }
            }
        }

        // 4. 에디터 새로고침 (파일 생성 반영)
        AssetDatabase.Refresh();
    }
}