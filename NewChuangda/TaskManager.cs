using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NLua;

namespace NewChuangda
{
    public static class AppLog
    {
        private static Queue<string> IrRobotRevLog = new Queue<string>();

        public static void AddLog(string log) { IrRobotRevLog.Enqueue(log); }

        public static void Info(string cls, string log)
        {
            string str = cls + ":" + log;
            //IrRobotRevLog.Enqueue(str);
        }

        public static string ReadLog() { return IrRobotRevLog.Dequeue(); }

        public static bool IsEmpty() { return IrRobotRevLog.Count == 0; }

        public static void Error(string err)
        {
            err = "Error:" + err;
            //IrRobotRevLog.Enqueue(err);
        }
    }

    public class TaskManager
    {
        private Lua taskState;
        private IoController ioController;
        private IrRobot irRobot;
        private NanotecStep zStep;
        private NanotecStep yStep;

        private bool isInitialized;

        public TaskManager()
        {
            this.taskState = new Lua();
            isInitialized = false;
        }
        public static void NcdSleep(int mil)
        {
            Thread.Sleep(mil);
        }
        public bool IsInitialized { get => isInitialized; }

        public bool Initialize(string initFile)
        {
            isInitialized = false;
            taskState.DoFile(initFile);

            var ip = taskState["ir_robot_ip"] as string;
            var strPort = taskState["ir_robot_port"] as string;
            if (ip == null || strPort == null)
            {
                return false;
            }
            int port = int.Parse(strPort);
            irRobot = new IrRobot(ip, port);

            ip = taskState["io_controller_ip"] as string;
            strPort = taskState["io_controller_port"] as string;
            if (ip == null || strPort == null)
            {
                return false;
            }
            port = int.Parse(strPort);
            var strStartAddress = taskState["io_controller_start_address"] as string;
            var strNumofPoints = taskState["io_controller_num_of_points"] as string;
            if (strStartAddress == null || strNumofPoints == null)
            {
                return false;
            }
            ushort ioStartAddress = ushort.Parse(strStartAddress);
            ushort ioNumofPoints = ushort.Parse(strNumofPoints);
            ioController = new IoController(ip, port, ioStartAddress, ioNumofPoints);

            ip = taskState["z_step_ip"] as string;
            strPort = taskState["z_step_port"] as string;
            if (ip == null || strPort == null)
            {
                return false;
            }
            port = int.Parse(strPort);
            zStep = new NanotecStep(ip, port);

            ip = taskState["y_step_ip"] as string;
            strPort = taskState["y_step_port"] as string;
            if (ip == null || strPort == null)
            {
                return false;
            }
            port = int.Parse(strPort);
            yStep = new NanotecStep(ip, port);

            taskState["z_step_state"] = zStep;
            taskState["y_step_state"] = yStep;
            taskState["io_controller_state"] = ioController;
            taskState["ir_robot_state"] = irRobot;
            // taskState["task_manager"] = this;
            taskState.LoadCLRPackage();
            taskState.DoString(@" import ('NewChuangda') ");
            isInitialized = true;
            return true;
        }

        public bool DoScript(string scriptFile)
        {
            if (!isInitialized)
            {
                return false;
            }
            taskState.DoFile(scriptFile);
            return true;
        }

        public bool DoString(string str)
        {
            if (!isInitialized)
            {
                return false;
            }
            taskState.DoString(str);
            return true;
        }

        public void OnHighTimer()
        {
           // if (irRobot.IsErrored)
            //{
           ///     return;
           // }

            ioController.OnTimer();
        }

        public void OnTimer()
        {
            //if (irRobot.IsErrored)
            //{
            //    return;
            //}

            irRobot.OnTimer();
            zStep.OnTimer();
            yStep.OnTimer();
        }
    }
}
