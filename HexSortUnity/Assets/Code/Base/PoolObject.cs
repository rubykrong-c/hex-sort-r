using UnityEngine;

namespace Code
{
    public class PoolObject : MonoBehaviour
    {
        [HideInInspector] public Vector3 initScale;
        private void Start()
        {
            initScale = transform.localScale;
        }

        private void OnDestroy()
        {
            // if (Managers.Instance == null)
            // 	return;
            // LevelManager.Instance.OnLevelFinish.RemoveListener(() => transform.SetParent(PoolingSystem.Instance.transform));
            // SceneController.Instance.OnSceneStartedLoading.RemoveListener(() => PoolingSystem.Instance.DestroyAPS(gameObject));
        }
    }
}