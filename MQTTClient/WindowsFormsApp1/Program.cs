using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;


namespace WindowsFormsApp1
{
    static class Program
    {

        static public MqttClient client;
        static string clientId;

        static public string Data;
        static public byte connect;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            client = new MqttClient("broker.hivemq.com");
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            clientId = Guid.NewGuid().ToString();
            connect = client.Connect(clientId);
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)

        {

            Data = Encoding.UTF8.GetString(e.Message);
            
            //SetText(ReceivedMessage);

        }

        static public  void MQTTPublish(string data)
        {
            string Topic = "/RZ/test";
            client.Publish(Topic, Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        }
        static public void MQTTSubscribe()
        {
            string Topic = "/RZ/test";
            client.Subscribe(new string[] { Topic }, new byte[] { 0 });
        }
    }
}
