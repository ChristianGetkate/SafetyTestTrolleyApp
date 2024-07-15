
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TwinCAT.Ads;

namespace SafetyTestTrolleyApp
{
    public partial class Form1 : Form
    {
        private TwinCATConnector connector = new TwinCATConnector();
        private Dictionary<string, uint> VariableHandledata = new();

        public Form1()
        {
            InitializeComponent();
            connector.Connect(AmsNetId.Local.ToString(), 851);

            // Add ESB buttons
            int esbCount = 8;
            AddButtonsAndTextBoxes("ESB", "EmergencyStopButtons_TS", 20, 15, esbCount);

            // Add LC buttons
            int lcCount = 8;
            AddButtonsAndTextBoxes("LC", "Lightcurtains_TS", 220, 15, lcCount);

            // Add IG buttons
            int igCount = 4;
            AddButtonsAndTextBoxes("IG", "InterlockingGuards_TS", 420, 15, igCount);

            // Add IG1 (MZ) button
            AddSingleButtonAndTextBox("IG", "bInterlockingGuard1_MZ", 420, 15 + igCount * 40, "IG1 (MZ)");

            // Add RB buttons
            int rbCount = 8;
            AddButtonsAndTextBoxes("RB", "ResetButtons_TS", 620, 15, rbCount);

            // Add additional MZ buttons (excluding IG1 (MZ) which is already added)
            int additionalButtonY = 15 + 8 * 40;
            AddSingleButtonAndTextBox("ESB", "bEmergencyStopButton1_MZ", 20, additionalButtonY, "ESB1 (MZ)");
            AddSingleButtonAndTextBox("LC", "bLightcurtain1_MZ", 220, additionalButtonY, "LC1 (MZ)");
            AddSingleButtonAndTextBox("RB", "bResetButton1_MZ", 620, additionalButtonY, "RB1 (MZ)");
        }

        private void AddButtonsAndTextBoxes(string prefix, string variableArrayName, int buttonX, int startY, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleButtonAndTextBox(prefix, $"arr{variableArrayName}[{i}]", buttonX, startY + i * 40, $"{prefix}{i + 1} (TS)");
            }
        }

        private void AddSingleButtonAndTextBox(string prefix, string variableName, int buttonX, int yPosition, string buttonText)
        {
            Button button = new Button
            {
                Text = buttonText,
                Location = new Point(buttonX, yPosition)
            };

            TextBox textBox = new TextBox
            {
                Location = new Point(buttonX + 80, yPosition)
            };

            try
            {
                uint handle = connector.adsClient.CreateVariableHandle($"SafetyTestTrolleyObjectsArray.SafetyTestTrolley[0].{variableName}");
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

            connector.adsClient.WriteAny(variablehandle, true);
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;

            uint variablehandle = VariableHandledata[button.Text];

            connector.adsClient.WriteAny(variablehandle, false);
        }
    }
}
