using System;
using Modding;
using UnityEngine;

namespace ConfigurableMaskDamage
{
    public class Settings : IGlobalSettings<SettingsData>
    {
        // Поле для хранения данных
        private SettingsData _data = new SettingsData();

        // Реализация интерфейса IGlobalSettings<T>
        public void OnLoadGlobal(SettingsData settings)
        {
            _data = settings; // Сохраняем данные при загрузке
        }

        public SettingsData OnSaveGlobal()
        {
            return _data; // Возвращаем данные при сохранении
        }

        // Геттеры/сеттеры для доступа к данным
        public int DamageMultiplier
        {
            get => _data.DamageMultiplier;
            set => _data.DamageMultiplier = value;
        }

        public bool IsDamageShow
        {
            get => _data.IsDamageShow;
            set => _data.IsDamageShow = value;
        }

        public KeyCode KeyIncrease
        {
            get
            {
                try
                {
                    return (KeyCode)Enum.Parse(typeof(KeyCode), _data.KeyIncrease);
                }
                catch (Exception)
                {
                    return KeyCode.O; // Значение по умолчанию
                }
            }
            set => _data.KeyIncrease = value.ToString();
        }

        public KeyCode KeyDecrease
        {
            get
            {
                try
                {
                    return (KeyCode)Enum.Parse(typeof(KeyCode), _data.KeyDecrease);
                }
                catch (Exception)
                {
                    return KeyCode.I; // Значение по умолчанию
                }
            }
            set => _data.KeyDecrease = value.ToString();
        }

        public KeyCode KeyToggle
        {
            get
            {
                try
                {
                    return (KeyCode)Enum.Parse(typeof(KeyCode), _data.KeyToggle);
                }
                catch (Exception)
                {
                    return KeyCode.P; // Значение по умолчанию
                }
            }
            set => _data.KeyToggle = value.ToString();
        }
    }

    // Структура данных настроек
    [Serializable]
    public class SettingsData
    {
        public int DamageMultiplier = 1;
        public bool IsDamageShow = true;
        public string KeyIncrease = "O";
        public string KeyDecrease = "I";
        public string KeyToggle = "P";
    }
}