using System;
using System.IO;
using Modding;
using UnityEngine;

namespace ConfigurableMaskDamage
{
    public class ConfigurableMaskDamage : Mod
    {
        // Используем класс Settings для управления настройками
        private readonly Settings _settings = new Settings();

        // Конструктор мода
        public ConfigurableMaskDamage() : base("ConfigurableMaskDamage") { }

        public override string GetVersion() => "1.0.0";

        public override void Initialize()
        {
            Log("ConfigurableMaskDamage Initializing...");

            // Загружаем настройки
            LoadSettings();

            // Подключаем хуки
            ModHooks.AfterTakeDamageHook += OnAfterTakeDamage;
            ModHooks.HeroUpdateHook += OnHeroUpdate;

            // Создаем UI
            CreateUI();
        }

        private void CreateUI()
        {
            if (ModDisplay.Instance == null)
            {
                ModDisplay.Instance = new ModDisplay();
            }
            else
            {
                ModDisplay.Instance.Destroy(); // Удаляем старый UI
                ModDisplay.Instance = new ModDisplay(); // Создаем новый
            }
        }

        // Функция, вызываемая при получении урона
        private int OnAfterTakeDamage(int hazardType, int damageAmount)
        {
            Log($"{hazardType}");
            return damageAmount * _settings.DamageMultiplier;
        }

        // Логика управления через клавиши
        private void OnHeroUpdate()
        {
            if (Input.GetKeyDown(_settings.KeyIncrease))
            {
                _settings.DamageMultiplier++;
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{_settings.DamageMultiplier}");
                SaveSettings();
                UpdateDisplay();
            }

            if (Input.GetKeyDown(_settings.KeyDecrease))
            {
                if (_settings.DamageMultiplier > 1)
                {
                    _settings.DamageMultiplier--;
                }
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{_settings.DamageMultiplier}");
                SaveSettings();
                UpdateDisplay();
            }

            if (Input.GetKeyDown(_settings.KeyToggle))
            {
                _settings.IsDamageShow = !_settings.IsDamageShow;
                if (_settings.IsDamageShow)
                {
                    CreateUI(); // Создание UI при включении мода
                    UpdateDisplay();
                }
                else
                {
                    ModDisplay.Instance.Destroy(); // Удаление UI при выключении
                }
                Log($"[ConfigurableMaskDamage] Изменение урона: {(_settings.IsDamageShow ? "Показывается" : "Скрыто")}");
                SaveSettings();
            }
        }

        private void UpdateDisplay()
        {
            if (ModDisplay.Instance != null)
            {
                ModDisplay.Instance.Display($"DamageMultiplier: {_settings.DamageMultiplier}");
            }
        }

        // Загрузка настроек
        private void LoadSettings()
        {
            try
            {
                // Получаем путь к файлу настроек
                string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "..", "LocalLow", "Team Cherry", "Hollow Knight", "ConfigurableMaskDamage.GlobalSettings.json");

                // Проверяем, существует ли файл
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    SettingsData data = JsonUtility.FromJson<SettingsData>(json);

                    // Применяем настройки
                    _settings.OnLoadGlobal(data);

                    Log("[ConfigurableMaskDamage] Настройки загружены из GlobalSettings.json.");
                }
                else
                {
                    Log("[ConfigurableMaskDamage] Файл настроек не найден. Используются значения по умолчанию.");
                }
            }
            catch (Exception ex)
            {
                LogError($"[ConfigurableMaskDamage] Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        // Сохранение настроек
        private void SaveSettings()
        {
            try
            {
                // Получаем путь к файлу настроек
                string settingsPath = Path.Combine(Application.persistentDataPath, "ConfigurableMaskDamage.GlobalSettings.json");
                string backupPath = settingsPath + ".bak";

                // Создаем директорию, если она не существует
                string directoryPath = Path.GetDirectoryName(settingsPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log($"[ConfigurableMaskDamage] Создана директория: {directoryPath}");
                }

                // Создаем резервную копию существующего файла
                if (File.Exists(settingsPath))
                {
                    File.Copy(settingsPath, backupPath, overwrite: true);
                }

                // Сохраняем новые данные
                var data = _settings.OnSaveGlobal();
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(settingsPath, json);

                Log("[ConfigurableMaskDamage] Настройки сохранены в GlobalSettings.json.");
            }
            catch (Exception ex)
            {
                LogError($"[ConfigurableMaskDamage] Ошибка при сохранении настроек: {ex.Message}");
            }
        }
    }
}