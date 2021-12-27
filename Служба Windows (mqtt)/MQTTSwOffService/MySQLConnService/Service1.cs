using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Globalization;


public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public int dwServiceType;
    public ServiceState dwCurrentState;
    public int dwControlsAccepted;
    public int dwWin32ExitCode;
    public int dwServiceSpecificExitCode;
    public int dwCheckPoint;
    public int dwWaitHint;
};

namespace MySQLConnService
{
    public partial class Service1 : ServiceBase
    {

        MQTTCli MQTTCommand;
        MQTTCli MQTTState;
        MQTTCli MQTTStateUpdTime;
        //       private int eventId = 1;
        public Service1()
        {
            InitializeComponent();

            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        protected override void OnStart(string[] args)
        {

            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart.");

            MQTTCommand = new MQTTCli("/RZ/CompCommand");
            MQTTCommand.Connect();
            MQTTCommand.MQTTSubscribe();

            MQTTState = new MQTTCli("/RZ/CompState");
            MQTTState.Connect();

            MQTTStateUpdTime = new MQTTCli("/RZ/CompStateUpdateTime");
            MQTTStateUpdTime.Connect();

            MQTTCommand.MQTTPublish("Нет команды");
            MQTTState.MQTTPublish("Включено");

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = 20000; // 20 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {

            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            if (!MQTTCommand.Connected)
            { 
                MQTTCommand.Connect();
                MQTTCommand.MQTTSubscribe();
            }

            if (!MQTTState.Connected)
            {
                MQTTState.Connect();
                MQTTState.MQTTSubscribe();
            }
            MQTTCommand.MQTTPublish("Нет команды");
            MQTTState.MQTTPublish("Выключено");

            eventLog1.WriteEntry("In OnStop.");

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.

            //textBox1.Text = Program.Data;

            eventLog1.WriteEntry("Monitoring the System");

            if (!MQTTCommand.Connected)
            {
                MQTTCommand.Connect();
                MQTTCommand.MQTTSubscribe();
            }

            if (!MQTTState.Connected) MQTTState.Connect();
            if (!MQTTStateUpdTime.Connected)
            {
                MQTTStateUpdTime.Connect();
            }

            if (MQTTCommand.Data != "")
            {
                eventLog1.WriteEntry("Current Command: " + MQTTCommand.Data);
                if (MQTTCommand.Data == "Выключить")
                {
                    MQTTState.MQTTPublish("Выключение компьютера");
                    eventLog1.WriteEntry("Turn Off Command");
                    System.Diagnostics.Process.Start("Shutdown", "-s -t 10");
                    MQTTState.MQTTPublish("Выключено");
                    MQTTCommand.MQTTPublish("Нет команды");
                }
            }
            DateTime localDate = DateTime.Now;

            string cultureName = "ru-RU";
            var culture = new CultureInfo(cultureName);
            MQTTStateUpdTime.MQTTPublish(localDate.ToString(culture));

        }


    }

    class MQTTCli
    {
        private MqttClient client;

        public string clientId;
        public string Data;
        public string Topic;

        public bool Connected;

        public MQTTCli(string Topic)
        {

            this.Topic = Topic;

            this.client = new MqttClient("broker.hivemq.com");
            this.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            this.clientId = Guid.NewGuid().ToString();
        }

        public void Connect()
        {
            try
            {
                this.client.Connect(clientId);
                Connected = this.client.IsConnected;
            }
            catch (Exception)
            {
                Connected = false;
            }
        }

        public void Disconnect()
        {
            this.client.Disconnect();
            Connected = false;
        }

        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            this.Data = Encoding.UTF8.GetString(e.Message);
        }

        public void MQTTPublish(string PublishData)
        {
            this.client.Publish(this.Topic, Encoding.UTF8.GetBytes(PublishData), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        }
        public void MQTTSubscribe()
        {
            this.client.Subscribe(new string[] { this.Topic }, new byte[] { 0 });
        }


    }
}
