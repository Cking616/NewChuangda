using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SuperSocket.ProtoBase;
using SuperSocket.ClientEngine;


namespace NewChuangda
{
    struct IrScaraPoint
    {
        public float acutualZPoint;
        public float acutualSPoint;
        public float acutualEPoint;
        public float acutualWPoint;
        public string station;
        public bool isPerch;
        public float index;
        public float closeZPoint;
        public float closeSPoint;
        public float closeEPoint;
        public float closeWPoint;
    }

    class IrRobotFilter : TerminatorReceiveFilter<StringPackageInfo>
    {
        public IrRobotFilter()
            : base(Encoding.ASCII.GetBytes("\r\n\0"))
        {
        }

        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            byte[] bytes = new byte[bufferStream.Length];
            bufferStream.Read(bytes, 0, bytes.Length);
            string str = System.Text.Encoding.ASCII.GetString(bytes);
            str = str.Replace("\0", string.Empty);
            str = str.Replace("\r\n", string.Empty);
            AppLog.Info("Ir接收", str);

            if (str.StartsWith("!") || str.StartsWith(">!"))
            {
                string[] strArry = str.Split(new Char[] { '\t', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string strHead = strArry[1];
                string[] strParam = strArry.Skip(2).ToArray();
                string[] strHeads = strHead.Split('-');
                if (strHeads.Length < 2)
                {
                    return new StringPackageInfo(strHead, "", strParam);
                }
                else
                {
                    string strBody = strHeads[0];
                    strHead = strHeads[1];
                    return new StringPackageInfo(strHead, strBody, strParam);
                }
            }
            else if (str.StartsWith(">"))
            {
                return new StringPackageInfo("BEGIN", "", null);
            }
            else
            {
                string[] strArry = str.Split();
                string strHead = strArry[0];
                string[] strParam = strArry.Skip(2).ToArray();
                string[] strHeads = strHead.Split('-');
                if (strHeads.Length < 2)
                {
                    return new StringPackageInfo(strHead, "", strParam);
                }
                else
                {
                    string strBody = strHeads[0];
                    strHead = strHeads[1];
                    return new StringPackageInfo(strHead, strBody, strParam);
                }
            }
        }
    }

    class IrRobot
    {
        private EasyClient irClient;
        private string irAddress = "";
        private int irPort = 5000;

        private int irResetC = 0;
        private bool irNeedReset = false;
        private bool irIsIdle = true;
        private string irLastSend = "";

        private IrRobotFilter irFilter = new IrRobotFilter();
        private Queue<string> irSendBuffer = new Queue<string>();

        private IrScaraPoint irCurPoint;
        private string irTargetStation;
        private bool irNeedFixPoint;
        private bool irIsSendRCP;
        private bool irTimeEnable;
        private bool irIsErrored;

        public bool IsConnected { get => irClient.IsConnected; }
        public bool IsIdle { get => irIsIdle; }
        public string CurStation { get => irCurPoint.station; }
        public bool IsErrored { get => irIsErrored; set => irIsErrored = value; }

        public IrRobot(string address, int port)
        {
            irAddress = address;
            irPort = port;
            irClient = new EasyClient();

            irClient.Connected += OnConnected;
            irClient.Closed += OnClosed;
            // Initialize the client with the receive filter and request handler
            irClient.Initialize(irFilter, OnRecieve);
            irClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(irAddress), irPort));
            irIsErrored = false;
        }

        public bool CheckStation(string station, bool isLow, bool isPerch, int index)
        {
            if (irCurPoint.station != station)
                return false;
            if (irCurPoint.isPerch != isPerch)
                return false;
            if (irCurPoint.index != index)
                return false;
            return true;
        }

        public bool LearnStation(string station, bool isLow, bool isPerch, int index)
        {
            string cmd = "learn " + station;

            if (isPerch)
            {
                cmd += " perch,";
            }
            else
            {
                cmd += " inside,";
            }

            if (isLow)
            {
                index = -index;
            }

            cmd += string.Format(" index = {0:D},", index);

            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool MoveStation(string station, bool isHigh, bool isInside, bool islinear, int index, int speed)
        {
            string cmd = "move " + station;

            if (!isInside)
            {
                cmd += " perch,";
            }
            else
            {
                cmd += " inside,";
            }

            if (!isHigh)
            {
                index = -index;
            }

            cmd += string.Format(" index = {0:D},", index);

            cmd += string.Format(" speed {0:D}", speed);

            if (islinear)
            {
                cmd += ", linear";
            }

            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool Pick(string station, int index)
        {
            string cmd = "pick " + station;

            cmd += string.Format(" index = {0:D},", index);

            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool Place(string station, int index)
        {
            string cmd = "place " + station;
            cmd += string.Format(" index = {0:D},", index);
            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool Grip()
        {
            string cmd = "Grip 1 ON";
            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool Release()
        {
            string cmd = "Grip 1 OFF";
            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool SendCmd(string cmd)
        {
            lock (irSendBuffer)
            {
                irSendBuffer.Enqueue(cmd);
            }
            return true;
        }

        public bool ReConnect()
        {
            irClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(irAddress), irPort));
            return true;
        }

        private void PaserRcpParameters(string[] param)
        {
            if (param[0] == "ACTUAL-Z")
            {
                irCurPoint.acutualZPoint = float.Parse(param[1]);
            }
            else if (param[0] == "ACTUAL-S")
            {
                irCurPoint.acutualSPoint = float.Parse(param[1]);
            }
            else if (param[0] == "ACTUAL-E")
            {
                irCurPoint.acutualEPoint = float.Parse(param[1]);
            }
            else if (param[0] == "ACTUAL-W")
            {
                irCurPoint.acutualWPoint = float.Parse(param[1]);
            }
            else if (param[0] == "STATION")
            {
                irCurPoint.station = param[1].Replace("\"", string.Empty); ;
            }
            else if (param[0] == "INDEX")
            {
                irCurPoint.index = float.Parse(param[1]);
            }
            else if (param[0] == "CLOSEST-Z")
            {
                irCurPoint.closeZPoint = float.Parse(param[1]);
            }
            else if (param[0] == "CLOSEST-S")
            {
                irCurPoint.closeSPoint = float.Parse(param[1]);
            }
            else if (param[0] == "CLOSEST-E")
            {
                irCurPoint.closeEPoint = float.Parse(param[1]);
            }
            else if (param[0] == "CLOSEST-W")
            {
                irCurPoint.closeWPoint = float.Parse(param[1]);
            }
            else if (param[0] == "PERCH")
            {
                irCurPoint.isPerch = true;
            }
            else if (param[0] == "INSIDE")
            {
                irCurPoint.isPerch = false;
            }
            else
            {
                return;
            }
        }

        private void OnConnected(Object state, EventArgs e)
        {
            lock (irSendBuffer)
            {
                irSendBuffer.Clear();
            }
            irNeedFixPoint = true;
            irIsSendRCP = false;
            irTimeEnable = true;
            AppLog.Info("系统", "成功与Ir控制器建立连接");
        }

        private void OnClosed(Object state, EventArgs e)
        {
            irTimeEnable = false;
            AppLog.Info("系统", "与Ir控制器连接断开");
        }

        private bool __SendCmd(string cmd)
        {
            if (!irIsIdle)
            {
                if (!irNeedReset)
                {
                    return false;
                }
            }

            string send = cmd;
            if (cmd.IndexOf("\n") != cmd.Length - 1)
                send = cmd + "\n";

            if( irIsErrored && send != "reset\n")
            {
                return false;
            }

            irClient.Send(Encoding.ASCII.GetBytes(send));
            string[] cmdList = cmd.Split();
            irLastSend = cmdList[0].ToUpper();
           
            if (irLastSend == "MOVE")
            {
                irTargetStation = cmdList[1].ToUpper();
                if (irTargetStation == "HOME")
                {
                    irLastSend = irTargetStation;
                }
            }
            irIsIdle = false;
            return true;
        }

        private void OnRecieve(StringPackageInfo request)
        {
            string key = request.Key.ToUpper();
            string body = request.Body.ToUpper();
            if (key == "BEGIN")
            {
                return;
            }

            if (key == "ERROR")
            {
                irResetC = 15;
                irNeedReset = true;
                irIsErrored = true;
                AppLog.Info("系统", "收到错误返回，将会自动Reset");
                lock (irSendBuffer)
                {
                    irSendBuffer.Clear();
                }
                return;
            }

            if (key == "RESET" && body == "END")
            {
                AppLog.Info("系统", "Reset成功");
                irNeedReset = false;
                irIsIdle = true;
                return;
            }

            if (key == "RCP" )
            {
                if(body == "END")
                {
                    irNeedFixPoint = false;
                    irIsIdle = true;
                    return;
                }
                else
                {
                    string[] param = request.Parameters;
                    PaserRcpParameters(param);
                    return;
                }
            }

            if (key == "MOVE" && body == "END")
            {
                if(irTargetStation != irCurPoint.station)
                {
                    irNeedFixPoint = true;
                    irIsSendRCP = false;
                }

                irIsIdle = true;
                return;
            }

            if (key == "HOME" && body == "END")
            {
                if (irTargetStation != irCurPoint.station)
                {
                    irNeedFixPoint = true;
                    irIsSendRCP = false;
                }

                irIsIdle = true;
                return;
            }

            if (key == irLastSend && body == "END")
            {
                irIsIdle = true;
                string msg = request.Key + "指令执行成功";
                AppLog.Info("系统", msg);
            }
        }

        public void OnTimer()
        {
            if (!irTimeEnable)
                return;

            if (irNeedReset)
            {
                if (irResetC > 0)
                {
                    irResetC--;
                }
                else
                {
                    AppLog.Info("系统", "发送Reset");
                    __SendCmd("reset");
                    irResetC = 200;
                }
                return;
            }

            if ( irNeedFixPoint && !irIsSendRCP)
            {
                irIsSendRCP = true;
                __SendCmd("rcp");
            }

            lock(irSendBuffer)
            { 
                if (irSendBuffer.Count > 0)
                {
                    string cmd = irSendBuffer.ElementAt(0);
                    if (__SendCmd(cmd))
                    {
                        string msg = "执行指令" + cmd;
                        AppLog.Info("系统", msg);
                        irSendBuffer.Dequeue();
                    }
                }
            }
        }
    }
}
