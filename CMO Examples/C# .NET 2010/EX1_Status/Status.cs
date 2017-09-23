//
// Example 1 Status and Control Rev 3
//
// This program demonstrates Copley Motion Object Status and Control
//
// The purpose of this Examble is to enable and disable the amplifier and
// read and decode the amplifier status.
//
// This program assumes the following axis configuration:
// 1. Upon startup it will enable axis at Can Node ID 1
// 2. If EtherCAT network is used, the ECAT_NODE needs to be
//    set to match the drives physical location on the network. See
//    EtherCAT network below.
//
// This code also includes the following prerequisites:
// 1. The amplifier and motor must be preconfigured and set up properly to run.
// 2. The hardware enable switch must be installed and easily accessible
// 
// As with any motion product extreme caution must used! Read and understand
// all parameter settings before attemping to send to amplifier.
//
//
// Copley Motion Objects are Copyright, 2002-2015, Copley Controls.
//
// For more information on Copley Motion products see:
// http://www.copleycontrols.com
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using CMLCOMLib;


namespace EX1_Status
{
    public partial class Status : Form
    {
        //***************************************************
        //*
        //*  EtherCAT Network
        //*
        //***************************************************
        //// A negative node number refers to the drive's physical position on the network
        //// -1 for the first drive, -2 for the second, etc.
        //const int ECAT_NODE = -1;
        //EcatObj ecatObj;

        //***************************************************
        //
        //  CANOpen Network
        //
        //***************************************************
        const int X_AXIS_CAN_ADDRESS = 1;
        canOpenObj canOpen;

        AmpObj xAxisAmp;

        // Create a delegate to close down the application in a thread safe way
        delegate void CloseApp();

        public Status()
        {     
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try{ 
                //Initialize code here

                xAxisAmp = new AmpObj();


                //***************************************************
                //
                //  CANOpen Network
                //
                //***************************************************
                canOpen = new canOpenObj();
                //
                //**************************************************************************
                //* The next two lines of code are optional. If no bit rate is specified,
                //* then the default bit rate (1 Mbit per second) is used.  If no port name
                //* is specified, then CMO will use the first supported CAN card found and
                //* use channel 0 of that card.
                //**************************************************************************
                // Set the bit rate to 1 Mbit per second
                canOpen.BitRate = CML_BIT_RATES.BITRATE_1_Mbit_per_sec;
                // Indicate that channel 0 of a Copley CAN card should be used
                canOpen.PortName = "copley0";
                //// Indicate that channel 0 of a KVaser card should be used
                //canOpen.PortName = "kvaser0";
                //// Indicate that channel 0 of an IXXAT card should be used
                //canOpen.PortName = "IXXAT0";
                //// Indicate that channel 0 of an IXXAT card (V3.x or newer drivers) should be used
                //canOpen.PortName = "IXXATV30";
                //***************************************************
                //* Initialize the CAN card and network
                //***************************************************
                canOpen.Initialize();
                //***************************************************
                //* Initialize the amplifier
                //***************************************************
                xAxisAmp.Initialize(canOpen, X_AXIS_CAN_ADDRESS);


                //***************************************************
                //*
                //*  EtherCAT Network
                //*
                //***************************************************
                //ecatObj = new EcatObj();
                //
                //***************************************************
                //* The next line of code is optional. The port name is the IP address of 
                //* the Ethernet adapter. Alternatively, a shortcut name “eth” can be used 
                //* in conjunction with the adapter number. For example “eth0” for the first 
                //* Ethernet adapter, “eth1” for the second adapter. If no port name is 
                //* supplied, it will default to “eth0”.
                //**************************************************************************
                //// Indicate that the first Ethernet adapter is to be used
                //ecatObj.PortName = "eth0";
                //// Specify an IP address
                //ecatObj.PortName = "192.168.1.1";
                //
                //***************************************************
                //* Initialize the EtherCAT network
                //***************************************************
                //ecatObj.Initialize();
                //
                //***************************************************
                //* Initialize the amplifier
                //***************************************************
                //xAxisAmp.InitializeEcat(ecatObj, ECAT_NODE);
                //

                // read event sticky clears all initial events
                CML_EVENT_STATUS status=0;
                xAxisAmp.ReadEventSticky(ref status);
                statusTextBox.Text = Convert.ToString(status);

                Timer1.Enabled = true;
            }
            catch (Exception ex)
                {
                DisplayError(ex);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try{
                //Read and display actual position 
                posTextBox.Text = Convert.ToString(xAxisAmp.PositionActual);

                //Read and display amplifier status
                CML_EVENT_STATUS status=0;
                xAxisAmp.ReadEventStatus(ref status);
                statusTextBox.Text = Convert.ToString((int)status);
            }
            catch (Exception ex){
                DisplayError(ex);
                Timer1.Stop();
            }
        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            try{
                if (enableButton.Text == "Amp Disable") {
                   //disable the amp
                    xAxisAmp.Disable();
                    enableButton.Text = "Amp Enable";
                }
                else{
                    //enable the amp
                    xAxisAmp.Enable();
                    enableButton.Text = "Amp Disable";
                }
            }
         catch (Exception ex){
                DisplayError(ex);
                Timer1.Stop();
            }
        }

        private void readButton_Click(object sender, EventArgs e)
        {
            try{
                CML_EVENT_STATUS stickyStatus = 0;

                //Read event sticky status
                //Note reading event sticky status automatically clears 
                //events which have been set since last read
                xAxisAmp.ReadEventSticky(ref stickyStatus);
                ArrayList values = new ArrayList();


                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_BRAKE) == CML_EVENT_STATUS.EVENT_STATUS_BRAKE)
                {
                    values.Add("EVENT_STATUS_BRAKE");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_FAULT) == CML_EVENT_STATUS.EVENT_STATUS_FAULT)
                {
                    values.Add("EVENT_STATUS_FAULT");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_NEGATIVE_LIMIT) == CML_EVENT_STATUS.EVENT_STATUS_NEGATIVE_LIMIT)
                {
                    values.Add("EVENT_STATUS_NEGATIVE_LIMIT");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_POSITIVE_LIMIT) == CML_EVENT_STATUS.EVENT_STATUS_POSITIVE_LIMIT)
                {
                    values.Add("EVENT_STATUS_POSITIVE_LIMIT");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_PWM_DISABLE) == CML_EVENT_STATUS.EVENT_STATUS_PWM_DISABLE)
                {
                    values.Add("EVENT_STATUS_PWM_DISABLE");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_SOFTWARE_DISABLE) == CML_EVENT_STATUS.EVENT_STATUS_SOFTWARE_DISABLE)
                {
                    values.Add("EVENT_STATUS_SOFTWARE_DISABLE");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_SOFTWARE_LIMIT_NEGATIVE) == CML_EVENT_STATUS.EVENT_STATUS_SOFTWARE_LIMIT_NEGATIVE)
                {
                    values.Add("EVENT_STATUS_SOFTWARE_LIMIT_NEGATIVE");
                }
                if ((CML_EVENT_STATUS)(stickyStatus & CML_EVENT_STATUS.EVENT_STATUS_SOFTWARE_LIMIT_POSITIVE) == CML_EVENT_STATUS.EVENT_STATUS_SOFTWARE_LIMIT_POSITIVE)
                {
                    values.Add("EVENT_STATUS_SOFTWARE_LIMIT_POSITIVE");
                }
                //Note all Events are not included for complete list see documentation

                ListBox1.DataSource = values;
            }
            catch (Exception ex){
                DisplayError(ex);
                Timer1.Stop();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try{
                xAxisAmp.Disable();
            }
            catch (Exception ex){
                DisplayError(ex);
                Timer1.Stop();
            }
        }


         public void DisplayError(Exception ex)
        {
            DialogResult errormsgbox = DialogResult;
            MessageBox.Show("Error Message: " + ex.Message + "\n" + "Error Source: "
                + ex.Source, "CMO Error", MessageBoxButtons.RetryCancel);
            if (errormsgbox == DialogResult.Cancel)
            {
                // it is possible that this method was called from a thread other than the 
                // GUI thread - if this is the case we must use a delegate to close the application.
                //Dim d As New CloseApp(AddressOf ThreadSafeClose)
                CloseApp d = new CloseApp(ThreadSafeClose);
                this.Invoke(d);
            }
        }

        public void ThreadSafeClose()
        {
            //If the calling thread is different than the GUI thread, then use the
            //delegate to close the application, otherwise call close() directly
            if (this.InvokeRequired)
            {
                CloseApp d = new CloseApp(ThreadSafeClose);
                this.Invoke(d);
            }
            else
                Close();
        }

    }
}
