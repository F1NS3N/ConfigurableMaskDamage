using System;
using System.IO;
using InControl;
using Modding;
using Modding.Converters;
using Newtonsoft.Json;
using Satchel;
using Satchel.BetterMenus;
using UnityEngine;

namespace ConfigurableMaskDamage
{
    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction IncreaseKey;
        public PlayerAction DecreaseKey;
        public PlayerAction ToggleKey;

        public KeyBinds()
        {
            IncreaseKey = CreatePlayerAction("Increase Damage");
            DecreaseKey = CreatePlayerAction("Decrease Damage");
            ToggleKey = CreatePlayerAction("Toggle UI");

            IncreaseKey.AddDefaultBinding(Key.O);
            DecreaseKey.AddDefaultBinding(Key.I);
            ToggleKey.AddDefaultBinding(Key.P);
        }
    }

    public class GlobalSettings
    {
        public int DamageMultiplier = 1;
        public bool IsDamageShow = true;


        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();
    }
    public class ConfigurableMaskDamage : Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings>
    {
        private Menu MenuRef;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates)
        {
            if (MenuRef == null)
            {
                Log("Creating new menu instance...");
                MenuRef = new Menu(
                    name: "ConfigurableMaskDamage",
                    elements: new Element[]
                    {
                new HorizontalOption(
                    name: "Show Damage Multiplier",
                    description: "Should the damage multiplier be active?",
                    values: new[] { "Yes", "No" },
                    applySetting: index =>
                    {
                        try
                        {
                            GS.IsDamageShow = index == 0;

                            OnSaveGlobal();

                            if (GS.IsDamageShow)
                            {
                                CreateUI();
                                UpdateDisplay();
                            }
                            else
                            {
                                ModDisplay.Instance?.Destroy();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"Error in applySetting: {ex}");
                        }
                    },
                    loadSetting: () => GS.IsDamageShow ? 0 : 1
                ),
                    new KeyBind(
                    name: "IncreaseKey",
                    playerAction: GS.keybinds.IncreaseKey),

                    new KeyBind(
                    name: "DecreaseKey",
                    playerAction: GS.keybinds.DecreaseKey),

                    new KeyBind(
                    name: "ToggleKey",
                    playerAction: GS.keybinds.ToggleKey),


                    }
                );

            }

            return MenuRef.GetMenuScreen(modListMenu);
        }
        public bool ToggleButtonInsideMenu { get; }

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
            if (GS.keybinds.IncreaseKey.WasPressed)
            {
                GS.DamageMultiplier++;
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{GS.DamageMultiplier}");
                OnSaveGlobal();
                UpdateDisplay();
            }

            if (GS.keybinds.DecreaseKey.WasPressed)
            {
                if (GS.DamageMultiplier > 1)
                {
                    GS.DamageMultiplier--;
                }
                Log($"[ConfigurableMaskDamage] Установлен множитель урона: x{GS.DamageMultiplier}");
                OnSaveGlobal();
                UpdateDisplay();
            }

            if (GS.keybinds.ToggleKey.WasPressed)
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