using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Globalization;
using System.Threading;

namespace Dictator
{
    public partial class Form1 : Form
    {
        public bool reading;
        public SpeechSynthesizer synthetiser;
        public bool textchanged;
        public Thread CheckState;

        public Form1()
        {
            InitializeComponent();
            Pausebtn.Enabled = false;
            StopBtn.Enabled = false;
            reading = false;
            textchanged = false;
        }

        private void CheckReadState()
        {
            int oldState = 0;
            String state = synthetiser.State.ToString();
            label1.Text = state;
            while (reading)
            {
                if ((int)synthetiser.State != oldState)
                {
                    //state changed
                    int newState = ((int)synthetiser.State);
                    if (newState == 2)
                    {
                        oldState = 2;
                        //Paused
                        Pausebtn.Enabled = false;
                        StopBtn.Enabled = true;
                        ReadBtn.Enabled = true;
                    }
                    else if (newState == 1)
                    {
                        oldState = 1;
                        //Speaks again
                        Pausebtn.Enabled = true;
                        ReadBtn.Enabled = false;
                        StopBtn.Enabled = true;
                    }
                }
            }
            //if i change text and then click on read again, it finished directly, as if it were not reading (=false)
            label1.Text = "finished";
            textchanged = false;
            ReadBtn.Enabled = true;
            Pausebtn.Enabled = false;
            StopBtn.Enabled = false;
        }

        public void Speak_done(object sender, SpeakCompletedEventArgs e)
        {
            reading = false;
            textchanged = false;
        }

        public void ReadText(String text)
        {
            try
            {
                CheckState.Abort();
                label1.Text = "Process aborted";
            }
            catch (NullReferenceException) { }
            synthetiser = new SpeechSynthesizer();
            synthetiser.SetOutputToDefaultAudioDevice();
            synthetiser.SpeakAsync(text);
            synthetiser.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(Speak_done);
            reading = true;
            CheckState = new Thread(new ThreadStart(CheckReadState));
            Pausebtn.Enabled = true;
            ReadBtn.Enabled = false;
            StopBtn.Enabled = true;
            CheckState.Start();
        }

        private void ReadBtn_Click(object sender, EventArgs e)
        {
            if (!reading)
            {
                //not reading yet
                textchanged = false;
                String text = textBox1.Text;
                ReadText(text);
            }
            else
            {
                //Just paused
                if (textchanged)
                {
                    label1.Text = "textchanged";
                    textchanged = false;
                    synthetiser.SpeakAsyncCancelAll();
                    ReadText(textBox1.Text);
                }
                else
                {
                    synthetiser.Resume();
                }
            }
        }

        private void Pausebtn_Click(object sender, EventArgs e)
        {
            synthetiser.Pause();
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            synthetiser.SpeakAsyncCancelAll();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (reading)
                {
                    textchanged = true;
                }
            }
            catch (NullReferenceException) { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try{
                synthetiser.SpeakAsyncCancelAll();
            }catch (NullReferenceException) { }
        }
    }
}
