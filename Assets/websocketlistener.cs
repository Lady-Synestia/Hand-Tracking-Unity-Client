using System;
using System.Threading;
using System.Text;
using System.Net.WebSockets;
using UnityEngine;

public class websocketlistener : MonoBehaviour
{
    /// <summary>
    /// Semaphore code and docs
    /// solution >  https://stackoverflow.com/a/21163280
    /// SemaphoreSlim class > https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=net-8.0
    ///
    /// </summary>
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

    /// <summary>
    /// websockets docs
    /// websocket support > https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/websockets
    /// ClientWebSocket class > https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=net-9.0
    /// </summary>
    Uri uri = new Uri("ws://localhost:8765");
    ClientWebSocket ws = new ClientWebSocket();
    byte[] buffer = new byte[4096];

    string JsonString;

    // string testJson = "{\"point0\": {\"x\": 0.36451587080955505, \"y\": 0.7492399215698242, \"z\": -1.5179728585223984e-09}, \"point1\": {\"x\": 0.34811195731163025, \"y\": 0.7084940671920776, \"z\": -0.05889080837368965}, \"point2\": {\"x\": 0.3175491392612457, \"y\": 0.6842957139015198, \"z\": -0.08604178577661514}, \"point3\": {\"x\": 0.28366610407829285, \"y\": 0.6672555804252625, \"z\": -0.09910319745540619}, \"point4\": {\"x\": 0.24972884356975555, \"y\": 0.6543729305267334, \"z\": -0.10958848148584366}, \"point5\": {\"x\": 0.26708173751831055, \"y\": 0.7825995683670044, \"z\": -0.07823742181062698}, \"point6\": {\"x\": 0.20796909928321838, \"y\": 0.7588865160942078, \"z\": -0.09051723033189774}, \"point7\": {\"x\": 0.18948495388031006, \"y\": 0.7131353616714478, \"z\": -0.09657265245914459}, \"point8\": {\"x\": 0.18492534756660461, \"y\": 0.6728100180625916, \"z\": -0.09973837435245514}, \"point9\": {\"x\": 0.2606772780418396, \"y\": 0.8008275628089905, \"z\": -0.0475996658205986}, \"point10\": {\"x\": 0.20537029206752777, \"y\": 0.7662889361381531, \"z\": -0.05684434249997139}, \"point11\": {\"x\": 0.18718063831329346, \"y\": 0.7139981985092163, \"z\": -0.06536299735307693}, \"point12\": {\"x\": 0.18383654952049255, \"y\": 0.6727067828178406, \"z\": -0.07237333804368973}, \"point13\": {\"x\": 0.25679683685302734, \"y\": 0.8021751046180725, \"z\": -0.017644982784986496}, \"point14\": {\"x\": 0.20753738284111023, \"y\": 0.772013247013092, \"z\": -0.021955737844109535}, \"point15\": {\"x\": 0.19017675518989563, \"y\": 0.7260228991508484, \"z\": -0.03151095658540726}, \"point16\": {\"x\": 0.1863010674715042, \"y\": 0.686863899230957, \"z\": -0.03952827677130699}, \"point17\": {\"x\": 0.2558435797691345, \"y\": 0.7944516539573669, \"z\": 0.00818195752799511}, \"point18\": {\"x\": 0.2147524505853653, \"y\": 0.7713050246238708, \"z\": 0.003776286030188203}, \"point19\": {\"x\": 0.19767005741596222, \"y\": 0.7385501265525818, \"z\": -0.003902278607711196}, \"point20\": {\"x\": 0.1897059679031372, \"y\": 0.7055431008338928, \"z\": -0.009910108521580696}}";

    public HandInterface HandInterface;

    async void Start()
    {
        await ws.ConnectAsync(uri, default);

    }
    void FixedUpdate()
    {
        ReceiveData();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    async void ReceiveData()
    {
        // semaphore timeout is set to 0
        // prevents receive requests from backing up
        if (await semaphore.WaitAsync(0))
        {
            try
            {
                var result = await ws.ReceiveAsync(buffer, default);

                JsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandInterface.UpdateHandPositions(JsonString);
            }
            finally
            {
                semaphore.Release();
            }
        }

    }

    async void CloseSocket()
    {
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
    }
}