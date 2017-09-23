//
// Example 3 Basic Moves Rev 3
//
// This program demonstrates how to perform basic trap profile move.
//
// This program demonstrates the following types of motion:
// 1. Trapezoidal move 
// 2. Halt move
//
// This program assumes the following axis configuration:
// 1. Upon startup it will enable axis at Can Node ID 1, or
// as the first drive on the EtherCAT network, using the first
// available EtherCAT port, "eth0".
// 2. The motor has an encoder with an index
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
using CMLCOMLib;

namespace EX3_BasicMoves
{
    public partial class BasicMoves : Form
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
        ProfileSettingsObj ProfileSettings;
        HomeSettingsObj Home;


        // Create a delegate to close down the application in a thread safe way
        delegate void CloseApp();


        public BasicMoves()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
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

                // Read velocity loop settings from the amp, use these as reasonble starting
                // points for the trajectory limits.
                VelocityTextBox.Text = Convert.ToString((xAxisAmp.VelocityLoopSettings.VelLoopMaxVel) / 10);
                AccelTextBox.Text = Convert.ToString((xAxisAmp.VelocityLoopSettings.VelLoopMaxAcc) / 10);
                DecelTextBox.Text = Convert.ToString((xAxisAmp.VelocityLoopSettings.VelLoopMaxDec) / 10);

                // Initialize home object with amplifier home settings 
                Home = xAxisAmp.HomeSettings;

                Timer1.Start();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
        }

        private void HomeAxisButton_Click(object sender, EventArgs e)
        {
            try
            {

            HomeAxisButton.Enabled = false;

            Home.HomeVelFast = (xAxisAmp.VelocityLoopSettings.VelLoopMaxVel) / 10;
            Home.HomeVelSlow = (xAxisAmp.VelocityLoopSettings.VelLoopMaxVel) / 15;
            Home.HomeAccel = xAxisAmp.VelocityLoopSettings.VelLoopMaxAcc / 10;
            Home.HomeMethod = CML_HOME_METHOD.CHOME_INDEX_POSITIVE;
            Home.HomeOffset = -1000;
            xAxisAmp.HomeSettings = Home;
            xAxisAmp.GoHome();
            xAxisAmp.WaitMoveDone(10000);

            doMoveButton.Enabled = true;

            HomeAxisButton.Enabled = true;
            }
        catch(Exception ex)
            {
            HomeAxisButton.Enabled = true;
            DisplayError(ex);
            }
        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (enableButton.Text == "Amp Disable")
                {
                    xAxisAmp.Disable();
                    enableButton.Text = "Amp Enable";
                }
                else
                {
                    xAxisAmp.Enable();
                    enableButton.Text = "Amp Disable";
                }
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
                //Read and display actual position 
                posTextBox.Text = Convert.ToString(xAxisAmp.PositionActual);
            }
            catch (Exception ex)
            {
                Timer1.Stop();
                DisplayError(ex);
            }

        }

        private void doMoveButton_Click(object sender, EventArgs e)
        {
            try
            {
                Double Distance;
                Distance = Convert.ToDouble(DistanceTextBox.Text);

                ProfileSettings = xAxisAmp.ProfileSettings; // read profile settings from amp

                //Set the profile type for move
                ProfileSettings.ProfileType = CML_PROFILE_TYPE.PROFILE_TRAP;

                //Set the profile trajectory values 
                ProfileSettings.ProfileAccel = Convert.ToDouble(AccelTextBox.Text);
                ProfileSettings.ProfileDecel = Convert.ToDouble(DecelTextBox.Text);
                ProfileSettings.ProfileVel = Convert.ToDouble(VelocityTextBox.Text);

                // Update the amplier's profile settigns
                xAxisAmp.ProfileSettings = ProfileSettings;

                // Execute the move
                xAxisAmp.MoveRel(Distance);
                }
            catch (Exception ex)
            {
                doMoveButton.Enabled = true;
                HomeAxisButton.Enabled = true;
                DisplayError(ex);
            }
        }

        private void haltMoveButton_Click(object sender, EventArgs e)
        {
            try
            {

            doMoveButton.Enabled = false;
            HomeAxisButton.Enabled = false;

            //set halt mode desired before halting the move
            xAxisAmp.HaltMode = CML_HALT_MODE.HALT_DECEL;
            // now halt the move
            xAxisAmp.HaltMove();

            doMoveButton.Enabled = true;
            HomeAxisButton.Enabled = true;
            }
        catch (Exception ex)
            {
            doMoveButton.Enabled = true;
            HomeAxisButton.Enabled = true;
            DisplayError(ex);
        }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            xAxisAmp.Disable();
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

