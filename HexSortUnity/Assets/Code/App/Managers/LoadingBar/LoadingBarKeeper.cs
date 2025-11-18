using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.App.Managers.LoadingBar
{
    public class LoadingBarAdapter
    {
        public ILoadingBarKeeper LoadingBarKeeper { get; private set; }

        public void SetApplicationScreen(ILoadingBarKeeper loadingBarKeeper)
        {
            LoadingBarKeeper = loadingBarKeeper;
        }
    }

    public interface ILoadingBarKeeper
    {
        void ActiveLoadingBar(bool active);
        void SetStartProgressValue();
        UniTask RunIndependentLoadingBarAnimation();
        UniTask RunDependentLoadingBarAnimation(DependLoadingType dependLoadingType);
    }

    public class LoadingBarKeeper : MonoBehaviour, ILoadingBarKeeper
    {
        [SerializeField]
        private Image _fillingImage;

        [SerializeField]
        private float _defaultStartProgressAmount = 0.1f;

        [SerializeField]
        private float _independentLoadingBarProgressAmount = 0.65f;

        [SerializeField]
        private float _independentLoadingBarAnimationTime = 1f;

        [SerializeField]
        private float _analyticsEndProgressAmount = 0.85f;


        [Inject]
        private void Construct(LoadingBarAdapter loadingBarAdapter)
        {
            DontDestroyOnLoad(gameObject);
            ActiveLoadingBar(false);
            loadingBarAdapter.SetApplicationScreen(this);
        }

        public void ActiveLoadingBar(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetStartProgressValue()
        {
            _fillingImage.fillAmount = _defaultStartProgressAmount;
        }

        public async UniTask RunIndependentLoadingBarAnimation()
        {
            float currentTime = 0;
            float startAmount = _defaultStartProgressAmount;
            float tartgetAmount = _independentLoadingBarProgressAmount;

            while (currentTime < _independentLoadingBarAnimationTime)
            {
                currentTime += Time.deltaTime;
                float currentProgressAmount = Mathf.Lerp(startAmount, tartgetAmount, currentTime / _independentLoadingBarAnimationTime);
                _fillingImage.fillAmount = currentProgressAmount;

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private Tween _tween;
        public async UniTask RunDependentLoadingBarAnimation(DependLoadingType dependLoadingType)
        {
            switch (dependLoadingType)
            {
                case DependLoadingType.ANALYTICS_START:
                    await UniTask.DelayFrame(1);
                    _tween = DOTween.To(() => _fillingImage.fillAmount, x => _fillingImage.fillAmount = x, _analyticsEndProgressAmount, 5f).Play();
                    break;

                case DependLoadingType.ANALYTICS_END:
                    _tween.Kill();
                    _tween = DOTween.To(() => _fillingImage.fillAmount, x => _fillingImage.fillAmount = x, _analyticsEndProgressAmount, 0.25f);
                    await _tween.Play().AsyncWaitForCompletion();
                    break;

                case DependLoadingType.GAME_SCENE_LOAD_START:
                    ActiveLoadingBar(true);
                    _tween.Kill();
                    _tween = DOTween.To(() => _fillingImage.fillAmount, x => _fillingImage.fillAmount = x, 1f, 5f);
                    break;

                case DependLoadingType.GAME_SCENE_LOAD_END:
                    _tween.Kill();
                    _tween = DOTween.To(() => _fillingImage.fillAmount, x => _fillingImage.fillAmount = x, 1f, 0.5f);
                    await _tween.Play().AsyncWaitForCompletion();
                    await UniTask.Delay(250);
                    ActiveLoadingBar(false);
                    _fillingImage.fillAmount = _defaultStartProgressAmount;
                    await UniTask.Delay(250);
                    break;
            }
        }
    }

    public enum DependLoadingType
    {
        ANALYTICS_START,
        ANALYTICS_END,
        GAME_SCENE_LOAD_START,
        GAME_SCENE_LOAD_END
    }
}
