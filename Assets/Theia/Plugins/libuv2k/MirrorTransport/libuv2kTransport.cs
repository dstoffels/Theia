//#if MIRROR <- commented out because MIRROR isn't defined on first import yet
// uses libuv2k for both client & server
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using libuv2k;
using libuv2k.Native;

namespace Mirror
{
    public class libuv2kTransport : Transport
    {
        // scheme used by this transport
        // "tcp4" means tcp with 4 bytes header, network byte order
        public const string Scheme = "tcp4";

        public ushort Port = 7777;

        [Tooltip("Nagle Algorithm can be disabled by enabling NoDelay")]
        public bool NoDelay = true;

        // libuv can be ticked multiple times per frame up to max so we don't
        // deadlock
        public const int LibuvMaxTicksPerFrame = 100;

        // Libuv state
        //
        // IMPORTANT: do NOT create new Loop & Client here, otherwise a loop is
        //            also allocated if we run a test while a scene with this
        //            component on a GameObject is openened.
        //
        //            we need to create it when needed and dispose when we are
        //            done, otherwise dispose isn't called until domain reload.
        //
        // TODO what if we use one loop for both?
        Loop clientLoop;
        Loop serverLoop;
        TcpStream client;
        TcpStream server;

        // server state
        Dictionary<int, TcpStream> connections = new Dictionary<int, TcpStream>();
        // connectionId = 0 is reserved for localhost. need to start at 1.
        int nextConnectionId = 1;

        void Awake()
        {
            // initialize
            libuv2k.libuv2k.Initialize();

            // configure
            Log.Info = Debug.Log;
            Log.Warning = Debug.LogWarning;
            Log.Error = Debug.LogError;

            // IMPORTANT: create loops only once
            // DO NOT dispose in OnDisconnect, since that's called from within
            // a loop's uv_run. disposing while it's running could crash libuv.
            clientLoop = new Loop();
            serverLoop = new Loop();

            Debug.Log("libuv2k Transport initialized!");
        }

        public override bool Available()
        {
            // and only compiled for desktop platforms at the moment.
            return Application.platform == RuntimePlatform.OSXEditor ||
                   Application.platform == RuntimePlatform.OSXPlayer ||
                   Application.platform == RuntimePlatform.WindowsEditor ||
                   Application.platform == RuntimePlatform.WindowsPlayer ||
                   Application.platform == RuntimePlatform.LinuxEditor ||
                   Application.platform == RuntimePlatform.LinuxPlayer;
        }

        // client //////////////////////////////////////////////////////////////
        public override bool ClientConnected() => client != null && client.IsActive;
        void ClientConnect(string hostname, ushort port)
        {
            if (client != null)
                return;

            // libuv doesn't resolve host name, and it needs ipv4.
            if (LibuvUtils.ResolveToIPV4(hostname, out IPAddress address))
            {
                // connect client
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);
                IPEndPoint remoteEndPoint = new IPEndPoint(address, port);

                Debug.Log("Libuv connecting to: " + address + ":" + port);
                client = new TcpStream(clientLoop);
                client.NoDelay(NoDelay);
                client.onClientConnect = OnLibuvClientConnected;
                client.onMessage = OnLibuvClientMessage;
                client.onError = OnLibuvClientError;
                client.onClosed = OnLibuvClientClosed;
                client.ConnectTo(localEndPoint, remoteEndPoint);
            }
            else Debug.LogWarning("Libuv Connect: no IPv4 found for hostname: " + hostname);
        }
        public override void ClientConnect(string address) => ClientConnect(address, Port);
        public override void ClientConnect(Uri uri)
        {
            if (uri.Scheme != Scheme)
                throw new ArgumentException($"Invalid url {uri}, use {Scheme}://host:port instead", nameof(uri));

            ushort serverPort = uri.IsDefaultPort ? Port : (ushort)uri.Port;
            ClientConnect(uri.Host, serverPort);
        }
        public override void ClientSend(int channelId, ArraySegment<byte> segment)
        {
            client?.Send(segment);
        }
        public override void ClientDisconnect()
        {
            client?.Dispose();
            client = null;
        }
        // messages should always be processed in early update
        public override void ClientEarlyUpdate()
        {
            // note: we need to check enabled in case we set it to false
            // when LateUpdate already started.
            // (https://github.com/vis2k/Mirror/pull/379)
            if (enabled)
            {
                // tick libuv client while loop is valid
                // IMPORTANT: tick even if client is null.
                //            when disposing the client, uv_close callbacks are
                //            only called in the next loop update, so we need to
                //            keep updating the loop even if client was disposed.
                if (clientLoop != null)
                {
                    // Run with UV_RUN_NOWAIT returns 0 when nothing to do, but we
                    // should avoid deadlocks via LibuvMaxTicksPerFrame
                    for (int i = 0; i < LibuvMaxTicksPerFrame; ++i)
                    {
                        if (clientLoop.Run(uv_run_mode.UV_RUN_NOWAIT) == 0)
                        {
                            //Debug.Log("libuv cl ticked only " + i + " times");
                            break;
                        }
                    }
                }
            }
        }


        // client callbacks ////////////////////////////////////////////////////
        void OnLibuvClientConnected(TcpStream handle, Exception exception)
        {
            // close if errors (AFTER setting up onClosed callback!)
            if (exception != null)
            {
                Debug.Log($"libuv cl: client error {exception}");
                handle.Dispose();
                return;
            }

            // Mirror event
            OnClientConnected.Invoke();

            Debug.Log($"libuv cl: client connected.");
        }

        // segment is valid until function returns.
        void OnLibuvClientMessage(TcpStream handle, ArraySegment<byte> segment)
        {
            //Debug.Log("libuv cl: data=" + BitConverter.ToString(segment.Array, segment.Offset, segment.Count));

            // Mirror event
            OnClientDataReceived.Invoke(segment, Channels.Reliable);
        }

        void OnLibuvClientError(TcpStream handle, Exception error)
        {
            // Mirror event
            Debug.Log($"libuv cl: read error {error}");
            OnClientDisconnected.Invoke();
        }

        // OnClosed is called after we closed the stream
        void OnLibuvClientClosed(TcpStream handle)
        {
            // important: clear the connection BEFORE calling the Mirror event
            // otherwise Mirror OnDisconnected might try to send to a closed
            // connection which we didn't clear yet. do it first.
            client = null;

            // Mirror event
            Debug.Log("libuv cl: closed connection");
            OnClientDisconnected.Invoke();
        }

        // server //////////////////////////////////////////////////////////////
        public override Uri ServerUri()
        {
            UriBuilder builder = new UriBuilder();
            builder.Scheme = Scheme;
            builder.Host = Dns.GetHostName();
            builder.Port = Port;
            return builder.Uri;
        }
        public override bool ServerActive() => server != null;
        public override void ServerStart()
        {
            if (server != null)
                return;

            // start server
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, Port);

            Debug.Log($"libuv sv: starting TCP..." + EndPoint);
            server = new TcpStream(serverLoop);
            server.SimultaneousAccepts(true);
            server.onServerConnect = OnLibuvServerConnected;
            server.Listen(EndPoint);
            Debug.Log($"libuv sv: TCP started!");
        }
        public override void ServerSend(int connectionId, int channelId, ArraySegment<byte> segment)
        {
            if (server != null && connections.TryGetValue(connectionId, out TcpStream connection))
            {
                //Debug.Log("libuv sv sending " + segment.Count + " bytes: " + BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                connection.Send(segment);
            }
        }
        public override bool ServerDisconnect(int connectionId)
        {
            if (server != null && connections.TryGetValue(connectionId, out TcpStream connection))
            {
                // Dispose will disconnect, and OnLibuvClosed will clean up
                connection.Dispose();
                return true;
            }
            return false;
        }
        public override string ServerGetClientAddress(int connectionId)
        {
            if (server != null && connections.TryGetValue(connectionId, out TcpStream connection))
            {
                return connection.GetPeerEndPoint().Address.ToString();
            }
            return "";
        }
        public override void ServerStop()
        {
            if (server != null)
            {
                server.Dispose();
                server = null;
                connections.Clear();
                Debug.Log("libuv sv: TCP stopped!");
            }
        }
        // messages should always be processed in early update
        public override void ServerEarlyUpdate()
        {
            // note: we need to check enabled in case we set it to false
            // when LateUpdate already started.
            // (https://github.com/vis2k/Mirror/pull/379)
            if (enabled)
            {
                // tick libuv server while loop is valid
                // IMPORTANT: tick even if server is null.
                //            when disposing the client, uv_close callbacks are
                //            only called in the next loop update, so we need to
                //            keep updating the loop even if client was disposed.
                if (serverLoop != null)
                {
                    // Run with UV_RUN_NOWAIT returns 0 when nothing to do, but we
                    // should avoid deadlocks via LibuvMaxTicksPerFrame
                    for (int i = 0; i < LibuvMaxTicksPerFrame; ++i)
                    {
                        if (serverLoop.Run(uv_run_mode.UV_RUN_NOWAIT) == 0)
                        {
                            //Debug.Log("libuv sv ticked only " + i + " times");
                            break;
                        }
                    }
                }
            }
        }

        // server callbacks ////////////////////////////////////////////////////
        void OnLibuvServerConnected(TcpStream handle, Exception error)
        {
            // setup callbacks for the new connection
            handle.onMessage = OnLibuvServerMessage;
            handle.onError = OnLibuvServerError;
            handle.onClosed = OnLibuvServerClosed;

            // close if errors (AFTER setting up onClosed callback!)
            if (error != null)
            {
                Debug.Log($"libuv sv: client connection failed {error}");
                handle.Dispose();
                return;
            }

            // assign a connectionId via UserToken.
            // this is better than using handle.InternalHandle.ToInt32() because
            // the InternalHandle isn't available in OnLibuvClosed anymore.
            handle.UserToken = nextConnectionId++;
            connections[(int)handle.UserToken] = handle;

            Debug.Log("libuv sv: client connected with connectionId=" + (int)handle.UserToken);

            // dotsnet event
            OnServerConnected.Invoke((int)handle.UserToken);
        }

        void OnLibuvServerMessage(TcpStream handle, ArraySegment<byte> segment)
        {
            // valid connection?
            if (connections.ContainsKey((int)handle.UserToken))
            {
                // DOTSNET event
                OnServerDataReceived.Invoke((int)handle.UserToken, segment, Channels.Reliable);
            }
            else Debug.LogError("libuv sv: invalid connectionid: " + (int)handle.UserToken);
        }

        void OnLibuvServerError(TcpStream handle, Exception error)
        {
            Debug.Log($"libuv sv: error {error}");
            connections.Remove((int)handle.UserToken);

            // TODO invoke OnDisconnected or does OnLibuvClosed get called anyway?
        }

        void OnLibuvServerClosed(TcpStream handle)
        {
            Debug.Log($"libuv sv: closed client {handle}");

            // important: remove the connection BEFORE calling the Mirror event
            // otherwise Mirror OnDisconnected Unspawn might try to send to a
            // close connection which we didn't remove yet. do it first.
            int connectionId = (int)handle.UserToken;
            connections.Remove(connectionId);

            // Mirror event
            OnServerDisconnected.Invoke(connectionId);
        }

        // common //////////////////////////////////////////////////////////////
        public override void Shutdown()
        {
            Debug.Log("libuv2k Transport Shutdown()");
            // OnDestroy is enough. we should not call libuv2k.Shutdown() twice.
            //OnDestroy();
        }

        public override int GetMaxPacketSize(int channelId) => TcpStream.MaxMessageSize;

        void OnDestroy()
        {
            // clean up
            // IMPORTANT: dispose loops only once
            // DO NOT dispose in OnDisconnect, since that's called from within
            // a loop's uv_run. disposing while it's running could crash libuv.
            clientLoop.Dispose();
            serverLoop.Dispose();
            libuv2k.libuv2k.Shutdown();
        }

        public override string ToString()
        {
            if (ServerActive())
            {
                return "libuv2k Server port: " + Port;
            }
            else if (ClientConnected())
            {
                return "libuv2k Client port: " + Port;
            }
            return "libuv2k (inactive/disconnected)";
        }
    }
}
//#endif <- commented out because MIRROR isn't defined on first import yet