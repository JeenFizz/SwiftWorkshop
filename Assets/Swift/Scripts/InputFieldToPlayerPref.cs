using UnityEngine;
using UnityEngine.UI;


using Photon.Realtime;
using System.Collections;


namespace WS3
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class InputFieldToPlayerPref : MonoBehaviour
    {
        #region Private Constants


        // Store the PlayerPref Key to avoid typos
        public string PlayerPrefKey;


        #endregion


        #region MonoBehaviour CallBacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            updateFieldWithPlayerPrefValue();
        }


        #endregion


        #region Public Methods
        private void updateFieldWithPlayerPrefValue()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefKey)) return;

            string defaultValue = string.Empty;
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField != null)
            {
                defaultValue = PlayerPrefs.GetString(PlayerPrefKey);
                _inputField.text = defaultValue;
            }
        }

        /// <summary>
        /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SaveValueToPlayerPref(string value)
        {
            if (string.IsNullOrWhiteSpace(PlayerPrefKey)) return;
            PlayerPrefs.SetString(PlayerPrefKey, value);
        }


        #endregion
    }
}