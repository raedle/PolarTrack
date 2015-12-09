using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Command;

using Huddle.Engine.Data;
using Huddle.Engine.Properties;
using Huddle.Engine.Util;


using Phidgets.Events;
using Phidgets;

namespace Huddle.Engine.Processor.Sensors
{
    [ViewTemplate("Phidget Motor Control", "PhidgetsMotorControl")]
    public class PhidgetsMotorControl : BaseProcessor
    {
        #region private fields
        private MotorControl motorControl = null; //Declare a MotorControl object
        private bool _isRunning = false;
        #endregion

        #region properties

        #region DeviceName
        /// <summary>
        /// The <see cref="DeviceName" /> property's name.
        /// </summary>
        public const string DeviceNamePropertyName = "DeviceName";

        private string _deviceName = "";

        /// <summary>
        /// Sets and gets the DeviceName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DeviceName
        {
            get
            {
                return _deviceName;
            }

            set
            {
                if (_deviceName == value)
                {
                    return;
                }

                RaisePropertyChanging(DeviceNamePropertyName);
                _deviceName = value;
                RaisePropertyChanged(DeviceNamePropertyName);
            }
        }
        #endregion

        #region Velocity
        /// <summary>
        /// The <see cref="Velocity" /> property's name.
        /// </summary>
        public const string VelocityPropertyName = "Velocity";

        // could be 0.416 *100 ?
        //http://www.mercurymotion.com/products/xxmd/mp28m-2838.pdf
        // vmax = 1080; vneed = 30/4*60 = 450; 450/1080 = 0.416;
        private double _velocity = 62.0; // good first test

        /// <summary>
        /// Sets and gets the Velocity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Velocity
        {
            get
            {
                return _velocity;
            }

            set
            {
                if (_velocity == value)
                {
                    return;
                }

                RaisePropertyChanging(VelocityPropertyName);
                _velocity = value;
                RaisePropertyChanged(VelocityPropertyName);
            }
        }
        #endregion

        #region CurrentVelocity
        /// <summary>
        /// The <see cref="CurrentVelocity" /> property's name.
        /// </summary>
        public const string CurrentVelocityPropertyName = "CurrentVelocity";

        // could be 0.416 *100 ?
        //http://www.mercurymotion.com/products/xxmd/mp28m-2838.pdf
        // vmax = 1080; vneed = 30/4*60 = 450; 450/1080 = 0.416;
        private double _currentVelocity = 0.0;

        /// <summary>
        /// Sets and gets the CurrentVelocity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [IgnoreDataMember]
        public double CurrentVelocity
        {
            get
            {
                return _currentVelocity;
            }

            set
            {
                if (_currentVelocity == value)
                {
                    return;
                }

                RaisePropertyChanging(CurrentVelocityPropertyName);
                _currentVelocity = value;
                RaisePropertyChanged(CurrentVelocityPropertyName);
            }
        }
        #endregion

        #endregion

        #region ctor/dtor

        public PhidgetsMotorControl()
        {

        }

        ~PhidgetsMotorControl()
        {
            Stop();
        }

        #endregion


        public override IData Process(IData data)
        {
            return null;
        }

        #region override methods

        public override void Start()
        {
            motorControl = new MotorControl();

            motorControl.Attach += new AttachEventHandler(motorControl_Attach);
            motorControl.EncoderPositionUpdate += new EncoderPositionUpdateEventHandler(motorControl_EncoderPositionUpdate);

            // connect
            motorControl.open(-1);

            _isRunning = true;

            //base.Start();
        }

        public override void Stop()
        {
            if (_isRunning)
            {
                //run any events in the message queue - otherwise close will hang if there are any outstanding events
                //TODO
                // Application.DoEvents();
                motorControl.Attach -= motorControl_Attach;
                motorControl.EncoderPositionUpdate -= motorControl_EncoderPositionUpdate;

                motorControl.motors[0].Velocity = 0;

                motorControl.close();

                motorControl = null;

                _isRunning = false;
            }


            //base.Stop();
        }
        #endregion

        #region private functions
        //MotorControl Attach event handler...populate the fields and controls
        private void motorControl_Attach(object sender, AttachEventArgs e)
        {
            MotorControl attached = (MotorControl)sender;
            DeviceName = attached.Name; ;

            motorControl.motors[0].Velocity = Velocity;

            //attachedTxt.Text = attached.Attached.ToString();
            //nameTxt.Text = attached.Name;
            //serialTxt.Text = attached.SerialNumber.ToString();
            //versiontxt.Text = attached.Version.ToString();
            //numMotorsTxt.Text = attached.motors.Count.ToString();
            //numInputsTxt.Text = attached.inputs.Count.ToString();
            //numEncodersTxt.Text = attached.encoders.Count.ToString();
            //numSensorsTxt.Text = attached.sensors.Count.ToString();

            ////Re-size according to capabilities
            //if (attached.inputs.Count > 0)
            //{
            //    this.Bounds = new Rectangle(this.Location, new Size(this.Width, 585));
            //    digitalInputsGroup.Visible = true;
            //    for (int i = 0; i < attached.inputs.Count; i++)
            //    {
            //        ((CheckBox)digitalInputsGroup.Controls["input" + i + "Chk"]).Visible = true;
            //    }
            //}
            //if (attached.encoders.Count > 0)
            //{
            //    this.Bounds = new Rectangle(this.Location, new Size(this.Width, 641));
            //    encodersGroup.Visible = true;

            //    encoderPosition.Text = motoControl.encoders[0].Position.ToString();
            //}
            //if (attached.sensors.Count > 0)
            //{
            //    this.Bounds = new Rectangle(this.Location, new Size(this.Width, 695));
            //    sensorsGroup.Visible = true;

            //    ratiometricCheck.Checked = motoControl.Ratiometric;
            //}

            ////Set accel range (varies according to controller)
            //accelTrk.SetRange((int)Math.Round(attached.motors[0].AccelerationMin), (int)attached.motors[0].AccelerationMax);
            //accelTrk.TickFrequency = ((int)attached.motors[0].AccelerationMax - (int)Math.Round(attached.motors[0].AccelerationMin)) / 10;
            //accelTrk.Value = accelTrk.Minimum;

            ////Enable everything
            //motorCmb.Enabled = true;
            //velocityTrk.Enabled = true;
            //accelTrk.Enabled = true;

            //if (attached.ID == Phidget.PhidgetID.MOTORCONTROL_1MOTOR)
            //{
            //    backEmfSensingCheck.Enabled = true;
            //    brakingTrk.Enabled = true;
            //}

            //supplyVoltageTimer.Start();

            //for (int i = 0; i < attached.motors.Count; i++)
            //{
            //    motorCmb.Items.Add(i);
            //}

            //motorCmb.SelectedIndex = 0;
        }

        //OnEncoderPositionUpdate(int EncoderIndex, int PositionChange) [event]
        //An event containing position change information for an encoder, which is issued at a set interval of 8ms, regardless of whether the position has changed. This is generally used for PID velocity and/or position control.
        private void motorControl_EncoderPositionUpdate(object sender, EncoderPositionUpdateEventArgs e)
        {
            PID();
        }

        private const double MAXOUTPUT = 100.0;
        private const double DEADBAND = 0.0;
        private const double Kp = 1.0; //proportional control
        private const double Ki = 1.0; //integral control
        private const double Kd = 1.0; //overall gain
        private const double dt = 8.0 / 1000.0;//feedbackPeriod;

        private double integral = 0.0;
        private double derivative = 0.0;
        private double errorLast = 0.0;

        private void PID()
        {
            double output = 0.0;
            double feedback = motorControl.motors[0].Velocity;
            CurrentVelocity = feedback;
            double error = Velocity - feedback;

            integral = integral + (error * dt);
            //derivative = (error - errorLast) / dt;
            derivative = 1.0;

            //Create a dual-sided deadband around the desired value to prevent noisey feedback from producing control jitters
            //This is disabled by setting the deadband values both to zero 
            if (Math.Abs(error) <= DEADBAND)
            {
                error = 0;
                if (Velocity == 0)
                    output = 0;
            }
            else
            {
                output = (Kp * error) + (Ki * integral) + (Kd * derivative);
            }

            //Prevent output value from exceeding maximum output
            if (output >= MAXOUTPUT)
            {
                output = MAXOUTPUT;
            }
            else if (output <= -MAXOUTPUT)
            {
                output = -MAXOUTPUT;
            }
            errorLast = error;

            motorControl.motors[0].Velocity = output;
        }

        ////error handler...display the error description in a messagebox
        //void motoControl_Error(object sender, ErrorEventArgs e)
        //{
        //    Phidget phid = (Phidget)sender;
        //    DialogResult result;
        //    switch (e.Type)
        //    {
        //        case PhidgetException.ErrorType.PHIDGET_ERREVENT_BADPASSWORD:
        //            phid.close();
        //            TextInputBox dialog = new TextInputBox("Error Event",
        //                "Authentication error: This server requires a password.", "Please enter the password, or cancel.");
        //            result = dialog.ShowDialog();
        //            if (result == DialogResult.OK)
        //                openCmdLine(phid, dialog.password);
        //            else
        //                Environment.Exit(0);
        //            break;
        //        case PhidgetException.ErrorType.PHIDGET_ERREVENT_PACKETLOST:
        //            //Ignore this error - it's not useful in this context.
        //            return;
        //        default:
        //            if (!errorBox.Visible)
        //                errorBox.Show();
        //            break;
        //    }
        //    errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + e.Description);
        //}
        #endregion
    }
}
