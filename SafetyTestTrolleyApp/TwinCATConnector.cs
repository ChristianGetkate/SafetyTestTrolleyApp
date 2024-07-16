using System;
using TwinCAT.Ads;
using System.Windows.Forms;

namespace SafetyTestTrolleyApp
{
    internal class TwinCATConnector
    {
        private AdsClient adsClient = new AdsClient();

        public void Connect(string AmsNetId, int port)
        {
            adsClient.Connect(AmsNetId, port);
        }

        public uint CreateVariableHandle(string variableName)
        {
            try
            {
                return adsClient.CreateVariableHandle(variableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating variable handle: {ex.Message}");
                throw;
            }
        }

        public bool ReadBool(uint handle)
        {
            try
            {
                return (bool)adsClient.ReadAny(handle, typeof(bool));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading variable: {ex.Message}");
                return false;
            }
        }

        public void WriteBool(uint handle, bool value)
        {
            try
            {
                adsClient.WriteAny(handle, value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing variable: {ex.Message}");
            }
        }
    }
}
