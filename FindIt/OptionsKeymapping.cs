// By algernon via Realistic Population Revisited, based on analysis and refactoring of game code by Colossal Order.
// Further modified by sway. If there is anything wrong blame it on me.

using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace FindIt
{
    public class OptionsKeymapping : UICustomControl
    {
        // Components.
        UILabel label;
        UIButton button;

        // State flag.
        private bool isPrimed = false;

        private UnsavedInputKey unsavedInputKey;

        protected string labelString;

        /// <summary>
        /// Setup this control
        /// Called by Unity immediately before the first update.
        /// </summary>
        public void Start()
        {
            // Get the template from the game and attach it here.
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

            // Find our sub-components.
            label = uIPanel.Find<UILabel>("Name");
            button = uIPanel.Find<UIButton>("Binding");

            // Attach our event handlers.
            button.eventKeyDown += (control, keyEvent) => OnKeyDown(keyEvent);
            button.eventMouseDown += (control, mouseEvent) => OnMouseDown(mouseEvent);

            // Set label and button text.
            label.text = labelString;
            button.text = unsavedInputKey.ToLocalizedString("KEYNAME");
            button.eventVisibilityChanged += ButtonVisibilityChanged;
        }

        private void ButtonVisibilityChanged(UIComponent component, bool isVisible)
        {
            if (isVisible)
            {
                button.text = unsavedInputKey.ToLocalizedString("KEYNAME");
            }
        }


        /// <summary>
        /// KeyDown event handler to record the new hotkey.
        /// </summary>
        /// <param name="keyEvent">Keypress event parameter</param>
        public void OnKeyDown(UIKeyEventParameter keyEvent)
        {
            // Debugging.Message("keydown " + isPrimed);

            // Only do this if we're primed and the keypress isn't a modifier key.
            if (isPrimed && !IsModifierKey(keyEvent.keycode))
            {
                // Variables.
                InputKey inputKey;

                // Use the event.
                keyEvent.Use();

                // If escape was entered, we don't change the code.
                if (keyEvent.keycode == KeyCode.Escape)
                {
                    inputKey = unsavedInputKey.value;
                }
                else
                {
                    // If backspace was pressed, then we blank the input; otherwise, encode the keypress.
                    inputKey = (keyEvent.keycode == KeyCode.Backspace) ? SavedInputKey.Empty : SavedInputKey.Encode(keyEvent.keycode, keyEvent.control, keyEvent.shift, keyEvent.alt);
                }

                // Apply our new key.
                ApplyKey(inputKey);
            }
        }


        /// <summary>
        /// MouseDown event handler to handle mouse clicks; primarily used to prime hotkey entry.
        /// </summary>
        /// <param name="mouseEvent">Mouse button event parameter</param>
        public void OnMouseDown(UIMouseEventParameter mouseEvent)
        {
            // Use the event.
            mouseEvent.Use();

            // Check to see if we're already primed for hotkey entry.
            if (isPrimed)
            {
                // We were already primed; is this a bindable mouse button?
                if (mouseEvent.buttons == UIMouseButton.Left || mouseEvent.buttons == UIMouseButton.Right)
                {
                    // Not a bindable mouse button - set the button text and cancel priming.
                    button.text = unsavedInputKey.ToLocalizedString("KEYNAME");
                    UIView.PopModal();
                    isPrimed = false;
                }
                else
                {
                    // Bindable mouse button - do keybinding as if this was a keystroke.
                    KeyCode mouseCode;

                    switch (mouseEvent.buttons)
                    {
                        // Convert buttons to keycodes (we don't bother with left and right buttons as they're non-bindable).
                        case UIMouseButton.Middle:
                            mouseCode = KeyCode.Mouse2;
                            break;
                        case UIMouseButton.Special0:
                            mouseCode = KeyCode.Mouse3;
                            break;
                        case UIMouseButton.Special1:
                            mouseCode = KeyCode.Mouse4;
                            break;
                        case UIMouseButton.Special2:
                            mouseCode = KeyCode.Mouse5;
                            break;
                        case UIMouseButton.Special3:
                            mouseCode = KeyCode.Mouse6;
                            break;
                        default:
                            // No valid button pressed: exit without doing anything.
                            return;
                    }

                    // We got a valid mouse button key - apply settings and save.
                    ApplyKey(SavedInputKey.Encode(mouseCode, IsControlDown(), IsShiftDown(), IsAltDown()));
                }
            }
            else
            {
                // We weren't already primed - set our text and focus the button.
                button.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                button.text = "ESC->cancel. Backspace->delete";
                button.Focus();

                // Prime for new keybinding entry.
                isPrimed = true;
                UIView.PushModal(button);
            }
        }

        public void Init(UnsavedInputKey keybind)
        {
            unsavedInputKey = keybind;
            labelString = Translations.Translate(keybind.name);
        }

        /// <summary>
        /// Applies a valid key to our settings.
        /// </summary>
        /// <param name="key">InputKey to apply</param>
        private void ApplyKey(InputKey key)
        {
            // Apply key to current settings and save.
            unsavedInputKey.value = key;
            XMLUtils.SaveSettings();

            // Set the label for the new hotkey.
            button.text = SavedInputKey.ToLocalizedString("KEYNAME", key);

            // Remove priming.
            UIView.PopModal();
            isPrimed = false;
        }


        /// <summary>
        /// Checks to see if the given keycode is a modifier key.
        /// </summary>
        /// <param name="code">Leycode to check</param>
        /// <returns>True if the key is a modifier key, false otherwise</returns>
        private bool IsModifierKey(KeyCode keyCode)
        {
            return (keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl || keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift || keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt || keyCode == KeyCode.AltGr);
        }


        /// <summary>
        /// Checks to see if the control key is pressed.
        /// </summary>
        /// <returns>True if the control key is down, false otherwise.</returns>
        private bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }


        /// <summary>
        /// Checks to see if the shift key is pressed.
        /// </summary>
        /// <returns>True if the shift key is down, false otherwise.</returns>
        private bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }


        /// <summary>
        /// Checks to see if the alt key is pressed.
        /// </summary>
        /// <returns>True if the alt key is down, false otherwise.</returns>
        private bool IsAltDown()
        {
            // Don't worry, Alt.Gr, I still remember you, even if everyone else forgets!
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
        }
    }
}