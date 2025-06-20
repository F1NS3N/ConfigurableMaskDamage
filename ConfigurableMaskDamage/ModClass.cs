using System;
using System.IO;
using Modding;
using UnityEngine;

namespace ConfigurableMaskDamage
{

    public class GlobalSettings
    {
        public int DamageMultiplier = 1;
        public bool IsDamageShow = true;
        public KeyCode KeyIncrease = KeyCode.O;
        public KeyCode KeyDecrease = KeyCode.I; 
        public KeyCode KeyToggle = KeyCode.P;  
    }
    public class ConfigurableMaskDamage : Mod, IGlobalSettings<GlobalSettings>
    {

        // Конструктор мода
        public ConfigurableMaskDamage() : base("ConfigurableMaskDamage") { }

        public override string GetVersion() => "1.0.0";

    public static GlobalSettings GS { get; set; } = new GlobalSettings();

    public void OnLoadGlobal(GlobalSettings s)
    {
        GS = s;
    }

    public GlobalSettings OnSaveGlobal()
    {
        return GS;
    }

    public override void Initialize()
        {
            Log("ConfigurableMaskDamage Initializing...");

            // всякие хуки
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
                ModDisplay.Instance = new ModDisplay(); // Создаем новый
            }
        }

        // Функция, вызываемая при получении урона
        private int OnAfterTakeDamage(int hazardType, int damageAmount)
        {
            Log($"{hazardType}");
            return damageAmount * GS.DamageMultiplier;
        }

        // Логика управления через клавиши
        private void OnHeroUpdate()
        {
            if (Input.GetKeyDown(GS.KeyIncrease))
            {
                GS.DamageMultiplier++;
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{GS.DamageMultiplier}");
                OnSaveGlobal();
                UpdateDisplay();
            }

            if (Input.GetKeyDown(GS.KeyDecrease))
            {
                if (GS.DamageMultiplier > 1)
                {
                    GS.DamageMultiplier--;
                }
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{GS.DamageMultiplier}");
                OnSaveGlobal();
                UpdateDisplay();
            }

            if (Input.GetKeyDown(GS.KeyToggle))
            {
                GS.IsDamageShow = !GS.IsDamageShow;
                if (GS.IsDamageShow)
                {
                    CreateUI(); // Создание UI при включении мода
                    UpdateDisplay();
                }
                else
                {
                    ModDisplay.Instance.Destroy(); // Удаление UI при выключении
                }
                Log($"[ConfigurableMaskDamage] Изменение урона: {(GS.IsDamageShow ? "Показывается" : "Скрыто")}");
                OnSaveGlobal();
            }
        }

        private void UpdateDisplay()
        {
            if (ModDisplay.Instance != null)
            {
                ModDisplay.Instance.Display($"DamageMultiplier: {GS.DamageMultiplier}");
            }

        }
    }
}