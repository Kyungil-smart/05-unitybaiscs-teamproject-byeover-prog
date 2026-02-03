using UnityEngine;

namespace DigitalRuby.RainMaker
{
    // 씬에서 바로 실행 가능, 에디터에서도 업데이트됨
    [ExecuteAlways]
    // 인스펙터 메뉴에서 "RainMaker/Rain Controller Extension"으로 추가 가능
    [AddComponentMenu("RainMaker/Rain Controller Extension")]
    public class RainControllerExtension : MonoBehaviour
    {
        [Tooltip("Target rain script (RainScript or RainScript2D)")]
        public BaseRainScript RainScript; // 제어할 RainScript 또는 RainScript2D

        [Header("Sound Settings")]
        [Range(0f, 1f)]
        public float RainVolume = 1.0f; // 비 소리 볼륨
        [Range(0f, 1f)]
        public float WindVolume = 1.0f; // 바람 소리 볼륨

        [Header("Rain Appearance")]
        [Range(0f, 1f)]
        public float RainIntensity = 0.5f; // 비 강도

        [Tooltip("Adjust X (width) and Z (depth) scale of rain and wind")]
        public Vector2 RainWidthDepthMultiplier = new Vector2(1.0f, 1.0f);
        // 비/바람 범위 조절. x = 폭, z = 깊이

        [Tooltip("Adjust height above camera for rain start")]
        public float RainHeightOffset = 0f;
        // 카메라 위에서 비가 시작될 높이 오프셋

        // 매 프레임마다 실행
        private void Update()
        {
            // RainScript가 없으면 아무 것도 안함
            if (RainScript == null) return;

            // 1️⃣ 비 강도 설정
            RainScript.RainIntensity = RainIntensity;

            // 2️⃣ Rain ParticleSystem 크기 및 위치 조정
            if (RainScript.RainFallParticleSystem != null)
            {
                // 현재 로컬 스케일 가져오기
                var scale = RainScript.RainFallParticleSystem.transform.localScale;

                // x, z 스케일 적용
                scale.x = RainWidthDepthMultiplier.x * GetCameraWidthMultiplier(); // 카메라 너비에 맞춤
                scale.z = RainWidthDepthMultiplier.y;
                RainScript.RainFallParticleSystem.transform.localScale = scale;

                // 카메라 높이에 맞춰 비 위치 설정
                if (RainScript.Camera != null)
                {
                    Vector3 pos = RainScript.RainFallParticleSystem.transform.position;
                    pos.y = RainScript.Camera.transform.position.y + RainHeightOffset;
                    RainScript.RainFallParticleSystem.transform.position = pos;
                }
            }

            // 3️⃣ WindZone 범위 확대
            if (RainScript.WindZone != null)
            {
                Vector3 windScale = RainScript.WindZone.transform.localScale;
                windScale.x = RainWidthDepthMultiplier.x * GetCameraWidthMultiplier(); // 카메라 너비에 맞춤
                windScale.z = RainWidthDepthMultiplier.y;
                RainScript.WindZone.transform.localScale = windScale;
            }
        }

        // 카메라 가로 크기에 맞춰 비/바람 범위 확대 계산
        private float GetCameraWidthMultiplier()
        {
            // 카메라가 없으면 1배
            if (RainScript.Camera == null) return 1f;

            if (RainScript.Camera.orthographic)
                // Orthographic 카메라: orthographicSize * 2 * 화면비율 = 화면 너비
                return RainScript.Camera.orthographicSize * 2f * RainScript.Camera.aspect;
            else
            {
                // Perspective 카메라: 삼각함수로 화면 너비 대략 계산
                float distance = Mathf.Abs(RainScript.RainFallParticleSystem.transform.position.y - RainScript.Camera.transform.position.y);
                return 2f * distance * Mathf.Tan(Mathf.Deg2Rad * RainScript.Camera.fieldOfView * 0.5f) * RainScript.Camera.aspect;
            }
        }
    }
}
