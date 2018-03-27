using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Text;
using LUIS.Settings;
namespace LUIS.Handler
{
    public class IotHubHandler
    {
        RegistryManager registryManager;
        ServiceClient serviceClient;
         //string connectionString = "HostName=EVAIOTHUB.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=fUervPYast3jF+mp8aFUFrdzjjCnDQ/R2xjMZDPyVrU=";
        IOTHubSettings iotHubSettings;
        public IotHubHandler(IOTHubSettings iotHubSettings)
        {
            
            this.iotHubSettings = iotHubSettings;

        }

        public string addDevice(string deviceId)
        {
            registryManager = RegistryManager.CreateFromConnectionString(iotHubSettings.connectionString);
            Task<string> response = AddDeviceAsync(deviceId);
            return response.Result;
        }

        private async Task<string> AddDeviceAsync(string deviceIdentity)
        {
            string deviceId = deviceIdentity;
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        public int getDevicesCount()
        {
            DevicesProcessor a = new DevicesProcessor(iotHubSettings.connectionString, 10, "");
            var listOfDevices = a.GetDevices().GetAwaiter().GetResult();
            return listOfDevices.Count;
        }

        public string getDevices()
        {
            DevicesProcessor a = new DevicesProcessor(iotHubSettings.connectionString, 10, "");
            var listOfDevices = a.GetDevices().GetAwaiter().GetResult();
            return Newtonsoft.Json.JsonConvert.SerializeObject(listOfDevices);
        }

        public async Task SendCloudToDeviceMessageAsync(string deviceID, string message)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubSettings.connectionString);
            var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
            await serviceClient.SendAsync(deviceID, commandMessage);
        }
    }
}
