﻿using System.Collections;
using System.Linq;
using CGS.Menus;
using UnityEngine;
using UnityEngine.Networking;

namespace CGS.Play.Multiplayer
{
    public class LobbyDiscovery : NetworkDiscovery
    {
        public const string BroadcastErrorMessage = "Unable to broadcast game session. Other players may not be able to join this game.";
        public const string ListenErrorMessage = "Error: Unable to listen for game sessions.";

        public LobbyMenu lobby;
        public bool HasReceivedBroadcast { get; set; }

        public void StartAsHost()
        {
            if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                Debug.LogError(BroadcastErrorMessage);
                return;
            }

            if (running)
                StopBroadcast();

            StartCoroutine(WaitToStartBroadcast());
        }

        // Wait a frame to get it to start broadcasting; there should be a better way to check if it's ok to start
        IEnumerator WaitToStartBroadcast()
        {
            yield return null;

            bool started = Initialize() && StartAsServer();
            if (!started)
                Debug.LogError(BroadcastErrorMessage);
        }

        public void SearchForHost()
        {
            if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                Debug.LogError(ListenErrorMessage);
                return;
            }

            if (running)
                StopBroadcast();
#if !UNITY_WSA
            Network.Disconnect();
#endif
            NetworkServer.Reset();

            StartCoroutine(WaitToStartListening());
        }

        // Wait a frame to get it to start broadcasting; there should be a better way to check if it's ok to start
        IEnumerator WaitToStartListening()
        {
            yield return null;

            bool started = Initialize() && StartAsClient();
            if (!started)
                Debug.LogError(ListenErrorMessage);
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            HasReceivedBroadcast = true;
            if (lobby == null || !lobby.gameObject.activeInHierarchy || NetworkManager.singleton.isNetworkActive)
                return;

            lobby.DisplayHosts(broadcastsReceived.Keys.ToList());
        }
    }
}
