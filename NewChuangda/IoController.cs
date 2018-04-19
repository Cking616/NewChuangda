using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.Net.Sockets;

namespace NewChuangda
{
    class IoController
    {
        private ModbusIpMaster ioMaster;
        private TcpClient ioClient;
        private string ioAddress = "";
        private int ioPort = 502;
        private ushort[] ioInMapBuffer;
        private ushort[] ioOutMapBuffer;
        private ushort ioStartAddress;
        private ushort ioNumberOfPoints;
        public bool Connected { get => ioClient.Connected; }

        public IoController(string ioAddress, int ioPort, ushort ioStartAddress, ushort ioNumberOfPoints)
        {
            this.ioAddress = ioAddress;
            this.ioPort = ioPort;
            this.ioStartAddress = ioStartAddress;
            this.ioNumberOfPoints = ioNumberOfPoints;

            ioClient = new TcpClient(ioAddress, ioPort);
            ioMaster = ModbusIpMaster.CreateIp(ioClient);

            ioInMapBuffer = ioMaster.ReadInputRegisters(ioStartAddress, ioNumberOfPoints);
            ioOutMapBuffer = ioInMapBuffer;
        }

        public bool WriteOutput(int port, int bit, bool value)
        {
            if (port >= ioNumberOfPoints || port < 0)
                return false;
            if (bit < 0 || bit > 7)
                return false;

            if (value)
            {
                ioOutMapBuffer[port] = (ushort)(ioOutMapBuffer[port] | (1 << bit));
            }
            else
            {
                ioOutMapBuffer[port] = (ushort)(ioOutMapBuffer[port] & (~(1 << bit)));
            }

            return true;
        }

        public bool ReadInput(int port, int bit)
        {
            if (port >= ioNumberOfPoints || port < 0)
                return false;
            if (bit < 0 || bit > 7)
                return false;

            int value = (ioOutMapBuffer[port] & (1 << bit));
            if (value != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnTimer()
        {
            ioInMapBuffer = ioMaster.ReadInputRegisters(ioStartAddress, ioNumberOfPoints);

            ioMaster.WriteMultipleRegisters(ioStartAddress, ioOutMapBuffer);
        }
    }
}
