// RedPhoneDXConfig/Program.cs
using RedPhoneDXConfig.Web;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using UCNLDrivers;

namespace RedPhoneDXConfig
{
    class Program
    {
        private static RPHPort? _rphPort;
        private static WebServer? _webServer;
        private static bool _isRunning = true;

        // Настройки по умолчанию
        private static int _webPort = 8080;
        private static string _portName = string.Empty;
        private static BaudRate _baudRate = BaudRate.baudRate9600;

        static async Task Main(string[] args)
        {

            Console.WriteLine($"{GetFullVersionInfo()} (C) UC&NL, https://unavlab.com");
            Console.WriteLine();

            ParseCommandLineArgs(args);

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _isRunning = false;
                Console.WriteLine("\nЗавершение работы...");
            };

            try
            {
                if (string.IsNullOrEmpty(_portName))
                {
                    Console.WriteLine("Порт не указан. RPHPort будет автоматически искать устройство...");
                    _portName = "AUTO";
                }
                else
                {
                    Console.WriteLine($"Указанный порт: {_portName}");
                }

                Console.WriteLine($"Скорость: {_baudRate}");
                Console.WriteLine($"Веб-порт: {_webPort}");
                Console.WriteLine();

                _rphPort = new RPHPort(_portName, _baudRate);

                SubscribeToDeviceEvents();

                _webServer = new WebServer(_webPort, _rphPort, Console.WriteLine);
                _webServer.Start();

                _rphPort.Start();

                Console.WriteLine();
                Console.WriteLine("Нажмите Ctrl+C для выхода...");

                try
                {
                    var url = $"http://localhost:{_webPort}/";
                    Console.WriteLine($"Открытие браузера: {url}");
                    OpenBrowser(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось открыть браузер: {ex.Message}");
                    Console.WriteLine($"Откройте вручную: http://localhost:{_webPort}/");
                }


                Console.WriteLine("Доступные команды:");
                Console.WriteLine("  i - запросить информацию об устройстве");
                Console.WriteLine("  s - запросить настройки SETS2");
                Console.WriteLine("  q - выход");
                Console.WriteLine();



                // Основной цикл с обработкой консольных команд
                while (_isRunning)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Q ||
                            (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C))
                        {
                            _isRunning = false;
                            break;
                        }

                        await ProcessConsoleKey(key);
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Cleanup();
            }
        }

        private static void SubscribeToDeviceEvents()
        {
            if (_rphPort == null) return;

            _rphPort.DeviceInfoValidChanged += (s, e) =>
            {
                if (_rphPort.IsDeviceInfoValid)
                {
                    Console.WriteLine($"[DEVICE] Тип: {_rphPort.DeviceType}, SN: {_rphPort.SerialNumber}, Версия: {_rphPort.SystemVersion}");
                }
            };           

            _rphPort.IsWaitingLocalChanged += (s, e) =>
            {
                if (!_rphPort.IsWaitingLocal)
                {
                    Console.WriteLine("[DEVICE] Готово к приёму команд");
                }
            };

            _rphPort.LogEventHandler += (s, e) =>
            {
                var lString = e.LogString.Replace("\r", "").Replace("\n", "").Trim();

                if (e.EventType == UCNLDrivers.LogLineType.ERROR)
                    Console.WriteLine($"[ERROR] {lString}");
                else if (e.EventType == UCNLDrivers.LogLineType.INFO)
                    Console.WriteLine($"[INFO] {lString}");
            };
        }

        private static async Task ProcessConsoleKey(ConsoleKeyInfo key)
        {
            if (_rphPort == null) return;

            try
            {
                switch (char.ToLower(key.KeyChar))
                {
                    case 'i':
                        Console.WriteLine("Запрос информации об устройстве...");
                        if (_rphPort.Query_DINFO())
                            Console.WriteLine("Запрос отправлен");
                        else
                            Console.WriteLine("Устройство занято или не подключено");
                        break;

                    case 's':
                        Console.WriteLine("Запрос настроек SETS2...");
                        if (_rphPort.Query_SETS2_Read())
                            Console.WriteLine("Запрос отправлен");
                        else
                            Console.WriteLine("Устройство занято или не подключено");
                        break;

                    case 'h':
                        Console.WriteLine("\nДоступные команды:");
                        Console.WriteLine("  i - запросить информацию об устройстве");
                        Console.WriteLine("  s - запросить настройки SETS2");
                        Console.WriteLine("  q - выход");
                        Console.WriteLine();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения команды: {ex.Message}");
            }
        }

        private static void ParseCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--port":
                    case "-p":
                        if (i + 1 < args.Length)
                            _portName = args[++i];
                        break;

                    case "--web-port":
                    case "-w":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int webPort))
                            _webPort = webPort;
                        break;

                    case "--baud":
                    case "-b":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int baud))
                            _baudRate = (BaudRate)baud;
                        break;

                    case "--help":
                    case "-h":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Использование: RedPhoneDXConfig [опции]");
            Console.WriteLine();
            Console.WriteLine("Опции:");
            Console.WriteLine("  --port, -p <порт>     Указать COM порт (по умолчанию: автоопределение)");
            Console.WriteLine("  --baud, -b <скорость> Скорость порта (по умолчанию: 9600)");
            Console.WriteLine("  --web-port, -w <порт> Порт веб-сервера (по умолчанию: 8080)");
            Console.WriteLine("  --help, -h            Показать эту справку");
            Console.WriteLine();
            Console.WriteLine("Примеры:");
            Console.WriteLine("  RedPhoneDXConfig");
            Console.WriteLine("  RedPhoneDXConfig --port COM3 --web-port 8888");
            Console.WriteLine("  RedPhoneDXConfig -p COM5 -w 80");
        }

        private static string GetApplicationVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return versionAttribute?.InformationalVersion ?? "Unknown";
        }

        private static string GetFullVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = assembly?.GetName().Name ?? "Unknown";
            var versionAttribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return $"{name} v{(versionAttribute?.InformationalVersion ?? "Unknown")}";
        }

        private static void OpenBrowser(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch
            {
                throw;
            }
        }

        private static void Cleanup()
        {
            Console.WriteLine("Остановка сервисов...");

            try
            {
                _webServer?.Stop();
                _webServer?.Dispose();
                Console.WriteLine("Веб-сервер остановлен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка остановки веб-сервера: {ex.Message}");
            }

            try
            {
                // RPHPort освобождается через uAuxPort
                _rphPort = null;
                Console.WriteLine("Порт освобождён");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка освобождения порта: {ex.Message}");
            }

            Console.WriteLine("Приложение завершено");
        }
    }
}