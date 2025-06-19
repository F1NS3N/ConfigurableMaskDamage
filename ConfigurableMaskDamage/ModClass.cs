using System;
using System.IO;
using Modding;
using UnityEngine;

namespace ConfigurableMaskDamage
{
    public class ConfigurableMaskDamage : Mod
    {
        private int damageMultiplier = 1; // множитель урона
        private bool isDamageShow = true; // показать или скрыть мод



        // бинды по умолчанию
        private KeyCode keyIncrease = KeyCode.O;
        private KeyCode keyDecrease = KeyCode.I;
        private KeyCode keyToggle = KeyCode.P;

        // путь до сейвов мода и создание его бэкапа
        private string settingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "..", "LocalLow", "Team Cherry", "Hollow Knight", "ConfigurableMaskDamage.GlobalSettings.json");
        private string backupPath => settingsPath + ".bak";

        // конструктор мода
        public ConfigurableMaskDamage() : base("ConfigurableMaskDamage") { }

        public override string GetVersion() => "1.0.2";

        public override void Initialize() // Хуки на которые надо ссылаться, загрузка настроек, чекать героя и когда он получает  дамаг
        {
            Log("ConfigurableMaskDamage Initializing...");

            LoadSettings();

            ModHooks.AfterTakeDamageHook += OnAfterTakeDamage;
            ModHooks.HeroUpdateHook += OnHeroUpdate;

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
                ModDisplay.Instance = new ModDisplay(); // Создаём новый
            }
        }
        // сама функция которая вызывается при получении урона :3
        private int OnAfterTakeDamage(int hazardType, int damageAmount)
        {
            Log($"{hazardType}");
            return damageAmount * damageMultiplier;

        }

        // логика управления через клавиши
        private void OnHeroUpdate()
        {
            if (Input.GetKeyDown(keyIncrease))
            {
                damageMultiplier++;
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{damageMultiplier}");
                SaveSettings();
                UpdateDisplay();
            }

            if (Input.GetKeyDown(keyDecrease))
            {
                if (damageMultiplier > 1)
                {
                    damageMultiplier--;
                }
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{damageMultiplier}");
                SaveSettings();
                UpdateDisplay();
            }

            if (Input.GetKeyDown(keyToggle))
            {
                isDamageShow = !isDamageShow;
                if (isDamageShow)
                {
                    CreateUI(); // Создание UI при включении мода
                    UpdateDisplay();
                }
                else
                {
                    ModDisplay.Instance.Destroy(); // Удаление UI при выключении
                }
                Log($"[ConfigurableMaskDamage] Изменение урона: {(isDamageShow ? "Показывается" : "Скрыто")}");
                SaveSettings();
            }
        }
        private void UpdateDisplay()
        {
            if (ModDisplay.Instance != null)
            {
                ModDisplay.Instance.Display($"DamageMultiplier: {damageMultiplier}");
            }
        }

        [Serializable]

        // структура файла-настройки
        private class SettingsData
        {
            public int DamageMultiplier = 1;
            public bool IsDamageShow = true;
            public string KeyIncrease = "O";
            public string KeyDecrease = "I";
            public string KeyToggle = "P";
        }

        // логика загрузки настроек
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    SettingsData data = JsonUtility.FromJson<SettingsData>(json);

                    damageMultiplier = data.DamageMultiplier;
                    isDamageShow = data.IsDamageShow;

                    Enum.TryParse(data.KeyIncrease, out keyIncrease);
                    Enum.TryParse(data.KeyDecrease, out keyDecrease);
                    Enum.TryParse(data.KeyToggle, out keyToggle);

                    Log("[ConfigurableMaskDamage] Настройки загружены из GlobalSettings.json.");
                }
                else if (File.Exists(backupPath))
                {
                    string json = File.ReadAllText(backupPath);
                    SettingsData data = JsonUtility.FromJson<SettingsData>(json);

                    damageMultiplier = data.DamageMultiplier;
                    isDamageShow = data.IsDamageShow;

                    Enum.TryParse(data.KeyIncrease, out keyIncrease);
                    Enum.TryParse(data.KeyDecrease, out keyDecrease);
                    Enum.TryParse(data.KeyToggle, out keyToggle);

                    Log("[ConfigurableMaskDamage] Настройки загружены из .bak файла.");
                    SaveSettings(); // Восстанавливаем основной файл
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


        // логика сохранения настроек
        private void SaveSettings()
        {
            try
            {
                // Создаём необходимые папки, если они отсутствуют
                string directoryPath = Path.GetDirectoryName(settingsPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log($"[ConfigurableMaskDamage] Создана директория: {directoryPath}");
                }

                var data = new SettingsData
                {
                    DamageMultiplier = damageMultiplier,
                    IsDamageShow = isDamageShow,
                    KeyIncrease = keyIncrease.ToString(),
                    KeyDecrease = keyDecrease.ToString(),
                    KeyToggle = keyToggle.ToString()
                };

                string json = JsonUtility.ToJson(data, true);

                // Создаём резервную копию, если существует оригинальный файл
                if (File.Exists(settingsPath))
                {
                    File.Copy(settingsPath, backupPath, overwrite: true);
                }

                // Сохраняем новые данные
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