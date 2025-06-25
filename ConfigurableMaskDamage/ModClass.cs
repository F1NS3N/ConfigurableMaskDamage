using System;
using InControl;
using Modding;
using Modding.Converters;
using Newtonsoft.Json;
using Satchel.BetterMenus;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // стандартная механика мода
        public int DamageMultiplier = 1;
        public bool IsDamageShow = true;


        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();

        // шаманские фичи
        public bool EnableTimeDamage = false;
        public bool DamageAnimation = true;
        public float TimeBetweenDamage = 10f;   // Интервал урона по времени (в секундах)


    }
    public class ConfigurableMaskDamage : Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings>
    {


        public float damageTimer = 0f;
        public bool isPlayerAlive = true;

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
                    playerAction: GS.keybinds.IncreaseKey
                ),

                new KeyBind(
                    name: "DecreaseKey",
                    playerAction: GS.keybinds.DecreaseKey
                ),

                new KeyBind(
                    name: "ToggleKey",
                    playerAction: GS.keybinds.ToggleKey
                ),

                // --- HorizontalOption: Enable Time Damage ---
                new HorizontalOption(
                    name: "Enable Time Damage",
                    description: "Enable or disable passive damage over time",
                    values: new[] { "On", "Off" },
                    applySetting: index =>
                    {
                        GS.EnableTimeDamage = index == 0; // 0 = On, 1 = Off
                        OnSaveGlobal();
                    },
                    loadSetting: () => GS.EnableTimeDamage ? 0 : 1
                ),
                    new HorizontalOption(
                    name: "Damage animation",
                    description: "Allows you to enable or disable the damage animation.",
                    values: new[] { "On", "Off" },
                    applySetting: index =>
                    {
                        GS.DamageAnimation = index == 0; // 0 = On, 1 = Off
                        OnSaveGlobal();
                    },
                    loadSetting: () => GS.DamageAnimation ? 0 : 1
                ),


                new CustomSlider(
                    name: "TimeBetweenDamage",
                    storeValue: val => GS.TimeBetweenDamage = val,
                    loadValue: () => GS.TimeBetweenDamage, //to load the value on menu creation
                    minValue: 1f,
                    maxValue: 60f,
                    wholeNumbers: false
                )
            }
            );

            }

            return MenuRef.GetMenuScreen(modListMenu);
        }


        public bool ToggleButtonInsideMenu { get; }

        // Конструктор мода
        public ConfigurableMaskDamage() : base("ConfigurableMaskDamage") { }

        public override string GetVersion() => "1.1.2";

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
            ModHooks.AfterPlayerDeadHook += OnPlayerDead;

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.NewGameHook += () =>
            {
                damageTimer = 0f;
                isPlayerAlive = true;
            };
            ModHooks.SavegameLoadHook += (slot) =>
            {
                damageTimer = 0f;
                isPlayerAlive = true;
            };

            CreateUI();
        }
        private bool IsPlayerBlocked()
        {
            if (GameManager.instance == null || HeroController.instance == null)
                return true;

            // Проверяем, если игрок не может управлять персонажем
            bool isInputDisabled = !HeroController.instance.acceptingInput;



            return isInputDisabled;
        }
        private void CreateUI()
        {
            if (ModDisplay.Instance == null)
            {
                ModDisplay.Instance = new ModDisplay(); // Создаём новый
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

        private void OnPlayerDead()
        {
            isPlayerAlive = false;
            damageTimer = 0f;
        }
        // Логика управления через клавиши
        private void OnHeroUpdate()
        {
            if (HeroController.instance == null || PlayerData.instance == null)
            {
                isPlayerAlive = false;
                return;
            }

            isPlayerAlive = PlayerData.instance.health > 0;
            if (!isPlayerAlive) return;

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Сейф сцены снова
            string[] safeScenes = new string[]
            {
        "GG_Atrium",
        "GG_Atrium_Roof",
        "GG_Blue_Room",
        "GG_Workshop"
            };

            bool isInSafeZone = Array.Exists(safeScenes, scene => scene == currentScene);
            bool isBossRush = PlayerData.instance.GetBool("bossRushMode");

            // можно ли наносить урон по времени
            bool canApplyTimeDamage = GS.EnableTimeDamage && !IsPlayerBlocked();
            bool shouldPauseTimer = isInSafeZone && isBossRush;

            if (canApplyTimeDamage && !shouldPauseTimer)
            {
                damageTimer += Time.deltaTime;

                if (damageTimer >= GS.TimeBetweenDamage)
                {
                    if (GS.DamageAnimation)
                    {
                        HeroController.instance.TakeDamage(
                            go: null,
                            damageSide: 0,
                            hazardType: 999,
                            damageAmount: 1
                        );
                        Log($"[TimeDamage] Time-based damage applied: -1 mask");
                        damageTimer = 0f;
                    }
                    else
                    {
                        if (PlayerData.instance.health == 1)
                        {
                            HeroController.instance.TakeDamage(
                                go: null,
                                damageSide: 0,
                                hazardType: 999,
                                damageAmount: 1
                            );
                        }
                        else
                        {
                            HeroController.instance.TakeHealth(1 * GS.DamageMultiplier);
                            damageTimer = 0f;
                        }
                    }
                }
            }

            UpdateDisplay();

            // --- Управление клавишами ---
            if (GS.keybinds.IncreaseKey.WasPressed)
            {
                GS.DamageMultiplier++;
                Log($"[ConfigurableMaskDamage] Damage multiplier set to: x{GS.DamageMultiplier}");
                OnSaveGlobal();
                UpdateDisplay();
            }
            if (GS.keybinds.DecreaseKey.WasPressed)
            {
                if (GS.DamageMultiplier > 1)
                {
                    GS.DamageMultiplier--;
                }
                Log($"[ConfigurableMaskDamage] Damage multiplier set to: x{GS.DamageMultiplier}");
                OnSaveGlobal();
                UpdateDisplay();
            }
            if (GS.keybinds.ToggleKey.WasPressed)
            {
                GS.IsDamageShow = !GS.IsDamageShow;
                if (GS.IsDamageShow)
                {
                    CreateUI();
                    UpdateDisplay();
                }
                else
                {
                    ModDisplay.Instance?.Destroy();
                }
                Log($"[ConfigurableMaskDamage] Mask damage display: {(GS.IsDamageShow ? "Visible" : "Hidden")}");
                OnSaveGlobal();
            }
        }


        private void UpdateDisplay()
        {
            if (!GS.IsDamageShow)
            {
                return; // Не показываем UI, если выключено
            }

            if (ModDisplay.Instance == null)
            {
                CreateUI(); // Пересоздаём, если был уничтожен
            }

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Сцены где таймер не идёт (годхоум)
            string[] safeScenes = new string[]
            {
        "GG_Atrium",
        "GG_Atrium_Roof",
        "GG_Blue_Room",
        "GG_Workshop"
            };
            // переменные массивчики и т.д и т.п
            bool isInSafeZone = Array.Exists(safeScenes, scene => scene == currentScene);
            bool isBossRush = PlayerData.instance.GetBool("bossRushMode");

            string displayText = $"Damage Multiplier: {GS.DamageMultiplier}\n";

            if (isInSafeZone && isBossRush)
            {
                displayText += "Timer Paused (Safe Zone)";
            }
            else
            {
                if (!GS.EnableTimeDamage)
                {
                    displayText += "Time Damage: Off";
                }
                else
                {
                    int secondsLeft = (int)(GS.TimeBetweenDamage - (damageTimer % GS.TimeBetweenDamage));
                    displayText += $"Next damage in: {secondsLeft}s";
                }
            }

            ModDisplay.Instance?.Display(displayText);
        }
    }
}