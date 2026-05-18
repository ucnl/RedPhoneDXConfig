### Сборка
#### Что нужно

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git (если будете клонировать, а не скачивать ZIP)

#### Быстрый старт

1. Скачайте репозиторий: `git clone https://github.com/ucnl/RedPhoneDXConfig.git`, или просто скачайте ZIP-архив с GitHub и распакуйте.
2. Запустите скрипт загрузки зависимостей. Откройте PowerShell в папке проекта и выполните: `powershell -File download\_nugets.ps1`. Скрипт сам скачает все необходимые NuGet-пакеты в папку `NuGetLocal`.
3. Соберите проект. В той же папке выполните: `dotnet build` или просто откройте `RedPhoneDXConfig.sln` в Visual Studio и нажмите **Сборка → Собрать решение**.

#### Если что-то пошло не так
- **PowerShell не даёт запустить скрипт** — выполните перед запуском: `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`

#### Зависимости

Проект использует следующие библиотеки (все с открытым исходным кодом):

| Библиотека | Репозиторий |
|-----------|-------------|
| UCNLDrivers | [github.com/ucnl/UCNLDrivers](https://github.com/ucnl/UCNLDrivers) |
| UCNLNMEA | [github.com/ucnl/UCNLNMEA](https://github.com/ucnl/UCNLNMEA) |


