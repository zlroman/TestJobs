using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using System.Globalization;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Subcribe
            Program.MQTTSubscribe();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Publish
            Program.MQTTPublish(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte a = Program.connect;

            textBox1.Text = Program.Data;
            if (Program.Data == "Shutdown")
            {
                Program.MQTTPublish("Done");

                // Выключиьт
                textBox1.Text = "Выключение";
            }





        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Program.client.IsConnected)
                label1.Text = "connected";
            else label1.Text = "Not connectd";


            DateTime localDate = DateTime.Now;
            DateTime utcDate = DateTime.UtcNow;
            String[] cultureNames = { "en-US", "en-GB", "fr-FR",
                                "de-DE", "ru-RU" };

            string cultureName = "ru-RU";
            var culture = new CultureInfo(cultureName);

            label2.Text = culture.NativeName;
            label3.Text = localDate.ToString(culture);
            label4.Text = utcDate.ToString(culture);



        }
    }

}