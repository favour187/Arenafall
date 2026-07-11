using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArenaFall.Core
{
    /// <summary>
    /// Manages scene loading with async operations, progress tracking, and transitions.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingScreenPrefab;
        [SerializeField] private float _minLoadTime = 0.5f;

        private GameObject _loadingScreenInstance;
        private readonly Queue<string> _pendingScenes = new();
        private bool _isLoading;

        public static SceneLoader Instance { get; private set; }
        public float LoadProgress { get; private set; }
        public string CurrentScene { get; private set; }
        public bool IsLoading => _isLoading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentScene = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Load a scene by name with loading screen.
        /// </summary>
        public void LoadScene(string sceneName, Action onComplete = null)
        {
            if (_isLoading)
            {
                _pendingScenes.Enqueue(sceneName);
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            _isLoading = true;
            LoadProgress = 0f;

            // Show loading screen
            ShowLoadingScreen(true);

            // Start async load
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            float elapsed = 0f;
            while (!asyncOp.isDone)
            {
                elapsed += Time.deltaTime;
                LoadProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);

                // Ensure minimum load time
                if (asyncOp.progress >= 0.9f && elapsed >= _minLoadTime)
                {
                    asyncOp.allowSceneActivation = true;
                }

                yield return null;
            }

            LoadProgress = 1f;
            CurrentScene = sceneName;

            // Small delay for transition
            yield return new WaitForSeconds(0.2f);

            ShowLoadingScreen(false);
            _isLoading = false;

            // Fire event
            EventBus.Raise(new SceneLoadedEvent { SceneName = sceneName });

            onComplete?.Invoke();

            // Load next in queue
            if (_pendingScenes.Count > 0)
            {
                string next = _pendingScenes.Dequeue();
                StartCoroutine(LoadSceneAsync(next));
            }
        }

        /// <summary>
        /// Load a scene additively.
        /// </summary>
        public void LoadAdditive(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadAdditiveAsync(sceneName, onComplete));
        }

        private IEnumerator LoadAdditiveAsync(string sceneName, Action onComplete = null)
        {
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncOp.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// Unload an additive scene.
        /// </summary>
        public void UnloadAdditive(string sceneName, Action onComplete = null)
        {
            StartCoroutine(UnloadAdditiveAsync(sceneName, onComplete));
        }

        private IEnumerator UnloadAdditiveAsync(string sceneName, Action onComplete = null)
        {
            AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);
            while (!asyncOp.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        private void ShowLoadingScreen(bool show)
        {
            if (_loadingScreenPrefab == null) return;

            if (show && _loadingScreenInstance == null)
            {
                _loadingScreenInstance = Instantiate(_loadingScreenPrefab);
                DontDestroyOnLoad(_loadingScreenInstance);
            }
            else if (!show && _loadingScreenInstance != null)
            {
                Destroy(_loadingScreenInstance);
                _loadingScreenInstance = null;
            }
        }

        public string GetCurrentScene() => CurrentScene;
    }

    public class SceneLoadedEvent : GameEvent
    {
        public string SceneName { get; set; }
    }
}
