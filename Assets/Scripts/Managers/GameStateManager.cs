using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Events;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages game state machine and transitions between states.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        private GameState _currentState = GameState.Boot;
        private GameState _previousState = GameState.Boot;

        public GameState CurrentState => _currentState;
        public GameState PreviousState => _previousState;

        private void Awake()
        {
            ServiceLocator.Register<GameStateManager>(this);
        }

        private void Start()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        /// <summary>
        /// Transition to a new state.
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            if (_currentState == newState) return;

            _previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameStateManager] {_previousState} -> {_currentState}");

            EventBus.Raise(new GameStateChangedEvent
            {
                PreviousState = _previousState,
                NewState = _currentState
            });
        }

        /// <summary>
        /// Go back to the previous state.
        /// </summary>
        public void GoBack()
        {
            if (_previousState != GameState.Boot)
            {
                TransitionTo(_previousState);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            // Handle global state change logic
        }

        /// <summary>
        /// Check if the game is in a specific state.
        /// </summary>
        public bool IsInState(GameState state) => _currentState == state;

        /// <summary>
        /// Check if the game is in any of the specified states.
        /// </summary>
        public bool IsInAnyState(params GameState[] states)
        {
            foreach (var state in states)
            {
                if (_currentState == state) return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
    }
}
