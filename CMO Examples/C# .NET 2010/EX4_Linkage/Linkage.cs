﻿//  Example 4 Linkage Rev 3
//
// This program demonstrates linkage moves (coordinated motion on multiple axes)
//
// The program allows the target positions for two axis to be set and 
// the user can then perform a linkage (coordinated) move
// to that set of positions.
//
// This program assumes the following axis configuration:
// 1. Upon startup it will enable one axis at Can Node ID 1 and
//    one at Can Node ID 2, or one axis at EtherCAT Node -1 and
//    one at EtherCAT node -2.
// 2. Both motors have an encoders with an index
//
//
// This code also includes the following prerequisites:
// 1. The both amplifiers and motors must be preconfigured and set up properly to run.
// 2. The hardware enable switch must be installed on both amplifiers 
// and both are easily accessible
// 
// As with any motion product extreme caution must used! Read and understand
// all parameter settings before attempting to send to amplifier.
// 
// Copley Motion Objects are Copyright, 2002-2015, Copley Controls.
//
// For more information on Copley Motion products see:
// http://www.copleycontrols.com
//
// NOTE: Must home each time before the linkage move.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMLCOMLib;

namespace EX4_Linkage
{
    public partial class Linkage : Form
    {
        //***************************************************
        //*
        //*  EtherCAT Network
        //*
        //***************************************************
        //// A negative node number refers to the drive's physical position on the network
        //// -1 for the first drive, -2 for the second, etc.
        //const int X_AXIS_ECAT_NODE = -1;
        //const int Y_AXIS_ECAT_NODE = -2;
        //EcatObj ecatObj;

        //***************************************************
        //
        //  CANOpen Network
        //
        //***************************************************
        const int X_AXIS_CAN_ADDRESS = 1;
        const int Y_AXIS_CAN_ADDRESS = 2;
        canOpenObj canOpen;

        LinkageObj Link;
        LinkageSettingsObj linkageSettings;

        AmpObj[] ampArray = new AmpObj[2];

        const short X_AXIS_AMP = 0;
        const short Y_AXIS_AMP = 1;


        HomeSettingsObj Home;

        // Create a delegate to close down the application in a thread safe way
        delegate void CloseApp();
        
            public Linkage()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try{
            //Initialize code here

            ampArray[X_AXIS_AMP] = new AmpObj();
            ampArray[Y_AXIS_AMP] = new AmpObj();
            Link = new LinkageObj();

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
            ampArray[X_AXIS_AMP].Initialize(canOpen, X_AXIS_CAN_ADDRESS);
            ampArray[Y_AXIS_AMP].Initialize(canOpen, Y_AXIS_CAN_ADDRESS);


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
            //ampArray[X_AXIS_AMP].InitializeEcat(ecatObj, X_AXIS_ECAT_NODE);
            //ampArray[Y_AXIS_AMP].InitializeEcat(ecatObj, Y_AXIS_ECAT_NODE);
            //
            //***************************************************
            //* Initialize both axis of a 2 axis EtherCAT drive (AE2, XE2, etc)
            //***************************************************
            //// Initialize the first axis. 2 axis etherCAT drives are treated
            //// like a single node on the EtherCAT bus, so only one ecat node ID
            //// is used.
            //ampArray[X_AXIS_AMP].InitializeEcat(ecatObj, X_AXIS_ECAT_NODE);
            //// Initalize the second axis by passing in the previously initialized
            //// x axis amp object as a paramter
            //ampArray[Y_AXIS_AMP].InitializeEcatSubAxis(ampArray[X_AXIS_AMP]);



            Link.Initialize(ampArray);

            //***************************************************
            //* An alternate way of initializing the Linkage object is to set up the 
            //* LinkageSettingsObj then pass it into the InitializeExt method of the LinkageObj. 
            //* To use it, first comment out the call to the LinkageObj's Initialize method 
            //* above, then uncomment the following three lines of code.
            //**************************************************************************
            //// Create an instance of the AmpSettings object by calling the constructor 
            //// (all of the properties will be initialized to their default values).
            //linkageSettings = new LinkageSettingsObj();
            //// Change a property
            //linkageSettings.moveAckTimeout = 400;
            //// Initialize the AmpObj with the settings object
            //Link.InitializeExt(ampArray, linkageSettings);

            }
            catch (Exception ex){
                DisplayError(ex);
            }


         }

        private void linkageMoveButton_Click(object sender, EventArgs e)
        {
            try{

            linkageMoveButton.Enabled = false;
            HomeAxisButton.Enabled = false;
            double[] positionArray = new double[2]; //Set up array
            positionArray[0] = Convert.ToDouble(xAxisTextBox.Text);
            positionArray[1] = Convert.ToDouble(yAxisTextBox.Text);

            //Get move values to be used by linkage 
            double velocity = (ampArray[X_AXIS_AMP].VelocityLoopSettings.VelLoopMaxVel) / 10;
            double accel = (ampArray[X_AXIS_AMP].VelocityLoopSettings.VelLoopMaxAcc) / 15;
            double decel = ampArray[X_AXIS_AMP].VelocityLoopSettings.VelLoopMaxDec / 10;
            double jerk = accel;

            //Set the Linkage move limits( all values must be non zero)
            Link.SetMoveLimits(velocity, accel, decel, jerk);

            //Do linkage move
            Link.MoveTo(positionArray);

            Link.WaitMoveDone(10000);

            linkageMoveButton.Enabled = true;
            HomeAxisButton.Enabled = true;

            }
            catch (Exception ex){
            linkageMoveButton.Enabled = true;
            HomeAxisButton.Enabled = true;
            DisplayError(ex);
            }

        }

        private void HomeAxisButton_Click(object sender, EventArgs e)
        {
            try{

            linkageMoveButton.Enabled = false;
            HomeAxisButton.Enabled = false;

            Home = ampArray[X_AXIS_AMP].HomeSettings;
            Home.HomeVelFast = (ampArray[X_AXIS_AMP].VelocityLoopSettings.VelLoopMaxVel) / 10;
            Home.HomeVelSlow = (ampArray[X_AXIS_AMP].VelocityLoopSettings.VelLoopMaxVel) / 15;
            Home.HomeAccel = ampArray[X_AXIS_AMP].VelocityLoopSettings.VelLoopMaxAcc / 10;
            Home.HomeMethod = CML_HOME_METHOD.CHOME_INDEX_POSITIVE;
            Home.HomeOffset = -100000;
            ampArray[X_AXIS_AMP].HomeSettings = Home;
            ampArray[X_AXIS_AMP].GoHome();
            ampArray[X_AXIS_AMP].WaitMoveDone(10000);


            Home = ampArray[Y_AXIS_AMP].HomeSettings;
            Home.HomeVelFast = (ampArray[Y_AXIS_AMP].VelocityLoopSettings.VelLoopMaxVel) / 10;
            Home.HomeVelSlow = (ampArray[Y_AXIS_AMP].VelocityLoopSettings.VelLoopMaxVel) / 15;
            Home.HomeAccel = ampArray[Y_AXIS_AMP].VelocityLoopSettings.VelLoopMaxAcc / 10;
            Home.HomeMethod = CML_HOME_METHOD.CHOME_INDEX_NEGATIVE;
            Home.HomeOffset = -1000;
            ampArray[Y_AXIS_AMP].HomeSettings = Home;
            ampArray[Y_AXIS_AMP].GoHome();
            ampArray[Y_AXIS_AMP].WaitMoveDone(10000);

            HomeAxisButton.Enabled = true;

            linkageMoveButton.Enabled = true;
            }
        catch (Exception ex){
            HomeAxisButton.Enabled = true;
            DisplayError(ex);
        }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try{

            ampArray[Y_AXIS_AMP].Disable();
            ampArray[X_AXIS_AMP].Disable();
            }
            catch (Exception ex){
            DisplayError(ex);
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