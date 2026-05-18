// RedPhoneDXConfig/Web/WebServer.cs
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using UCNLDrivers;

namespace RedPhoneDXConfig.Web
{
    public class WebServer : IDisposable
    {
        private HttpListener _listener;
        private readonly int _port;
        private bool _isRunning;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<WebSocketConnection> _webSockets = new();
        private readonly object _wsLock = new();

        private readonly RPHPort _rphPort;
        private readonly Action<string> _onLogAlways;

        // Храним последние данные SETS2
        private SETS2ReceivedEventArgs? _lastSets2Data;
        private readonly object _dataLock = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WebServer(int port, RPHPort rphPort, Action<string> onLogAlways)
        {
            _port = port;
            _rphPort = rphPort ?? throw new ArgumentNullException(nameof(rphPort));
            _onLogAlways = onLogAlways ?? throw new ArgumentNullException(nameof(onLogAlways));

            SubscribeToDeviceEvents();
            SubscribeToDeviceLogs();
        }

        private void SubscribeToDeviceEvents()
        {
            _rphPort.DeviceInfoValidChanged += (s, e) =>
            {
                Broadcast($@"
                {{
                    ""type"": ""device_status"",
                    ""data"": {{
                        ""isValid"": {_rphPort.IsDeviceInfoValid.ToString().ToLower()},
                        ""deviceType"": {(int)_rphPort.DeviceType},
                        ""serialNumber"": ""{_rphPort.SerialNumber}"",
                        ""systemVersion"": ""{_rphPort.SystemVersion}""
                    }}
                }}");                
            };

            _rphPort.SETS2Received += (s, e) =>
            {
                lock (_dataLock)
                {
                    _lastSets2Data = e;
                }

                LogInfo($"SETS2: Channel={e.SSBChannelID}, Vol={e.HeadphoneOutVolume}%, VAD={e.VADSensitivity}%, Bat={e.LowBatteryThresholdV}V, RWLT={e.IsRWLT}, Flags=0x{e.Flags1:X2}");

                Broadcast($@"
                {{
                    ""type"": ""settings"",
                    ""data"": {{
                        ""flashWrite"": {e.FlashWrite.ToString().ToLower()},
                        ""ssbChannelId"": {(int)e.SSBChannelID},
                        ""headphoneOutVolume"": {e.HeadphoneOutVolume},
                        ""vadSensitivity"": {e.VADSensitivity},
                        ""lowBatteryThresholdV"": {e.LowBatteryThresholdV.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                        ""isRWLT"": {e.IsRWLT.ToString().ToLower()},
                        ""rwltDiverId"": {e.RWLT_DiverID},
                        ""flags1"": {e.Flags1},
                        ""reserved1"": {e.Reserved1},
                        ""reserved2"": {e.Reserved2}
                    }}
                }}");
            };

            _rphPort.IsWaitingLocalChanged += (s, e) =>
            {
                Broadcast($@"
                {{
                    ""type"": ""busy_status"",
                    ""data"": {{
                        ""isWaiting"": {_rphPort.IsWaitingLocal.ToString().ToLower()}
                    }}
                }}");
            };
        }

        private void SubscribeToDeviceLogs()
        {
            _rphPort.LogEventHandler += (s, e) =>
            {                
                var cleanedMessage = e.LogString.Replace("\r", "").Replace("\n", "").Trim();

                if (string.IsNullOrWhiteSpace(cleanedMessage))
                    return;

                var logType = e.EventType switch
                {
                    LogLineType.ERROR => "error",
                    LogLineType.INFO => "info",
                    _ => "info"
                };

                var logEntry = new
                {
                    type = "log",
                    data = new
                    {
                        level = logType,
                        message = cleanedMessage,
                        timestamp = DateTime.Now.ToString("HH:mm:ss.fff")
                    }
                };

                var json = JsonSerializer.Serialize(logEntry);
                Broadcast(json);
            };
        }

        public void Start()
        {
            if (TryStartWithPrefixes(new[] { $"http://localhost:{_port}/", $"http://+:{_port}/" }))
            {
                LogInfo($"Веб-сервер запущен на порту {_port} (сетевой доступ разрешён)");
                LogNetworkAddresses();
            }
            else if (TryStartWithPrefixes(new[] { $"http://localhost:{_port}/" }))
            {
                LogInfo($"Веб-сервер запущен на порту {_port} (только localhost)");
                LogInfo($"  Локальный доступ: http://localhost:{_port}/");
                LogInfo("Для включения сетевого доступа выполните от Администратора:");
                LogInfo($"  netsh http add urlacl url=http://+:{_port}/ user=%USERDOMAIN%\\%USERNAME%");
            }
            else
            {
                throw new InvalidOperationException($"Не удалось запустить веб-сервер на порту {_port}");
            }

            _ = Task.Run(ListenAsync);
        }

        private bool TryStartWithPrefixes(string[] prefixes)
        {
            try
            {
                _listener = new HttpListener();
                foreach (var prefix in prefixes)
                    _listener.Prefixes.Add(prefix);
                _listener.Start();
                _isRunning = true;
                return true;
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5)
            {
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Ошибка запуска: {ex.Message}");
                return false;
            }
        }

        private void LogNetworkAddresses()
        {
            try
            {
                LogInfo("Веб-интерфейс доступен по адресам:");
                LogInfo($"  Локально: http://localhost:{_port}/");

                var addedIps = new HashSet<string>();
                var hostName = Dns.GetHostName();
                var host = Dns.GetHostEntry(hostName);

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(ip))
                    {
                        var ipStr = ip.ToString();
                        if (addedIps.Add(ipStr))
                            LogInfo($"  Сеть: http://{ipStr}:{_port}/");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Не удалось получить сетевые адреса: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();

            lock (_wsLock)
            {
                foreach (var ws in _webSockets)
                {
                    try { ws.Dispose(); }
                    catch { }
                }
                _webSockets.Clear();
            }

            Thread.Sleep(200);

            try { _listener.Stop(); } catch { }
            try { _listener.Close(); } catch { }
        }

        public void Broadcast(string message)
        {
            List<WebSocketConnection> sockets;
            lock (_wsLock)
            {
                sockets = _webSockets.ToList();
            }

            foreach (var ws in sockets)
                _ = ws.SendAsync(message);
        }

        private string GetDeviceTypeName(RPH_DEVICE_TYPE_Enum type)
        {
            return type switch
            {
                RPH_DEVICE_TYPE_Enum.DT_DX => "DX",
                RPH_DEVICE_TYPE_Enum.DT_OEM => "OEM",
                RPH_DEVICE_TYPE_Enum.DT_OS => "OS",
                _ => "UNKNOWN"
            };
        }


        private async Task ListenAsync()
        {
            while (_isRunning && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequestAsync(context));
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        LogError($"Ошибка прослушивания: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Добавляем CORS заголовки
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                if (request.IsWebSocketRequest)
                {
                    await HandleWebSocketAsync(context);
                    return;
                }

                var path = request.Url?.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(path))
                    path = "index.html";

                // API endpoints
                if (path == "api/device/info")
                {
                    await ServeDeviceInfo(response);
                    return;
                }

                if (path == "api/device/connect")
                {
                    await HandleConnect(response);
                    return;
                }

                if (path == "api/settings")
                {
                    if (request.HttpMethod == "GET")
                        await ServeSettings(response);
                    else if (request.HttpMethod == "POST")
                        await HandleSaveSettings(request, response);
                    return;
                }

                // Статические файлы
                await ServeStaticFile(response, path);
            }
            catch (Exception ex)
            {
                LogError($"Ошибка обработки запроса: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }

        private async Task HandleWebSocketAsync(HttpListenerContext context)
        {
            WebSocketConnection? connection = null;

            try
            {
                var wsContext = await context.AcceptWebSocketAsync(null);
                connection = new WebSocketConnection(wsContext.WebSocket);

                lock (_wsLock) { _webSockets.Add(connection); }

                LogInfo("WebSocket клиент подключён");

                // Отправляем текущее состояние устройства
                await SendDeviceState(connection);

                await connection.ReceiveMessagesAsync(async message =>
                {
                    try
                    {
                        await ProcessWebSocketCommand(message, connection);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Ошибка WebSocket команды: {ex.Message}");
                        await connection.SendAsync($@"{{""type"":""error"",""message"":""{ex.Message}""}}");
                    }
                });

                LogInfo("WebSocket клиент отключён");
            }
            catch (Exception ex)
            {
                LogError($"WebSocket ошибка: {ex.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    lock (_wsLock) { _webSockets.Remove(connection); }
                    connection.Dispose();
                }
            }
        }

        private async Task SendDeviceState(WebSocketConnection connection)
        {
            // Отправляем команду очистки лога для нового клиента
            await connection.SendAsync(@"
            {
                ""type"": ""log_clear"",
                ""data"": {}
            }");

            // Отправляем статус устройства
            await connection.SendAsync($@"
            {{
                ""type"": ""device_status"",
                ""data"": {{
                    ""isValid"": {_rphPort.IsDeviceInfoValid.ToString().ToLower()},
                    ""deviceType"": {(int)_rphPort.DeviceType},
                    ""deviceTypeName"": ""{GetDeviceTypeName(_rphPort.DeviceType)}"",
                    ""serialNumber"": ""{_rphPort.SerialNumber}"",
                    ""systemVersion"": ""{_rphPort.SystemVersion}"",
                    ""isWaiting"": {_rphPort.IsWaitingLocal.ToString().ToLower()}
                }}
            }}");

            bool needRequest = false;
            // Отправляем последние данные SETS2 только если они есть
            lock (_dataLock)
            {
                if (_lastSets2Data != null)
                {
                    _ = connection.SendAsync($@"
                    {{
                        ""type"": ""settings"",
                        ""data"": {{
                            ""flashWrite"": {_lastSets2Data.FlashWrite.ToString().ToLower()},
                            ""ssbChannelId"": {(int)_lastSets2Data.SSBChannelID},
                            ""headphoneOutVolume"": {_lastSets2Data.HeadphoneOutVolume},
                            ""vadSensitivity"": {_lastSets2Data.VADSensitivity},
                            ""lowBatteryThresholdV"": {_lastSets2Data.LowBatteryThresholdV.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                            ""isRWLT"": {_lastSets2Data.IsRWLT.ToString().ToLower()},
                            ""rwltDiverId"": {_lastSets2Data.RWLT_DiverID},
                            ""flags1"": {_lastSets2Data.Flags1},
                            ""reserved1"": {_lastSets2Data.Reserved1},
                            ""reserved2"": {_lastSets2Data.Reserved2}
                        }}
                    }}");
                }
                else
                {
                    needRequest = true;
                }
            }

            // Если данных нет, но устройство подключено — запрашиваем
            if (needRequest && _rphPort.IsDeviceInfoValid)
            {
                _rphPort.Query_SETS2_Read();
            }
        }

        private async Task ProcessWebSocketCommand(string message, WebSocketConnection connection)
        {            

            var command = JsonSerializer.Deserialize<WebSocketCommand>(message);
            if (command == null) return;

            switch (command.Command)
            {
                case "connect":
                    _rphPort.Query_DINFO();
                    break;

                case "get_settings":
                    _rphPort.Query_SETS2_Read();
                    break;

                case "save_settings":
                    if (command.Data != null)
                    {
                        Sets2Data? settings = null;

                        if (command.Data is JsonElement jsonElement)
                        {
                            // Десериализуем напрямую из JsonElement
                            settings = JsonSerializer.Deserialize<Sets2Data>(jsonElement, _jsonOptions);

                            if (settings != null)
                            {
                                LogInfo($"Save: Vol={settings.HeadphoneOutVolume}, VAD={settings.VADSensitivity}, Bat={settings.LowBatteryThresholdV}, Flags={settings.Flags1}");

                                _rphPort.Query_SETS2_Write(
                                    flashWrite: settings.FlashWrite,
                                    ssbChID: (SSB_CH_ID_Enum)settings.SSBChannelId,
                                    hdOutVolume: (byte)settings.HeadphoneOutVolume,
                                    vadSensitivity: (byte)settings.VADSensitivity,
                                    lowBatThresholdV: settings.LowBatteryThresholdV,
                                    isRWLT: settings.IsRWLT,
                                    RWLT_DiverID: (byte)settings.RWLT_DiverID,
                                    flags: (byte)settings.Flags1
                                );
                            }
                            else
                            {
                                LogError($"Failed to deserialize. Raw JSON: {jsonElement.GetRawText()}");
                            }
                        }
                        else
                        {
                            LogError($"Data is not JsonElement, type: {command.Data.GetType()}");
                        }
                    }
                    break;

                default:
                    await connection.SendAsync($@"{{""type"":""error"",""message"":""Unknown command: {command.Command}""}}");
                    break;
            }
        }

        private async Task ServeDeviceInfo(HttpListenerResponse response)
        {
            var json = JsonSerializer.Serialize(new
            {
                isValid = _rphPort.IsDeviceInfoValid,
                deviceType = _rphPort.DeviceType,
                serialNumber = _rphPort.SerialNumber,
                systemInfo = _rphPort.SystemInfo,
                systemVersion = _rphPort.SystemVersion,
                isWaiting = _rphPort.IsWaitingLocal
            });

            response.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }

        private async Task HandleConnect(HttpListenerResponse response)
        {
            _rphPort.Query_DINFO();

            var json = JsonSerializer.Serialize(new { status = "connecting" });
            response.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }

        private async Task ServeSettings(HttpListenerResponse response)
        {
            SETS2ReceivedEventArgs? data;
            lock (_dataLock)
            {
                data = _lastSets2Data;
            }

            var json = JsonSerializer.Serialize(new
            {
                flashWrite = data?.FlashWrite ?? false,
                ssbChannelId = (int)(data?.SSBChannelID ?? SSB_CH_ID_Enum.CH_ID_INVALID),
                headphoneOutVolume = data?.HeadphoneOutVolume ?? 100,
                vadSensitivity = data?.VADSensitivity ?? 100,
                lowBatteryThresholdV = data?.LowBatteryThresholdV ?? 12.0,
                isRWLT = data?.IsRWLT ?? false,
                rwltDiverId = data?.RWLT_DiverID ?? 0,
                flags1 = data?.Flags1 ?? 0
            });

            response.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }

        private async Task HandleSaveSettings(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                using var reader = new StreamReader(request.InputStream);
                var body = await reader.ReadToEndAsync();
                var settings = JsonSerializer.Deserialize<Sets2Data>(body);

                if (settings != null)
                {
                    _rphPort.Query_SETS2_Write(
                        flashWrite: settings.FlashWrite,
                        ssbChID: (SSB_CH_ID_Enum)settings.SSBChannelId,
                        hdOutVolume: (byte)settings.HeadphoneOutVolume,
                        vadSensitivity: (byte)settings.VADSensitivity,
                        lowBatThresholdV: settings.LowBatteryThresholdV,
                        isRWLT: settings.IsRWLT,
                        RWLT_DiverID: (byte)settings.RWLT_DiverID,
                        flags: (byte)settings.Flags1
                    );

                    var json = JsonSerializer.Serialize(new { status = "ok" });
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = 400;
                    var json = JsonSerializer.Serialize(new { error = "Invalid data" });
                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка сохранения настроек: {ex.Message}");
                response.StatusCode = 400;
                var json = JsonSerializer.Serialize(new { error = ex.Message });
                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            response.Close();
        }

        private async Task ServeStaticFile(HttpListenerResponse response, string path)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"RedPhoneDXConfig.wwwroot.{path.Replace('/', '.')}";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }

                response.ContentType = GetContentType(path);
                response.ContentLength64 = stream.Length;
                
                await stream.CopyToAsync(response.OutputStream, 81920, _cts.Token);

                response.Close();
            }
            catch (Exception ex)
            {
                LogError($"Ошибка загрузки файла {path}: {ex.Message}");
                response.StatusCode = 500;
                response.Close();
            }
        }

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".svg" => "image/svg+xml",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }

        private void LogInfo(string message) =>
            _onLogAlways($"[WEB] {message}");

        private void LogError(string message) =>
            _onLogAlways($"[WEB ERROR] {message}");

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }

    // Вспомогательные классы
    public class WebSocketCommand
    {
        [System.Text.Json.Serialization.JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    public class Sets2Data
    {
        public bool FlashWrite { get; set; }
        public int SSBChannelId { get; set; }
        public int HeadphoneOutVolume { get; set; }
        public int VADSensitivity { get; set; }
        public double LowBatteryThresholdV { get; set; }
        public bool IsRWLT { get; set; }
        public int RWLT_DiverID { get; set; }
        public int Flags1 { get; set; }
    }

    public class WebSocketConnection : IDisposable
    {
        private readonly WebSocket _socket;
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        public string Id { get; } = Guid.NewGuid().ToString();

        public WebSocketConnection(WebSocket socket)
        {
            _socket = socket;
        }

        public async Task SendAsync(string message)
        {
            if (_disposed || _socket.State != WebSocketState.Open) return;

            try
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _socket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    _cts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }

        public async Task ReceiveMessagesAsync(Func<string, Task> onMessage)
        {
            var buffer = new byte[4096];

            try
            {
                while (_socket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), _cts.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            try
                            {
                                await _socket.CloseAsync(
                                    WebSocketCloseStatus.NormalClosure,
                                    "Closed by client",
                                    CancellationToken.None);
                            }
                            catch { }
                            break;
                        }

                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await onMessage(message);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (WebSocketException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            finally { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cts.Cancel();
            try { _cts.Dispose(); } catch { }
        }
    }
}