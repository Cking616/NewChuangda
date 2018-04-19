﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.ClientEngine;
using System.Net;

namespace NewChuangda
{
    class NanotecFilter : TerminatorReceiveFilter<StringPackageInfo>
    {
        public NanotecFilter() : base(Encoding.ASCII.GetBytes("\r\0"))
        {
        }

        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            byte[] bytes = new byte[bufferStream.Length];
            bufferStream.Read(bytes, 0, bytes.Length);
            string str = System.Text.Encoding.ASCII.GetString(bytes);
            str = str.Replace("\0", string.Empty);
            str = str.Replace("\r", string.Empty);

            return new StringPackageInfo(str, "", null);
        }
    }

    class NanotecStep
    {
        private EasyClient nanoClient;
        private string nanoAddress = "";
        private int nanoPort = 5000;

        private bool nanoIsIdle = true;
        private string nanoLastSend = "";

        private IrRobotFilter nanoFilter = new IrRobotFilter();
        private Queue<string> nanoSendBuffer = new Queue<string>();

        private bool nanoTimeEnable;

        public bool IsConnected { get => nanoClient.IsConnected; }
        public bool IsIdle { get => nanoIsIdle; }

        public NanotecStep(string address, int port)
        {
            nanoAddress = address;
            nanoPort = port;
            nanoClient = new EasyClient();

            nanoTimeEnable = false;
            nanoClient.Connected += OnConnected;
            nanoClient.Closed += OnClosed;
            // Initialize the client with the receive filter and request handler
            nanoClient.Initialize(nanoFilter, OnRecieve);
            nanoClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(nanoAddress), nanoPort));

            //irTimer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromMilliseconds(50)
            //};
            //irTimer.Tick += OnTimer;
        }

        private bool __SendCmd(string cmd)
        {
            if (!nanoIsIdle)
            {
                return false;
            }

            string send = cmd;
            if (cmd.IndexOf("\r") != cmd.Length - 1)
                send = cmd + "\r";

            nanoClient.Send(Encoding.ASCII.GetBytes(send));
            nanoLastSend = cmd;

            nanoIsIdle = false;
            return true;
        }

        public bool SendCmd(string cmd)
        {
            nanoSendBuffer.Enqueue(cmd);
            return true;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            nanoSendBuffer.Clear();
            nanoTimeEnable = true;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            nanoTimeEnable = false;
        }

        private void OnRecieve(StringPackageInfo obj)
        {
            if (obj.Key[2] != 'A')
                nanoIsIdle = true;
            else
            {
                if ( obj.Key.StartsWith("001") )
                {
                    nanoIsIdle = true;
                }
            }
        }

        public void OnTimer()
        {
            if(!nanoTimeEnable)
            {
                return;
            }

            if (nanoSendBuffer.Count > 0)
            {
                string cmd = nanoSendBuffer.ElementAt(0);
                if (__SendCmd(cmd))
                {
                    //string msg = "执行指令" + cmd;
                    //AppLog.Info("系统", msg);
                    nanoSendBuffer.Dequeue();
                }
            }
        }
    }
}
