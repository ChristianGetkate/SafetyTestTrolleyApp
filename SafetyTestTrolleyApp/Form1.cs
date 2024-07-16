using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwinCAT.Ads;
using LedSingleLibrary;

namespace SafetyTestTrolleyApp
{
    public partial class Form1 : Form
    {
        private TwinCATConnector connector = new TwinCATConnector();
        private Dictionary<string, uint> VariableHandledata = new();
        private List<LedSingleControl> leds = new List<LedSingleControl>();
        private Dictionary<string, bool> ledStates = new();
        private SynchronizationContext context;

        public Form1()
        {
            InitializeComponent();
            connector.Connect(AmsNetId.Local.ToString(), 851);

            context = SynchronizationContext.Current;

            // Add ESB buttons
            int esbCount = 8;
            AddButtonsAndTextBoxes("ESB", "EmergencyStopButtons_TS", 20, 15, esbCount, Color.LightSalmon);

            // Add LC buttons
            int lcCount = 8;
            AddButtonsAndTextBoxes("LC", "Lightcurtains_TS", 180, 15, lcCount, Color.LightYellow);

            // Add IG buttons
            int igCount = 4;
            AddButtonsAndTextBoxes("IG", "InterlockingGuards_TS", 340, 15, igCount, Color.LightGray);

            // Add IG1 (MZ) button
            AddSingleButtonAndTextBox("IG", "bInterlockingGuard1_MZ", 340, 15 + igCount * 40, "IG1 (MZ)", Color.LightGray);

            // Add RB buttons
            int rbCount = 8;
            AddButtonsAndTextBoxes("RB", "ResetButtons_TS", 500, 15, rbCount, Color.LightBlue);

            // Add RB lamps
            AddLeds("LampRB", "LampResetButtons_TS", 640, 10, rbCount);

            // Add additional MZ buttons (excluding IG1 (MZ) which is already added)
            int additionalButtonY = 15 + 8 * 40;
            AddSingleButtonAndTextBox("ESB", "bEmergencyStopButton1_MZ", 20, additionalButtonY, "ESB1 (MZ)", Color.LightSalmon);
            AddSingleButtonAndTextBox("LC", "bLightcurtain1_MZ", 180, additionalButtonY, "LC1 (MZ)", Color.LightYellow);
            AddSingleButtonAndTextBox("RB", "bResetButton1_MZ", 500, additionalButtonY, "RB1 (MZ)", Color.LightBlue);

            // Start the cyclic read task
            StartCyclicRead();
        }

        private void AddButtonsAndTextBoxes(string prefix, string variableArrayName, int buttonX, int startY, int count, Color buttonColor)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleButtonAndTextBox(prefix, $"arr{variableArrayName}[{i}]", buttonX, startY + i * 40, $"{prefix}{i + 1} (TS)", buttonColor);
            }
        }

        private void AddSingleButtonAndTextBox(string prefix, string variableName, int buttonX, int yPosition, string buttonText, Color buttonColor)
        {
            Button button = new Button
            {
                Text = buttonText,
                Location = new Point(buttonX, yPosition),
                BackColor = buttonColor
            };

            TextBox textBox = new TextBox
            {
                Location = new Point(buttonX + 80, yPosition),
                Width = 60 // Reduced width by half
            };

            try
            {
                uint handle = connector.CreateVariableHandle($"MachineObjectsArray.SafetyTestTrolley[0].{variableName}");
                VariableHandledata.Add(button.Text, handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating variable handle for {buttonText}: {ex.Message}");
            }

            button.MouseDown += Button_MouseDown;
            button.MouseUp += Button_MouseUp;

            this.Controls.Add(button);
            this.Controls.Add(textBox);
        }

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            uint variablehandle = VariableHandledata[button.Text];
            connector.WriteBool(variablehandle, true);
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            uint variablehandle = VariableHandledata[button.Text];
            connector.WriteBool(variablehandle, false);
        }

        private void AddLeds(string prefix, string variableArrayName, int ledX, int startY, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleLed(prefix, $"arr{variableArrayName}[{i}]", ledX, startY + i * 40, $"{prefix}{i + 1} (TS)");
            }

            // Add the ninth LED for bLampResetButton1_MZ
            AddSingleLed(prefix, "bLampResetButton1_MZ", ledX, startY + count * 40, $"{prefix}{count + 1} (MZ)");
        }

        private void AddSingleLed(string prefix, string variableName, int ledX, int yPosition, string ledText)
        {
            LedSingleControl led = new LedSingleControl
            {
                Width = 35,
                Height = 35,
                Text = ledText,
                Value = false,
                Location = new Point(ledX, yPosition),
                OnColor = Color.Blue
            };

            try
            {
                uint handle = connector.CreateVariableHandle($"MachineObjectsArray.SafetyTestTrolley[0].{variableName}");
                VariableHandledata.Add(led.Text, handle);
                leds.Add(led);
                ledStates[led.Text] = false; // Initialize with default state
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating variable handle for {ledText}: {ex.Message}");
            }

            this.Controls.Add(led);
        }

        private void StartCyclicRead()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(500); // Adjusted to avoid flickering
                    ReadLedStates();
                }
            });
        }

        private void ReadLedStates()
        {
            foreach (var led in leds)
            {
                try
                {
                    uint handle = VariableHandledata[led.Text];
                    bool currentValue = connector.ReadBool(handle);
                    if (ledStates[led.Text] != currentValue)
                    {
                        ledStates[led.Text] = currentValue; // Update the state
                        UpdateLedControl(led, currentValue);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading variable handle for {led.Text}: {ex.Message}");
                }
            }
        }

        private void UpdateLedControl(LedSingleControl led, bool state)
        {
            context.Post(new SendOrPostCallback(o =>
            {
                led.Value = state;
            }), null);
        }
    }
}

