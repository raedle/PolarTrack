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

        #region RPM
        /// <summary>
        /// The <see cref="RPM" /> property's name.
        /// </summary>
        public const string RPMPropertyName = "RPM";

        private static double _rpm = 62.0; // good first test

        /// <summary>
        /// Sets and gets the RPM property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double RPM
        {
            get
            {
                return _rpm;
            }

            set
            {
                if (_rpm == value)
                {
                    return;
                }

                RaisePropertyChanging(RPMPropertyName);
                _rpm = value;
                RaisePropertyChanged(RPMPropertyName);
            }
        }
        #endregion

        #region Kp
        /// <summary>
        /// The <see cref="Kp" /> property's name.
        /// </summary>
        public const string KpPropertyName = "Kp";

        private double _Kp = 0.1; //proportional control

        /// <summary>
        /// Sets and gets the Kp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Kp
        {
            get
            {
                return _Kp;
            }

            set
            {
                if (_Kp == value)
                {
                    return;
                }

                RaisePropertyChanging(KpPropertyName);
                _Kp = value;
                RaisePropertyChanged(KpPropertyName);
            }
        }
        #endregion

        #region Ki
        /// <summary>
        /// The <see cref="Ki" /> property's name.
        /// </summary>
        public const string KiPropertyName = "Ki";

        private double _Ki = 0.1; //integral control

        /// <summary>
        /// Sets and gets the Ki property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Ki
        {
            get
            {
                return _Ki;
            }

            set
            {
                if (_Ki == value)
                {
                    return;
                }

                RaisePropertyChanging(KiPropertyName);
                _Ki = value;
                RaisePropertyChanged(KiPropertyName);
            }
        }
        #endregion

        #region Kd
        /// <summary>
        /// The <see cref="Kd" /> property's name.
        /// </summary>
        public const string KdPropertyName = "Kd";

        private double _Kd = 0.001; //overall gain

        /// <summary>
        /// Sets and gets the Kd property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Kd
        {
            get
            {
                return _Kd;
            }

            set
            {
                if (_Kd == value)
                {
                    return;
                }

                RaisePropertyChanging(KdPropertyName);
                _Kd = value;
                RaisePropertyChanged(KdPropertyName);
            }
        }
        #endregion

        #region delta
        /// <summary>
        /// The <see cref="delta" /> property's name.
        /// </summary>
        public const string deltaPropertyName = "delta";

        private static double _delta = 0.0; 

        /// <summary>
        /// Sets and gets the delta property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double delta
        {
            get
            {
                return _delta;
            }

            set
            {
                if (_delta == value)
                {
                    return;
                }

                RaisePropertyChanging(deltaPropertyName);
                _delta = value;
                RaisePropertyChanged(deltaPropertyName);

                RaisePropertyChanged(TargetPropertyName);
            }
        }
        #endregion

        #region Target
        /// <summary>
        /// The <see cref="Target" /> property's name.
        /// </summary>
        public const string TargetPropertyName = "Target";

        private static double _target = 7.5; // good first test

        /// <summary>
        /// Sets and gets the Target property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Target
        {
            get
            {
                return _target + _delta;
            }

            set
            {
                if (_target == value)
                {
                    return;
                }

                RaisePropertyChanging(TargetPropertyName);
                _target = value;
                delta = 0.0;
                RaisePropertyChanged(TargetPropertyName);
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

        #region RPM_ON_OUTER_AXIS
        /*
         * 3.7 * 360 
         * Übersetzung * Ticks vom encoder/Umdrehung
         */
        public const int TICKS_PER_TURN_ON_OUTER_AXIS = 1332;
        #endregion

        #region GearRatio
        /// <summary>
        /// The <see cref="GearRatio" /> property's name.
        /// </summary>
        public const string GearRatioPropertyName = "GearRatio";

        private int _gearRatio = 1;

        /// <summary>
        /// Sets and gets the GearRatio property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int GearRatio
        {
            get
            {
                return _gearRatio;
            }

            set
            {
                if (_gearRatio == value)
                {
                    return;
                }

                RaisePropertyChanging(GearRatioPropertyName);
                _gearRatio = value;
                RaisePropertyChanged(GearRatioPropertyName);
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

        private System.Timers.Timer aTimer = new System.Timers.Timer();
        private System.Timers.Timer pidTimer = new System.Timers.Timer();
        private double pidInterval = 200;

        public override void Start()
        {
            motorControl = new MotorControl();

            motorControl.Attach += new AttachEventHandler(motorControl_Attach);
            motorControl.EncoderPositionUpdate += new EncoderPositionUpdateEventHandler(motorControl_EncoderPositionUpdate);

            // connect
            motorControl.open(-1);

            _isRunning = true;

            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;

            pidTimer.Elapsed += new System.Timers.ElapsedEventHandler(pidEvent);
            pidTimer.Interval = pidInterval;
            pidTimer.Enabled = false;
            //base.Start();

            //disable Depth images
            Senz3DSoftKinetic.getInstance().IsUseDepthNode = false;

            //listen to properties
            PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case VelocityPropertyName:
                        // motorControl.motors[0].Velocity = Velocity;
                        break;
                }
            };
        }

        public override void Stop()
        {
            if (_isRunning)
            {
                aTimer.Enabled = false;
                pidTimer.Enabled = false;
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
        }

        //OnEncoderPositionUpdate(int EncoderIndex, int PositionChange) [event]
        //An event containing position change information for an encoder, which is issued at a set interval of 8ms, regardless of whether the position has changed. This is generally used for PID velocity and/or position control.
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private int cnt = 0;
        private int cma = 0;

        /*
         * http://www.phidgets.com/docs/1065_User_Guide
         * OnEncoderPositionUpdate(int EncoderIndex, int PositionChange) [event]
         * An event containing position change information for an encoder, which is issued at a set interval of 8ms, regardless of whether the position has changed. This is generally used for PID velocity and/or position control.
         */
        private int totalTicks = 0;
        private int pidTicks = 0;
        private void motorControl_EncoderPositionUpdate(object sender, EncoderPositionUpdateEventArgs e)
        {
            if (!sw.IsRunning)
            {
                sw.Start();
            }
            sw.Stop();
            long diff = sw.ElapsedMilliseconds;
            cma += (e.PositionChange - cma) / ((cnt++) + 1);
            //System.Console.WriteLine("{0}, {1}, {2}", e.PositionChange, diff, cma);
            PID(e.PositionChange);
            sw.Reset();
            sw.Start();

            totalTicks += e.PositionChange;
            pidTicks += e.PositionChange;
        }

        private const double MAXOUTPUT = 100.0;
        private const double DEADBAND = 0.0;
        private const double dt = 8.0;//feedbackPeriod;

        private double integral = 0.0;
        private double derivate = 0.0;
        private double errorLast = 0.0;


        private void PID(double steps = 0.0, double _dt = dt)
        {
            double output = 0.0;
            CurrentVelocity = motorControl.motors[0].Velocity;
            double target = (TICKS_PER_TURN_ON_OUTER_AXIS * Target * GearRatio) / (1000.0 / _dt);
            double error = target - steps;

            integral += error;
            derivate = error - errorLast;
            errorLast = error;

            output = (_Kp * error) + (_Ki * integral) + (_Kd * derivate);

            //Prevent output value from exceeding maximum output
            if (output >= MAXOUTPUT)
            {
                output = MAXOUTPUT;
            }
            else if (output <= -MAXOUTPUT)
            {
                output = -MAXOUTPUT;
            }

            motorControl.motors[0].Velocity = output;
        }

        DateTime last = DateTime.Now;
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            RPM = (totalTicks * 1.0) / (TICKS_PER_TURN_ON_OUTER_AXIS * GearRatio);
            totalTicks = 0;
            last = DateTime.Now;
        }

        private void pidEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            PID(pidTicks, pidInterval);
            pidTicks = 0;
        }

        #endregion
    }
}
