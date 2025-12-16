using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Photon Settings")]
 
    [SerializeField] private NetworkRunner runner; // Se encarga de la comunicación en red
    [SerializeField] private NetworkSceneManagerDefault sceneManager; // Maneja el cambio de escenas
    [SerializeField] private NetworkObject playerPrefab; // Prefab del jugador con NetworkObject
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

    [Header("UI")]
    [SerializeField] private Canvas mainCanvas; // Canvas que se apagará al entrar en la partida
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    [Header("Events")]
    [SerializeField] UnityEvent onPlayerJoinedToGame;// Los UnityEvent son llamadas que se hacen al invocar evento

    [Header("Score UI")]
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] GameObject rival;

    bool rivalSpawned = false;

    

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jugador {player.PlayerId} se ha unido a la partida.");

        if (runner.IsServer) //Unicamente la persona que tiene el host va a mandar a llamar ete metodo. Esto es para que no haya instaicias 
        {
            // Apagar el Canvas principal al iniciar la partida

            int randomSpawn = UnityEngine.Random.Range(0, spawnPoint.Length);
            NetworkObject networkPlayer = runner.Spawn(playerPrefab, spawnPoint[randomSpawn].position, spawnPoint[randomSpawn].rotation,player);
            players.Add(player, networkPlayer);
            NetworkObject scoreObject = runner.Spawn(scorePrefab, Vector3.zero, Quaternion.identity, player);
            NetworkScoreEntry scoreEntry = scoreObject.GetComponent<NetworkScoreEntry>();
            
            scoreEntry.SetOwner(player);
            scoreEntry.PlayerName = $"Player {player.PlayerId}";
            scoreEntry.Score = 0;

            networkPlayer.GetComponent<MovementController>().SetScoreEntry(scoreEntry);

        }

        onPlayerJoinedToGame.Invoke();//Invoca mi evento // Esto se pone afuera de el if para que todo jugador que entre 

        if (rivalSpawned)
        {
            return;
        }
        else
        {
            return;
            NetworkObject rivalObject = runner.Spawn(rival, new Vector3(), Quaternion.identity);
            rivalObject.GetComponent<Rival>().SetScoreEntry(runner.Spawn(scorePrefab, Vector3.zero, Quaternion.identity, PlayerRef.FromIndex(9)).GetComponent<NetworkScoreEntry>());
            rivalObject.GetComponent<Rival>().controllingPlayer.PlayerName = "Rival";
            rivalObject.GetComponent<Rival>().controllingPlayer.Score = 0;
            rivalObject.GetComponent<Rival>().controllingPlayer.OwnerPlayer = PlayerRef.FromIndex(9);
            rivalObject.GetComponent<Rival>().controllingPlayer.AddCustomPlayer(PlayerRef.FromIndex(9));
            rivalSpawned = true;
            Debug.Log("Rival spawned in the game.");
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    private async void StartGame(GameMode mode)
    {



        runner.AddCallbacks(this);
        runner.ProvideInput = true; // Habilitar el envio de inputs

        var scene = SceneRef.FromIndex(0);

        var scenInfo = new NetworkSceneInfo();

        if (scene.IsValid)
        {
            scenInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "0001",
            Scene = scene,
            CustomLobbyName = "Official EA Europe",
            SceneManager = sceneManager
        });
    }

    public void StartGameAsHost()
    {
        Debug.Log("Iniciando como HOST...");
        StartGame(GameMode.Host);
        
    }

    public void StartGameAsClient()
    {
        Debug.Log("Iniciando como CLIENTE...");
        StartGame(GameMode.Client);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player, NetworkObject playerObject)
    {
      if(runner.IsServer)
      {
       int randomSpawn = UnityEngine.Random.Range(0, spawnPoint.Length);
       NetworkObject networkPlayer = runner.Spawn(playerPrefab, spawnPoint[randomSpawn].position, spawnPoint[randomSpawn].rotation, player);
         players.Add(player, networkPlayer);
    
      }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player, NetworkObject playerObject)
    {
        if (players.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            players.Remove(player);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Creo un objeto de tipo NetworkInputData
        NetworkInputData data = new NetworkInputData()
        {
            move = InputManager.Instance.GetMoveInput() == null ? new Vector2(0,0) : InputManager.Instance.GetMoveInput(),
            look = InputManager.Instance.GetMouseDelta(),
            isRunning = InputManager.Instance.WasRunInputPressed(),
            yRotation = Camera.main.transform.eulerAngles.y,
            xRotation = (Camera.main.transform.localEulerAngles.x > 180) ? Camera.main.transform.localEulerAngles.x - 360 : Camera.main.transform.localEulerAngles.x,
            shoot = InputManager.Instance.ShootInputPresed()
        };
        
        input.Set(data);
    }

    #region Photon Callbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    #endregion
}