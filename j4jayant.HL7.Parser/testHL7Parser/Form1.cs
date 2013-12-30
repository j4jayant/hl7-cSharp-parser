using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using j4jayant.HL7.Parser;

namespace testHL7Parser
{
    public partial class Form1 : Form
    {
        string strHL7Message = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            strHL7Message = @"MSH|^~\&|ADT|ADI|ADT-1|ADI-1|20050215||ADT^A01|MSGADT003|T|2.4" + "\r" +
                             "EVN|A01|20031016000000" + "\r" +
                             "PID|1|111222333|H123123^^^^MR^ADT~111-222-333^^^^SS^ADT||John^Smith|GARSEN^^Melissa|19380818|M||2028-9|241 AVE^^Lake City^WA^98125^^^^100|100|(425)111-2222|(425)111-2222||S|CHR|1234567|111-222-333" + "\r" +
                             "NK1|2|GARSEN^Melissa" + "\r" +
                             "PV1|1|E|||||D123^Jeff^Carron|||MED||||7|||D123^Jeff^Taylor|E|3454|R^20050215|||||||||||||||||||EM|||||20050215" + "\r" +
                             "IN1|1|I123|ICOMP1|INS COMP 1|PO BOX 1^^Lake City^WA^98125||||||||||1|John^Smith|01|19380818" + "\r" +
                             "IN2|1||RETIRED" + "\r" +
                             "IN1|2|I456|ICOMP2|INS COMP 1|PO BOX 2^^Lake City^WA^98125||||||||||8|John^Smith|01|19380818" + "\r" +
                             "IN2|2||RETIRED" + "\r";

            txtResult.AppendText(strHL7Message);
            txtResult.AppendText("\n\n\n");

            j4jayant.HL7.Parser.Message hl7Message = new j4jayant.HL7.Parser.Message(strHL7Message);
            bool isParsed = false;
            try
            {
                isParsed = hl7Message.ParseMessage();
            }
            catch (Exception ex)
            {

            }

            if (isParsed)
            {
                txtResult.AppendText("Get List of All Segments\n");

                List<Segment> segList = hl7Message.Segments();

                foreach(Segment s in segList)
                {
                    txtResult.AppendText(s.Name + "\n");

                    //foreach (Field f in s.GetAllFields())
                    //{
                    //    txtResult.AppendText(f.Value);
                    //}
                }

                txtResult.AppendText("Get all repetitions of IN1 Segment\n");

                List<Segment> segIN1List = hl7Message.Segments("IN1");

                foreach (Segment s in segIN1List)
                {
                    txtResult.AppendText(s.Name + "\n");

                    //foreach (Field f in s.GetAllFields())
                    //{
                    //    txtResult.AppendText(f.Value);
                    //}
                }


                txtResult.AppendText("Get particular IN1 segment, second repetition\n");

                Segment segIN1_2 = hl7Message.Segments("IN1")[1];

                int fieldIndex = 1;
                foreach (Field f in segIN1_2.GetAllFields())
                {
                    txtResult.AppendText("IN1." + fieldIndex++ + ": " + f.Value + "\n");
                }

                txtResult.AppendText("Get count of all the IN1s in the message\n");

                int in1Count = hl7Message.Segments("IN1").Count;

                txtResult.AppendText("Count of IN1s in message: " + in1Count+ "\n");


                txtResult.AppendText("Access Field Value\n");

                txtResult.AppendText("MSH-4: " + hl7Message.getValue("MSH.4") + "\n");
                txtResult.AppendText("MSH-4: " + hl7Message.DefaultSegment("MSH").Fields(4).Value + "\n");
                txtResult.AppendText("MSH-4: " + hl7Message.Segments("MSH")[0].Fields(4).Value + "\n");

                txtResult.AppendText("Check if field is componentized\n");

                txtResult.AppendText("is PV1-7 componentized: " + hl7Message.IsComponentized("PV1.7") + "\n");
                txtResult.AppendText("is PV1-7 componentized: " + hl7Message.DefaultSegment("PV1").Fields(7).IsComponentized + "\n");
                txtResult.AppendText("is PV1-7 componentized: " + hl7Message.Segments("PV1")[0].Fields(7).IsComponentized + "\n");


                txtResult.AppendText("Check if field has repetitions\n");

                txtResult.AppendText("is PID-3 rereated?: " + hl7Message.HasRepeatitions("PID.3") + "\n");
                txtResult.AppendText("is PID-3 repeated?: " + hl7Message.DefaultSegment("PID").Fields(3).HasRepetitions + "\n");
                txtResult.AppendText("is PID-3 repeated?: " + hl7Message.Segments("PID")[0].Fields(3).HasRepetitions + "\n");

            }
        }
    }
}
