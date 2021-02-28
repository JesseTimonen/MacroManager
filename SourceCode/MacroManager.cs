﻿using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MacroManager
{
    public partial class MacroManagerForm : Form
    {
        private bool hotkeysEnabled = false;
        private bool isPlayingMacro = false;
        private bool isRecordingMacro = false;
        private bool autoClickerEnabled = false;
        private bool autoClickerDelayMode = false;
        private int macroDuration;
        private int hotkeyTimerStep;
        private int hotkeyIDCounter;
        private int macroRepeatCounter;
        private int AutoClickerDelayCounter;
        private int[] mouseLastPosition;
        private static int timerStep;
        private static string newRecordedMacroInstructions;
        private static string placeholderNewRecordedMacroInstructions;
        private int[] hotkeyInstructions;
        private string deletedObject;
        private int deletedIndex;
        private Dictionary<int, int[]> macroInstructions = new Dictionary<int, int[]>();
        private Dictionary<string, string> settings = new Dictionary<string, string>();
        private List<Hotkey> savedHotkeys = new List<Hotkey>();
        private List<Macro> savedMacros = new List<Macro>();
        private List<Label> macroNameLabels = new List<Label>();
        private List<Label> macroHotkeyLabels = new List<Label>();
        private List<Label> macroDelayLabels = new List<Label>();
        private List<Label> macroRepeatLabels = new List<Label>();
        private List<TextBox> macroNameTextboxes = new List<TextBox>();
        private List<TextBox> macroHotkeyTextboxes = new List<TextBox>();
        private List<TextBox> macroDelayTextboxes = new List<TextBox>();
        private List<TextBox> macroRepeatTextboxes = new List<TextBox>();
        private List<Button> playMacroButtons = new List<Button>();
        private List<Button> deleteMacroButtons = new List<Button>();
        private List<Label> hotkeyNameLabels = new List<Label>();
        private List<Label> hotkeyInputLabels = new List<Label>();
        private List<TextBox> hotkeyNameTextboxes = new List<TextBox>();
        private List<TextBox> hotkeyInputTextboxes = new List<TextBox>();
        private List<Button> deleteHotkeyButtons = new List<Button>();
        private List<KeyHandler> keyHandlers = new List<KeyHandler>();
        private KeyHandler newHotkeyKeyHandler;
        private KeyHandler startMacroKeyHandler;
        private KeyHandler stopMacroKeyHandler;
        private KeyHandler autoClickerKeyHandler;

        public struct Hotkey
        {
            public string Name;
            public string Input;
            public int X;
            public int Y;

            public Hotkey(string name, string input, int x, int y)
            {
                Name = name;
                Input = input;
                X = x;
                Y = y;
            }
        }

        public struct Macro
        {
            public string Name;
            public string Instructions;
            public string Hotkey;
            public int Delay;
            public int RepeatFor;
            public int Duration;

            public Macro(string name, string instructions, string hotkey, int delay, int repeatFor, int duration)
            {
                Name = name;
                Instructions = instructions;
                Hotkey = hotkey;
                Delay = delay;
                RepeatFor = repeatFor;
                Duration = duration;
            }
        }



        // ============================================== General ============================================== \\
        public MacroManagerForm()
        {
            InitializeComponent();
        }

        private void MacroManagerForm_Load(object sender, EventArgs e)
        {
            InitSave();
            LoadData();
            UpdateTimerInterval();
            CreateMacroPlaceholders();
            CreateHotkeyPlaceholders();
            UpdateMacrosUI();
            UpdateHotkeysUI();

            startMacroKeyHandler = new KeyHandler(Keys.F1, 10001, this);
            stopMacroKeyHandler = new KeyHandler(Keys.F2, 10002, this);
            newHotkeyKeyHandler = new KeyHandler(Keys.F3, 10003, this);
            autoClickerKeyHandler = new KeyHandler(Keys.F5, 10004, this);
            startMacroKeyHandler.Register();
            stopMacroKeyHandler.Register();
            newHotkeyKeyHandler.Register();
            autoClickerKeyHandler.Register();
        }

        private void MacroManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DeactivateMouseHook();

            foreach (KeyHandler keyHandler in keyHandlers)
            {
                keyHandler.Unregister();
            }

            startMacroKeyHandler.Unregister();
            stopMacroKeyHandler.Unregister();
            newHotkeyKeyHandler.Unregister();
            autoClickerKeyHandler.Unregister();
        }

        private void MinimizeApplicationButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void CloseApplicationButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // ============================================ End General ============================================ \\



        // =========================================== UI Navigation =========================================== \\
        private void MacroToHotkeysButton_Click(object sender, EventArgs e)
        {
            MacrosPanel.Visible = false;
            HotkeysPanel.Visible = true;
            MacrosSavedFeedbackLabel.Visible = false;
        }

        private void MacroToAutoClickerButton_Click(object sender, EventArgs e)
        {
            MacrosPanel.Visible = false;
            AutoClickerPanel.Visible = true;
            MacrosSavedFeedbackLabel.Visible = false;
        }

        private void MacroToSettingsButton_Click(object sender, EventArgs e)
        {
            MacrosPanel.Visible = false;
            SettingsPanel.Visible = true;
            MacrosSavedFeedbackLabel.Visible = false;

            SettingsClickWhileMovingCheckbox.Checked = bool.Parse(settings["ClickWhileMoving"]);

            if (settings["EngineSpeed"] == "fast")
            {
                SettingsSafeModeRadioButton.Checked = false;
                SettingsFastModeRadioButton.Checked = true;
            }
            else
            {
                SettingsFastModeRadioButton.Checked = false;
                SettingsSafeModeRadioButton.Checked = true;
            }
        }

        private void HotkeysToMacroButton_Click(object sender, EventArgs e)
        {
            HotkeysPanel.Visible = false;
            MacrosPanel.Visible = true;
            HotkeysSavedFeedbackLabel.Visible = false;
        }

        private void HotkeyToAutoClickerButton_Click(object sender, EventArgs e)
        {
            HotkeysPanel.Visible = false;
            AutoClickerPanel.Visible = true;
            HotkeysSavedFeedbackLabel.Visible = false;
        }

        private void HotkeysToSettingsButton_Click(object sender, EventArgs e)
        {
            HotkeysPanel.Visible = false;
            SettingsPanel.Visible = true;
            HotkeysSavedFeedbackLabel.Visible = false;

            SettingsClickWhileMovingCheckbox.Checked = bool.Parse(settings["ClickWhileMoving"]);

            if (settings["EngineSpeed"] == "fast")
            {
                SettingsSafeModeRadioButton.Checked = false;
                SettingsFastModeRadioButton.Checked = true;
            }
            else
            {
                SettingsFastModeRadioButton.Checked = false;
                SettingsSafeModeRadioButton.Checked = true;
            }
        }

        private void AutoClickerToMacroButton_Click(object sender, EventArgs e)
        {
            AutoClickerPanel.Visible = false;
            MacrosPanel.Visible = true;
        }

        private void AutoClickerToHotkeysButton_Click(object sender, EventArgs e)
        {
            AutoClickerPanel.Visible = false;
            HotkeysPanel.Visible = true;
        }

        private void AutoClickerToSettingsButton_Click(object sender, EventArgs e)
        {
            AutoClickerPanel.Visible = false;
            SettingsPanel.Visible = true;

            SettingsClickWhileMovingCheckbox.Checked = bool.Parse(settings["ClickWhileMoving"]);

            if (settings["EngineSpeed"] == "fast")
            {
                SettingsSafeModeRadioButton.Checked = false;
                SettingsFastModeRadioButton.Checked = true;
            }
            else
            {
                SettingsFastModeRadioButton.Checked = false;
                SettingsSafeModeRadioButton.Checked = true;
            }
        }

        private void SettingsToMacroButton_Click(object sender, EventArgs e)
        {
            SettingsPanel.Visible = false;
            MacrosPanel.Visible = true;
            SettingsSavedFeedbackLabel.Visible = false;
        }

        private void SettingsToHotkeysButton_Click(object sender, EventArgs e)
        {
            SettingsPanel.Visible = false;
            HotkeysPanel.Visible = true;
            SettingsSavedFeedbackLabel.Visible = false;
        }

        private void SettingsToAutoClickerButton_Click(object sender, EventArgs e)
        {
            SettingsPanel.Visible = false;
            AutoClickerPanel.Visible = true;
            SettingsSavedFeedbackLabel.Visible = false;
        }
        // ========================================= End UI Navigation ========================================= \\



        // ============================================== Hotkeys ============================================== \\
        private void toggleHotkeysButton_Click(object sender, EventArgs e)
        {
            hotkeysEnabled = !hotkeysEnabled;

            if (hotkeysEnabled)
            {
                EnableHotkeysUIButtons(false);
                ToggleHotkeysButton.Text = "Disable Hotkeys";
                HotkeysStatusLabel.Text = "ON";
                HotkeysStatusLabel.ForeColor = Color.Green;
                ToggleHotkeysButton.FlatAppearance.BorderColor = Color.Green;
                ToggleHotkeysButton.FlatAppearance.BorderSize = 2;

                hotkeyIDCounter = 0;
                foreach (Hotkey savedHotkey in savedHotkeys)
                {
                    keyHandlers.Add(new KeyHandler(KeyHandler.ConvertToKey(hotkeyInputTextboxes[hotkeyIDCounter].Text), hotkeyIDCounter, this));
                    hotkeyIDCounter++;
                }

                hotkeyIDCounter = 1000;
                foreach (Macro savedMacro in savedMacros)
                {
                    keyHandlers.Add(new KeyHandler(KeyHandler.ConvertToKey(macroHotkeyTextboxes[hotkeyIDCounter - 1000].Text), hotkeyIDCounter, this));
                    hotkeyIDCounter++;
                }

                foreach (KeyHandler keyHandler in keyHandlers)
                {
                    keyHandler.Register();
                }
            }
            else
            {
                EnableHotkeysUIButtons(true);
                ToggleHotkeysButton.Text = "Enable Hotkeys";
                HotkeysStatusLabel.Text = "OFF";
                HotkeysStatusLabel.ForeColor = Color.Red;
                ToggleHotkeysButton.FlatAppearance.BorderColor = Color.Red;
                ToggleHotkeysButton.FlatAppearance.BorderSize = 1;

                foreach (KeyHandler keyHandler in keyHandlers)
                {
                    keyHandler.Unregister();
                }

                keyHandlers.Clear();
            }
        }

        private void SaveHotkeysButton_Click(object sender, EventArgs e)
        {
            if (savedHotkeys.Count == 0) { return; }

            int index = 0;
            List<Hotkey> _savedHotkey = new List<Hotkey>();

            foreach (Hotkey savedHotkey in savedHotkeys)
            {
                _savedHotkey.Add(new Hotkey(hotkeyNameTextboxes[index].Text, hotkeyInputTextboxes[index].Text, savedHotkey.X, savedHotkey.Y));
                index++;
            }

            savedHotkeys.Clear();
            savedHotkeys = _savedHotkey;
            SaveHotkeys();
            UpdateHotkeysUI();
        }

        private void EnableHotkeysUIButtons(bool enable)
        {
            SaveHotkeysButton.Enabled = enable;

            foreach (var deleteHotkeyButton in deleteHotkeyButtons)
            {
                deleteHotkeyButton.Enabled = enable;
            }
            foreach (var hotkeyNameTextbox in hotkeyNameTextboxes)
            {
                hotkeyNameTextbox.Enabled = enable;
            }
            foreach (var hotkeyInputTextbox in hotkeyInputTextboxes)
            {
                hotkeyInputTextbox.Enabled = enable;
            }
            foreach (var macroHotkeyTextbox in macroHotkeyTextboxes)
            {
                macroHotkeyTextbox.Enabled = enable;
            }
        }
        // ============================================ End Hotkeys ============================================ \\



        // ============================================== Macros =============================================== \\
        private void NewMacroButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (isPlayingMacro) { stopMacroReplay(); }
            else
            {
                isRecordingMacro = !isRecordingMacro;
                if (isRecordingMacro) { startMacroRecording(); }
                else { stopMacroRecording(); }
            }
        }

        private void startMacroRecording()
        {
            isRecordingMacro = true;
            EnableMacroUIButtons(false);
            placeholderNewRecordedMacroInstructions = "";
            newRecordedMacroInstructions = "";
            NewMacroButton.Text = "Stop Recording (Hotkey: F2)";
            MacroStatusLabel.Visible = false;
            MacroStatusLabel.Text = "Recoding new Macro";
            MacroStatusLabel.Visible = true;
            MacroStatusLabel.ForeColor = Color.Green;
            StartTimer();
            ActivateMouseHook();
        }

        private void stopMacroRecording()
        {
            isRecordingMacro = false;
            DeactivateMouseHook();
            StopTimer();
            EnableMacroUIButtons(true);
            NewMacroButton.Text = "Record New Macro (Hotkey: F1)";
            MacroStatusLabel.Visible = false;
            MacroStatusLabel.Text = "OFF";
            MacroStatusLabel.Visible = true;
            MacroStatusLabel.ForeColor = Color.Red;

            if (newRecordedMacroInstructions != "")
            {
                CreateNewMacroPlaceholder();
                savedMacros.Add(new Macro("New macro", newRecordedMacroInstructions, "none", 0, 99999, timerStep));
                SaveMacros();
                UpdateMacrosUI();
                if (hotkeysEnabled) { macroHotkeyTextboxes[savedMacros.Count - 1].Enabled = false; }
            }
        }

        private void stopMacroReplay()
        {
            StopTimer();
            EnableMacroUIButtons(true);
            isPlayingMacro = false;
            macroInstructions.Clear();
            NewMacroButton.Text = "Record New Macro (Hotkey: F1)";
            MacroStatusLabel.Visible = false;
            MacroStatusLabel.Text = "OFF";
            MacroStatusLabel.Visible = true;
            MacroStatusLabel.ForeColor = Color.Red;
        }

        private void SaveMacrosButton_Click(object sender, EventArgs e)
        {
            if (savedMacros.Count == 0) { return; }

            int index = 0;
            List<Macro> _savedMacros = new List<Macro>();

            foreach (Macro savedMacro in savedMacros)
            {
                _savedMacros.Add(new Macro(macroNameTextboxes[index].Text, savedMacro.Instructions, macroHotkeyTextboxes[index].Text, int.Parse(macroDelayTextboxes[index].Text), int.Parse(macroRepeatTextboxes[index].Text), savedMacro.Duration));
                index++;
            }
   
            savedMacros.Clear();
            savedMacros = _savedMacros;
            SaveMacros();
            UpdateMacrosUI();
        }

        private void EnableMacroUIButtons(bool enable)
        {
            if (enable == false)
            {
                SettingsPanel.Visible = false;
                HotkeysPanel.Visible = false;
                AutoClickerPanel.Visible = false;
                MacrosPanel.Visible = true;

                foreach (var macroHotkeyTextbox in macroHotkeyTextboxes)
                {
                    macroHotkeyTextbox.Enabled = false;
                }
            }
            else
            {
                if (!hotkeysEnabled)
                {
                    foreach (var macroHotkeyTextbox in macroHotkeyTextboxes)
                    {
                        macroHotkeyTextbox.Enabled = true;
                    }
                }
            }

            MacroToHotkeysButton.Enabled = enable;
            MacroToAutoClickerButton.Enabled = enable;
            MacroToSettingsButton.Enabled = enable;
            SaveMacrosButton.Enabled = enable;

            foreach (var playMacroButton in playMacroButtons)
            {
                playMacroButton.Enabled = enable;
            }
            foreach (var deleteMacroButton in deleteMacroButtons)
            {
                deleteMacroButton.Enabled = enable;
            }
            foreach (var macroNameTextbox in macroNameTextboxes)
            {
                macroNameTextbox.Enabled = enable;
            }
            foreach (var macroDelayTextbox in macroDelayTextboxes)
            {
                macroDelayTextbox.Enabled = enable;
            }
            foreach (var macroRepeatTextbox in macroRepeatTextboxes)
            {
                macroRepeatTextbox.Enabled = enable;
            }
        }
        // ============================================ End Macros ============================================= \\



        // ============================================ Auto Clicker =========================================== \\
        private void AutoClickerModeButton_Click(object sender, EventArgs e)
        {
            autoClickerDelayMode = !autoClickerDelayMode;

            if (autoClickerDelayMode)
            {
                AutoClickerModeLabel.Text = "Time between clicks (seconds)";
            }
            else
            {
                AutoClickerModeLabel.Text = "Clicks per second";
            }
        }

        private void AutoClickerInputTextBox_Validated(object sender, EventArgs e)
        {
            if (!AutoClickerInputTextBox.Text.All(char.IsDigit) || AutoClickerInputTextBox.Text == "")
            {
                AutoClickerInputTextBox.Text = "0";
            }

            if (int.Parse(AutoClickerInputTextBox.Text) > 60)
            {
                AutoClickerInputTextBox.Text = "60";
            }
        }

        private void ToggleAutoClickerButton_Click(object sender, EventArgs e)
        {
            autoClickerEnabled = !autoClickerEnabled;

            if (autoClickerEnabled)
            {
                AutoClickerInputTextBox_Validated(null, null);
                if (int.Parse(AutoClickerInputTextBox.Text) == 0) { return;  }
                StartAutoClicker();
            }
            else
            {
                StopAutoClicker();
            }
        }

        private void StartAutoClicker()
        {
            EnableAutoClickerUIButtons(false);
            AutoClickerStatusLabel.Text = "ON";
            AutoClickerStatusLabel.ForeColor = Color.Green;
            ToggleAutoClickerButton.FlatAppearance.BorderColor = Color.Green;
            ToggleAutoClickerButton.FlatAppearance.BorderSize = 2;

            if (autoClickerDelayMode)
            {
                AutoClickerTimer.Interval = (int)Math.Ceiling(1000f * float.Parse(AutoClickerInputTextBox.Text));
            }
            else
            {
                AutoClickerTimer.Interval = (int)Math.Ceiling(1000f / float.Parse(AutoClickerInputTextBox.Text));
            }

            mouseLastPosition = new int[] { Cursor.Position.X, Cursor.Position.Y };
            if (!autoClickerDelayMode) { AutoClickerDelayCounter = (int)Math.Ceiling(1000f / AutoClickerTimer.Interval); }
            AutoClickerTimer.Start();
        }

        private void StopAutoClicker()
        {
            EnableAutoClickerUIButtons(true);
            AutoClickerStatusLabel.Text = "OFF";
            AutoClickerStatusLabel.ForeColor = Color.Red;
            ToggleAutoClickerButton.FlatAppearance.BorderColor = Color.Red;
            ToggleAutoClickerButton.FlatAppearance.BorderSize = 1;
            AutoClickerTimer.Stop();
        }

        private void EnableAutoClickerUIButtons(bool enable)
        {
            if (enable == false)
            {
                SettingsPanel.Visible = false;
                HotkeysPanel.Visible = false;
                MacrosPanel.Visible = false;
                AutoClickerPanel.Visible = true;
            }

            AutoClickerToMacroButton.Enabled = enable;
            AutoClickerToHotkeysButton.Enabled = enable;
            AutoClickerToSettingsButton.Enabled = enable;
            SaveMacrosButton.Enabled = enable;
            AutoClickerModeButton.Enabled = enable;
            AutoClickerInputTextBox.Enabled = enable;
        }
        // ========================================== End Auto Clicker ========================================= \\



        // =============================================== Timers ============================================== \\
        private void StartTimer()
        {
            timerStep = 0;
            Timer.Start();
        }

        private void StopTimer()
        {
            Timer.Stop();
        }

        private void StartHotkeyTimer()
        {
            hotkeyTimerStep = 0;
            HotkeyTimer.Start();
        }

        private void StopHotkeyTimer()
        {
            HotkeyTimer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isPlayingMacro)
            {
                if (timerStep >= macroDuration)
                {
                    macroRepeatCounter--;
                    timerStep = 0;

                    if (macroRepeatCounter <= 0)
                    {
                        stopMacroReplay();
                        return;
                    }
                }

                if (macroInstructions.ContainsKey(timerStep + 1))
                {
                    if (!macroInstructions.ContainsKey(timerStep) && !macroInstructions.ContainsKey(timerStep - 1))
                    {
                        mouseLastPosition = new int[] { Cursor.Position.X, Cursor.Position.Y };
                    }
                }

                if (macroInstructions.ContainsKey(timerStep))
                {
                    if (macroInstructions.ContainsKey(timerStep - 1))
                    {
                        MoveMouseToPosition(macroInstructions[timerStep][0], macroInstructions[timerStep][1]);
                    }
                    LeftMouseClick(macroInstructions[timerStep][0], macroInstructions[timerStep][1]);
                }

                if (macroInstructions.ContainsKey(timerStep + 1))
                {
                    MoveMouseToPosition(macroInstructions[timerStep + 1][0], macroInstructions[timerStep + 1][1]);
                }

                if (macroInstructions.ContainsKey(timerStep - 1))
                {
                    if (!macroInstructions.ContainsKey(timerStep) && !macroInstructions.ContainsKey(timerStep + 1))
                    {
                        MoveMouseToPosition(mouseLastPosition[0], mouseLastPosition[1]);
                    }
                }
            }

            timerStep++;
        }

        private void HotkeyTimer_Tick(object sender, EventArgs e)
        {
            if (hotkeyTimerStep == 0)
            {
                mouseLastPosition = new int[] { Cursor.Position.X, Cursor.Position.Y };
                MoveMouseToPosition(hotkeyInstructions[0], hotkeyInstructions[1]);
            }
            else if (hotkeyTimerStep == 1)
            {
                LeftMouseClick(hotkeyInstructions[0], hotkeyInstructions[1]);
            }
            else if (hotkeyTimerStep >= 2)
            {
                MoveMouseToPosition(mouseLastPosition[0], mouseLastPosition[1]);
                StopHotkeyTimer();
            }

            hotkeyTimerStep++;
        }

        private void AutoClickerTimer_Tick(object sender, EventArgs e)
        {
            if (bool.Parse(settings["ClickWhileMoving"]))
            {
                if (AutoClickerDelayCounter <= 0)
                {
                    LeftMouseClick(Cursor.Position.X, Cursor.Position.Y);
                }
                else
                {
                    AutoClickerDelayCounter--;
                }
            }
            else if (!autoClickerDelayMode)
            {
                AutoClickerDelayCounter--;

                if (AutoClickerDelayCounter <= 0 && mouseLastPosition[0] == Cursor.Position.X && mouseLastPosition[1] == Cursor.Position.Y)
                {
                    LeftMouseClick(Cursor.Position.X, Cursor.Position.Y);
                }
                else
                {
                    if (AutoClickerDelayCounter <= 0)
                    {
                        mouseLastPosition = new int[] { Cursor.Position.X, Cursor.Position.Y };
                        AutoClickerDelayCounter = (int)Math.Ceiling(1000f / AutoClickerTimer.Interval);
                    }
                }
            }
            else
            {
                if (AutoClickerTimer.Interval == 999)
                {
                    if (mouseLastPosition[0] == Cursor.Position.X && mouseLastPosition[1] == Cursor.Position.Y)
                    {
                        LeftMouseClick(Cursor.Position.X, Cursor.Position.Y);

                        if (AutoClickerInputTextBox.Text == "1")
                        {
                            AutoClickerTimer.Interval = 999;
                        }
                        else
                        {
                            AutoClickerTimer.Interval = (int)Math.Ceiling(1000 * (float.Parse(AutoClickerInputTextBox.Text) - 1));
                        }
                    }
                    else
                    {
                        mouseLastPosition = new int[] { Cursor.Position.X, Cursor.Position.Y };
                    }
                }
                else
                {
                    mouseLastPosition = new int[] { Cursor.Position.X, Cursor.Position.Y };
                    AutoClickerTimer.Interval = 999;
                }
            }
        }
        // ============================================= End Timers ============================================ \\



        // ============================================== Settings ============================================= \\
        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            if (settings.ContainsKey("EngineSpeed"))
            {
                settings.Remove("EngineSpeed");
            }
            if (settings.ContainsKey("ClickWhileMoving"))
            {
                settings.Remove("ClickWhileMoving");
            }

            if (SettingsFastModeRadioButton.Checked)
            {
                settings.Add("EngineSpeed", "fast");
            }
            else
            {
                settings.Add("EngineSpeed", "safe");
            }

            settings.Add("ClickWhileMoving", SettingsClickWhileMovingCheckbox.Checked.ToString());

            SaveSettings();
            LoadData();
            UpdateTimerInterval();
            foreach (var macroNameLabel in macroNameLabels) { macroNameLabel.Dispose(); }
            foreach (var macroHotkeyLabel in macroHotkeyLabels) { macroHotkeyLabel.Dispose(); }
            foreach (var macroDelayLabel in macroDelayLabels) { macroDelayLabel.Dispose(); }
            foreach (var macroRepeatLabel in macroRepeatLabels) { macroRepeatLabel.Dispose(); }
            foreach (var macroNameTextbox in macroNameTextboxes) { macroNameTextbox.Dispose(); }
            foreach (var macroHotkeyTextbox in macroHotkeyTextboxes) { macroHotkeyTextbox.Dispose(); }
            foreach (var macroDelayTextbox in macroDelayTextboxes) { macroDelayTextbox.Dispose(); }
            foreach (var macroRepeatTextbox in macroRepeatTextboxes) { macroRepeatTextbox.Dispose(); }
            foreach (var playMacroButton in playMacroButtons) { playMacroButton.Dispose(); }
            foreach (var deleteMacroButton in deleteMacroButtons) { deleteMacroButton.Dispose(); }
            macroNameLabels.Clear();
            macroHotkeyLabels.Clear();
            macroDelayLabels.Clear();
            macroRepeatLabels.Clear();
            macroNameTextboxes.Clear();
            macroHotkeyTextboxes.Clear();
            macroDelayTextboxes.Clear();
            macroRepeatTextboxes.Clear();
            playMacroButtons.Clear();
            deleteMacroButtons.Clear();
            CreateMacroPlaceholders();
            UpdateMacrosUI();
        }

        private void UpdateTimerInterval()
        {
            if (settings.ContainsKey("EngineSpeed"))
            {
                if (settings["EngineSpeed"] == "fast")
                {
                    Timer.Interval = 20;
                    HotkeyTimer.Interval = 20;
                }
                else
                {
                    Timer.Interval = 50;
                    HotkeyTimer.Interval = 50;
                }
            }
            else
            {
                Timer.Interval = 50;
                HotkeyTimer.Interval = 50;
            }
        }
        // ============================================ End Settings =========================================== \\



        // ========================================== Keyboard hotkeys ========================================= \\
        // Check for keyboard inputs
        protected override void WndProc(ref Message m)
        {
            if (ConfirmDeletePanel.Visible) { return; }
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();

                // Start macro hotkey
                if (id == 10001)
                {
                    if (!isPlayingMacro && !isRecordingMacro && !autoClickerEnabled) { startMacroRecording(); }
                }
                // Stop macro hotkey
                else if (id == 10002)
                {
                    if (isPlayingMacro) { stopMacroReplay(); }
                    else if (isRecordingMacro) { stopMacroRecording(); }
                }
                // "Create hotkey" -hotkey
                else if (id == 10003)
                {
                    if (!hotkeysEnabled)
                    {
                        CreateNewHotkeyPlaceholder();
                        savedHotkeys.Add(new Hotkey("New Hotkey", "none", Cursor.Position.X, Cursor.Position.Y));
                        SaveHotkeys();
                        UpdateHotkeysUI();
                    }
                }
                // Auto clicker hotkey
                else if (id == 10004)
                {
                    if (!isPlayingMacro && !isRecordingMacro)
                    {
                        ToggleAutoClickerButton_Click(null, null);
                    }
                }
                else
                {
                    // Macro hotkeys
                    if (id >= 1000)
                    {
                        if (!isPlayingMacro && !isRecordingMacro && !autoClickerEnabled)
                        {
                            id -= 1000;
                            PlayMacro(id);
                        }
                        else if (isPlayingMacro)
                        {
                            stopMacroReplay();
                        }
                    }
                    // Normal hotkeys
                    else
                    {
                        hotkeyInstructions = new int[] { savedHotkeys[id].X, savedHotkeys[id].Y };
                        StartHotkeyTimer();
                    }
                }
            }
            base.WndProc(ref m);
        }
        // ======================================== End Keyboard hotkeys ======================================= \\
    }
}