using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace SafetyTestTrolleyApp
{
    internal class TwinCATConnector
    {
        public AdsClient adsClient = new AdsClient();

        public void Connect(string AmsNetId, int port)
        {
            adsClient.Connect(AmsNetId, port);
        }

        // Example to read a boolean variable
        public bool ReadBool(string variableName)
        {
            try
            {
                var handle = adsClient.CreateVariableHandle(variableName);
                var value = (bool)adsClient.ReadAny(handle, typeof(bool));
                adsClient.DeleteVariableHandle(handle);
                return value;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading variable: {ex.Message}");
                return false;
            }
        }

        // Example to write a boolean variable
        public void WriteBool(string variableName, bool value)
        {
            try
            {
                var handle = adsClient.CreateVariableHandle(variableName);
                adsClient.WriteAny(handle, value);
                adsClient.DeleteVariableHandle(handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing variable: {ex.Message}");
            }
        }
    }
}
