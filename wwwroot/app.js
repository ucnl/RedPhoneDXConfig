// app.js - Основная логика приложения
class DeviceConfig {
    constructor() {
        this.ws = null;
        this.isConnected = false;
        this.deviceInfo = null;
        this.maxLogEntries = 200; // Максимальное количество записей в логе

        this.initWebSocket();
        this.initEventListeners();
        this.updateUI();

        this.setDefaultFlags();
    }

    initWebSocket() {
        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        const wsUrl = `${protocol}//${window.location.host}`;

        this.addLog(I18n.translate('log_connecting'), 'info');

        this.ws = new WebSocket(wsUrl);

        this.ws.onopen = () => {
            this.isConnected = true;
            this.addLog(I18n.translate('log_connected'), 'success');
            this.updateConnectionStatus('connected');
        };

        this.ws.onmessage = (event) => {
            try {
                const message = JSON.parse(event.data);
                this.handleMessage(message);
            } catch (e) {
                console.error('Failed to parse message:', e);
            }
        };

        this.ws.onclose = () => {
            this.isConnected = false;
            this.addLog(I18n.translate('log_disconnected'), 'error');
            this.updateConnectionStatus('disconnected');

            // Автопереподключение через 3 секунды
            setTimeout(() => this.initWebSocket(), 3000);
        };

        this.ws.onerror = (error) => {
            console.error('WebSocket error:', error);
        };
    }

    initEventListeners() {
        // Слайдеры с отображением значений
        document.getElementById('headphoneVolume').addEventListener('input', (e) => {
            document.getElementById('headphoneVolumeValue').textContent = e.target.value + '%';
        });

        document.getElementById('vadSensitivity').addEventListener('input', (e) => {
            document.getElementById('vadSensitivityValue').textContent = e.target.value + '%';
        });

        // Показывать/скрывать поля RWLT
        document.getElementById('isRWLT').addEventListener('change', (e) => {
            document.getElementById('rwltGroup').style.display = e.target.checked ? 'block' : 'none';
        });

        // Обработчики для флагов
        for (let i = 0; i < 8; i++) {
            document.getElementById(`flag${i}`).addEventListener('change', () => {
                this.updateFlagsHexValue();
            });
        }                
    }

    handleMessage(message) {
        switch (message.type) {
            case 'device_status':
                this.handleDeviceStatus(message.data);
                break;

            case 'settings':
                this.handleSettings(message.data);
                break;

            case 'busy_status':
                this.handleBusyStatus(message.data);
                break;

            case 'log':
                this.handleLogMessage(message.data);
                break;

            case 'log_clear':
                this.handleLogClear();
                break;

            default:
                console.log('Unknown message type:', message.type);
        }
    }

    handleDeviceStatus(data) {
        this.deviceInfo = data;

        const autoStatus = document.querySelector('.auto-connect-status span:last-child');
        const indicator = document.querySelector('.connect-indicator');

        if (data.isValid) {
            document.getElementById('deviceType').textContent = this.getDeviceTypeName(data.deviceType);
            document.getElementById('serialNumber').textContent = data.serialNumber || '-';
            document.getElementById('firmwareVersion').textContent = data.systemVersion || '-';

            if (autoStatus) autoStatus.textContent = I18n.translate('device_connected');
            if (indicator) indicator.className = 'connect-indicator connected';
            
        } else {
            if (autoStatus) autoStatus.textContent = I18n.translate('device_not_found');
            if (indicator) indicator.className = 'connect-indicator disconnected';            
        }

        if (data.isWaiting) {
            this.updateConnectionStatus('busy');
        } else {
            this.updateConnectionStatus('connected');
        }

        this.updateUI();
    }    

    handleSettings(data) {
        const channelId = (data.ssbChannelId != null) ? data.ssbChannelId : 16;

        document.getElementById('ssbChannel').value = channelId;
        document.getElementById('headphoneVolume').value = data.headphoneOutVolume ?? 100;
        document.getElementById('headphoneVolumeValue').textContent = (data.headphoneOutVolume ?? 100) + '%';
        document.getElementById('vadSensitivity').value = data.vadSensitivity ?? 100;
        document.getElementById('vadSensitivityValue').textContent = (data.vadSensitivity ?? 100) + '%';
        document.getElementById('lowBatteryThreshold').value = data.lowBatteryThresholdV ?? 12.0;
        document.getElementById('isRWLT').checked = data.isRWLT ?? false;
        document.getElementById('rwltGroup').style.display = data.isRWLT ? 'block' : 'none';
        document.getElementById('rwltDiverId').value = data.rwltDiverId ?? 0;
        document.getElementById('flashWrite').checked = data.flashWrite ?? false;

        const flags = data.flags1 ?? 0;
        this.setFlagsFromValue(flags);
    }

    handleBusyStatus(data) {
        if (data.isWaiting) {
            this.updateConnectionStatus('busy');
        } else {
            this.updateConnectionStatus('connected');
        }
    }

    handleLogMessage(data) {
        const timestamp = data.timestamp || new Date().toLocaleTimeString();
        const message = data.message || '';
        const level = data.level || 'info';

        // Цветовая схема для разных уровней
        const levelClass = level === 'error' ? 'error' :
            level === 'warning' ? 'warning' : 'info';

        this.addLog(`[${timestamp}] ${message}`, levelClass);
    }

    handleLogClear() {
        const logContainer = document.getElementById('logContainer');
        logContainer.innerHTML = '';
        this.addLog(I18n.translate('log_ready'), 'info');
    }

    setDefaultFlags() {
        // Устанавливаем биты 0 и 7 по умолчанию (129 = 0x81)
        const defaultFlags = 129;
        this.setFlagsFromValue(defaultFlags);
    }

    setFlagsFromValue(value) {
        for (let i = 0; i < 8; i++) {
            const checkbox = document.getElementById(`flag${i}`);
            if (checkbox) {
                checkbox.checked = (value & (1 << i)) !== 0;
            }
        }
        this.updateFlagsHexValue();
    }

    getFlagsValue() {
        let value = 0;
        for (let i = 0; i < 8; i++) {
            const checkbox = document.getElementById(`flag${i}`);
            if (checkbox && checkbox.checked) {
                value |= (1 << i);
            }
        }
        return value;
    }

    updateFlagsHexValue() {
        const value = this.getFlagsValue();
        document.getElementById('flagsHexValue').textContent =
            '0x' + value.toString(16).toUpperCase().padStart(2, '0');
    }

    sendCommand(command, data = null) {
        if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
            this.addLog(I18n.translate('log_error') + 'Нет подключения к серверу', 'error');
            return;
        }

        const message = JSON.stringify({ command, data });
        this.ws.send(message);
    }

    readSettings() {
        this.sendCommand('get_settings');
    }

    saveSettings() {
        if (!this.deviceInfo || !this.deviceInfo.isValid) {
            this.addLog('Устройство не подключено', 'error');
            return;
        }

        if (!this.validateForm()) return;

        const settings = {
            flashWrite: document.getElementById('flashWrite').checked,
            ssbChannelId: parseInt(document.getElementById('ssbChannel').value),
            headphoneOutVolume: parseInt(document.getElementById('headphoneVolume').value),
            vadSensitivity: parseInt(document.getElementById('vadSensitivity').value),
            lowBatteryThresholdV: parseFloat(document.getElementById('lowBatteryThreshold').value),
            isRWLT: document.getElementById('isRWLT').checked,
            rwltDiverId: parseInt(document.getElementById('rwltDiverId').value),
            flags1: this.getFlagsValue()
        };

        this.sendCommand('save_settings', settings);
        // Устройство само ответит → SETS2Received → форма обновится автоматически
    }

    updateConnectionStatus(status) {
        const badge = document.getElementById('connectionStatus');
        badge.className = 'status-badge ' + status;

        const statusTexts = {
            'connected': I18n.translate('status_connected'),
            'disconnected': I18n.translate('status_disconnected'),
            'busy': I18n.translate('status_busy')
        };

        badge.textContent = statusTexts[status] || status;
    }

    updateUI() {
        const hasDevice = this.deviceInfo && this.deviceInfo.isValid;

        // Блокируем только кнопки с атрибутом data-requires-device
        document.querySelectorAll('[data-requires-device="true"]').forEach(element => {
            element.disabled = !hasDevice;
        });
    }

    getDeviceTypeName(type) {
        const types = {
            0: I18n.translate('device_dx'),     // DT_DX - Водолазный прибор
            1: I18n.translate('device_oem'),    // DT_OEM - OEM-версия
            2: I18n.translate('device_os'),     // DT_OS - Надводная станция
            3: I18n.translate('device_unknown') // DT_UNKNOWN
        };
        return types[type] || I18n.translate('device_unknown');
    }

    getChannelName(channelId) {
        const channels = {
            0: '#1 — 32768 Hz, LSB',
            1: '#2 — 32768 Hz, HSB',
            2: '#3 — 31250 Hz, LSB',
            3: '#4 — 31250 Hz, HSB',
            4: '#5 — 28500 Hz, LSB',
            5: '#6 — 28500 Hz, HSB',
            6: '#7 — 25000 Hz, LSB',
            7: '#8 — 25000 Hz, HSB'
        };

        if (channels[channelId]) return channels[channelId];
        return I18n.translate('channel_invalid');
    }

    validateForm() {
        const lowBattery = parseFloat(document.getElementById('lowBatteryThreshold').value);
        const rwltDiverId = parseInt(document.getElementById('rwltDiverId').value);
        const isRWLT = document.getElementById('isRWLT').checked;

        if (isNaN(lowBattery) || lowBattery < 4.0 || lowBattery > 36.0) {
            this.addLog('Ошибка: порог напряжения должен быть 4.0 – 36.0 В', 'error');
            return false;
        }

        if (isRWLT && (isNaN(rwltDiverId) || rwltDiverId < 0 || rwltDiverId > 255)) {
            this.addLog('Ошибка: ID водолаза должен быть 0 – 255', 'error');
            return false;
        }

        return true;
    }

    addLog(message, type = '') {
        const logContainer = document.getElementById('logContainer');
        const entry = document.createElement('div');
        entry.className = 'log-entry ' + type;

        // Добавляем временную метку если её ещё нет
        if (!message.startsWith('[')) {
            const timestamp = new Date().toLocaleTimeString();
            entry.textContent = `[${timestamp}] ${message}`;
        } else {
            entry.textContent = message;
        }

        logContainer.appendChild(entry);

        // Автопрокрутка вниз
        logContainer.scrollTop = logContainer.scrollHeight;

        // Ограничиваем количество записей
        while (logContainer.children.length > this.maxLogEntries) {
            logContainer.removeChild(logContainer.firstChild);
        }
    }

    clearLog() {
        const logContainer = document.getElementById('logContainer');
        logContainer.innerHTML = '';
        this.addLog(I18n.translate('log_ready'), 'info');
    }
}

// Запуск приложения
document.addEventListener('DOMContentLoaded', () => {
    window.deviceConfig = new DeviceConfig();
});