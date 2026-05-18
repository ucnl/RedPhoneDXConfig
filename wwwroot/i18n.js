const I18n = {
    currentLang: 'ru',

    translations: {
        ru: {
            status_disconnected: 'Отключено',
            status_connected: 'Подключено',
            status_busy: 'Занято',
            connection_title: 'Подключение',
            device_type: 'Тип устройства:',
            serial_number: 'Серийный номер:',
            firmware_version: 'Версия прошивки:',
            connect: 'Подключить',
            disconnect: 'Отключить',
            settings_title: 'Настройки SETS2',
            ssb_channel: 'SSB канал:',
            headphone_volume: 'Громкость эффектов:',
            vad_sensitivity: 'Чувствительность VAD:',
            low_battery_threshold: 'Порог низкого заряда (В):',
            rwlt_mode: 'Режим RWLT',
            rwlt_diver_id: 'ID водолаза RWLT:',
            flash_write: 'Записать во flash',
            read_settings: 'Прочитать настройки',
            save_settings: 'Сохранить настройки',
            log_title: 'Лог обмена',
            log_ready: 'Готов к работе...',
            log_connected: 'WebSocket подключён',
            log_disconnected: 'WebSocket отключён',
            log_connecting: 'Подключение к устройству...',
            log_device_found: 'Устройство обнаружено',
            log_settings_read: 'Настройки прочитаны',
            log_settings_saved: 'Настройки сохранены',
            log_error: 'Ошибка: ',
            device_unknown: 'Неизвестно',
            device_dx: 'Водолазная станция (DX)',
            device_oem: 'OEM-версия',
            device_os: 'Надводная станция (OS)',
            channel_invalid: 'Не выбран',
            flags_title: 'Флаги (Flags1):',
            flag0: 'Бит 0',
            flag1: 'Бит 1',
            flag2: 'Бит 2',
            flag3: 'Бит 3',
            flag4: 'Бит 4',
            flag5: 'Бит 5',
            flag6: 'Бит 6',
            flag7: 'Бит 7',
            flags_hex_value: 'Значение (hex):',
            auto_connecting: 'Автоматический поиск устройства...',
            device_connected: 'Устройство подключено',
            device_not_found: 'Устройство не найдено',
            org_name: 'UC&NL',
            footer_desc: 'Инструмент конфигурации с открытым исходным кодом',
        },
        en: {
            status_disconnected: 'Disconnected',
            status_connected: 'Connected',
            status_busy: 'Busy',
            connection_title: 'Connection',
            device_type: 'Device type:',
            serial_number: 'Serial number:',
            firmware_version: 'Firmware version:',
            connect: 'Connect',
            disconnect: 'Disconnect',
            settings_title: 'SETS2 Settings',
            ssb_channel: 'SSB Channel:',
            headphone_volume: 'Effects Volume:',
            vad_sensitivity: 'VAD Sensitivity:',
            low_battery_threshold: 'Low Battery Threshold (V):',
            rwlt_mode: 'RWLT Mode',
            rwlt_diver_id: 'RWLT Diver ID:',
            flash_write: 'Write to Flash',
            read_settings: 'Read Settings',
            save_settings: 'Save Settings',
            log_title: 'Exchange Log',
            log_ready: 'Ready...',
            log_connected: 'WebSocket connected',
            log_disconnected: 'WebSocket disconnected',
            log_connecting: 'Connecting to device...',
            log_device_found: 'Device found',
            log_settings_read: 'Settings read',
            log_settings_saved: 'Settings saved',
            log_error: 'Error: ',
            device_unknown: 'Unknown',
            device_dx: 'Diver Device (DX)',
            device_oem: 'OEM Version',
            device_os: 'Surface Station (OS)',
            channel_invalid: 'Not selected',
            flags_title: 'Flags (Flags1):',
            flag0: 'Bit 0',
            flag1: 'Bit 1',
            flag2: 'Bit 2',
            flag3: 'Bit 3',
            flag4: 'Bit 4',
            flag5: 'Bit 5',
            flag6: 'Bit 6',
            flag7: 'Bit 7',
            flags_hex_value: 'Value (hex):',
            auto_connecting: 'Auto-searching for device...',
            device_connected: 'Device connected',
            device_not_found: 'Device not found',
            org_name: 'UC&NL',
            footer_desc: 'Open Source Configuration Tool',
        }
    },

    init() {
        // Автоопределение языка
        const savedLang = localStorage.getItem('lang');
        if (savedLang) {
            this.currentLang = savedLang;
        } else {
            const browserLang = navigator.language || navigator.userLanguage;
            this.currentLang = browserLang.startsWith('ru') ? 'ru' : 'en';
        }

        document.getElementById('langSelector').value = this.currentLang;
        this.apply();
    },

    setLanguage(lang) {
        this.currentLang = lang;
        localStorage.setItem('lang', lang);
        this.apply();
    },

    apply() {
        document.querySelectorAll('[data-i18n]').forEach(element => {
            const key = element.getAttribute('data-i18n');
            if (this.translations[this.currentLang][key]) {
                element.textContent = this.translations[this.currentLang][key];
            }
        });
    },

    translate(key) {
        return this.translations[this.currentLang][key] || key;
    }
};

// Инициализация при загрузке
document.addEventListener('DOMContentLoaded', () => {
    I18n.init();

    document.getElementById('langSelector').addEventListener('change', (e) => {
        I18n.setLanguage(e.target.value);
    });
});