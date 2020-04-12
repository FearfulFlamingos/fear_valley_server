using UnityEngine;

namespace Scripts.Networking
{
    /// <summary>
    /// Monobehaviour wrapper for the Server class.
    /// </summary>
    /// <remarks>Nothing really interacts with the Monoserver, but it allows us to test the server with EditorTests.</remarks>
    public class MonoServer : MonoBehaviour
    {
        /// <summary>The real server that handles all of the code.</summary>
        public static Server Instance { set; get; }

        private void Start()
        {
            if (Instance == null)
                Instance = new Server();
            DontDestroyOnLoad(gameObject);
            Instance.Init();
        }

        private void Update()
        {
            Instance.UpdateMessagePump();
        }
    }
}