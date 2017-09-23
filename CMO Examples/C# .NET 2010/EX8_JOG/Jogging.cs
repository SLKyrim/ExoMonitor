﻿//
// Example 8 Jogging Rev 2
//
// This program demonstrates how to perform jogging moves.
//
// This program assumes the following axis configuration:
// 1. Upon startup it will enable axis at Can Node ID 1 or
//    an axis at EtherCAT Node ID -1.
//
// This code also includes the following prerequisites:
// 1. The amplifier and motor must be preconfigured and set up properly to run.
// 2. The hardware enable switch must be installed and easily accessible
// 
// As with any motion product extreme caution must be used! Read and understand
// all parameter settings before attemping to send to amplifier.
// 
// 
// Copley Motion Objects are Copyright, 2006-2015, Copley Controls.
//
// For more information on Copley Motion products see:
// http://www.copleycontrols.com
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CMLCOMLib;

namespace EX8_JOG
{
    public partial class Jogging : Form
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
        const int CAN_ADDRESS = 1;
        canOpenObj canOpen;

        AmpObj ampObj;
        ProfileSettingsObj ProfileParameters;
        // Create a delegate to close down the application in a thread safe way
        delegate void CloseApp(); 
      
        public Jogging()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ampObj = new AmpObj();
                ProfileParameters = new ProfileSettingsObj();

                //Code to replace vb handle statements
                JogPosButton.MouseUp += JogPosButton_MouseUp;
                JogPosButton.MouseDown += JogPosButton_MouseDown;
                JogNegButton.MouseUp += JogNegButton_MouseUp;
                JogNegButton.MouseDown += JogNegButton_MouseDown;

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
                ampObj.Initialize(canOpen, CAN_ADDRESS);


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
                //ampObj.InitializeEcat(ecatObj, ECAT_NODE);
                //


                // Read velocity loop settings from the amp, use these as reasonble starting
                // points for the trajectory limits.
                VelocityTextBox.Text = Convert.ToString((ampObj.VelocityLoopSettings.VelLoopMaxVel) / 10);
                AccelTextBox.Text = Convert.ToString((ampObj.VelocityLoopSettings.VelLoopMaxAcc) / 10);
                DecelTextBox.Text = Convert.ToString((ampObj.VelocityLoopSettings.VelLoopMaxDec) / 10);

                //Selects profile type
                ProfileParameters.ProfileType = CML_PROFILE_TYPE.PROFILE_VELOCITY;

                Timer1.Start();
            }

            catch (Exception ex)
            {
                DisplayError(ex);
            }
        }
       
       private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //Reads actual position information from the amplifier
                ActPosVar.Text = Convert.ToString(ampObj.PositionActual);
            }
            catch (Exception ex)
            {
                Timer1.Stop();
                DisplayError(ex);
            }
        }
        
        //Code for button with mouse pressed
        private void JogPosButton_MouseDown(System.Object eventSender, System.Windows.Forms.MouseEventArgs eventArgs)// Handles JogPosButton.MouseDown
        {
            try
            {
                //Sets velocity, acceleration, deceleration limits for move to inputed values
                ProfileParameters.ProfileAccel = Convert.ToDouble(AccelTextBox.Text);
                ProfileParameters.ProfileDecel = Convert.ToDouble(DecelTextBox.Text);
                ProfileParameters.ProfileVel = Convert.ToDouble(VelocityTextBox.Text);

                //Store inputed parameters as the profile settings
                ampObj.ProfileSettings = ProfileParameters;

                //Performs positive move. 1 indicates positive move, -1 indicates negative move
                ampObj.MoveAbs(1);
            }
            catch(Exception ex)
            {
                DisplayError(ex);
            }

    }

         //Code for button with mouse depressed
        private void JogPosButton_MouseUp(System.Object eventSender, System.Windows.Forms.MouseEventArgs eventArgs)//Handles JogPosButton.MouseUp
        {   
            try
            {
                //Sets profile velocity to zero
                ProfileParameters.ProfileVel = 0;

                //Store inputed parameters as the profile settings
                ampObj.ProfileSettings = ProfileParameters;

                //Performs move
                ampObj.MoveAbs(1);
            }
            catch(Exception ex)
            {
            DisplayError(ex);
            }

        }


         //Code for button with mouse pressed
        private void JogNegButton_MouseDown(System.Object eventSender, System.Windows.Forms.MouseEventArgs eventArgs)// Handles JogNegButton.MouseDown
        {
            try
            {
                //Sets velocity, acceleration, deceleration limits for move to inputed values
                ProfileParameters.ProfileAccel = Convert.ToDouble(AccelTextBox.Text);
                ProfileParameters.ProfileDecel = Convert.ToDouble(DecelTextBox.Text);
                ProfileParameters.ProfileVel = Convert.ToDouble(VelocityTextBox.Text);

                //Store inputed parameters as the profile settings
                ampObj.ProfileSettings = ProfileParameters;

                //Performs negative move
                ampObj.MoveAbs(-1);
            }
            catch(Exception ex)
            {
                DisplayError(ex);
            }
        }

         //Code for button with mouse depressed
        private void JogNegButton_MouseUp(System.Object eventSender, System.Windows.Forms.MouseEventArgs eventArgs)// Handles JogNegButton.MouseUp
        {
            try
            {
            //Sets profile velocity to zero
            ProfileParameters.ProfileVel = 0;

            //Store inputed parameters as the profile settings
            ampObj.ProfileSettings = ProfileParameters;

            //Performs move
            ampObj.MoveAbs(1);
            }
            catch(Exception ex)
            {
            DisplayError(ex);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ampObj.Disable();
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