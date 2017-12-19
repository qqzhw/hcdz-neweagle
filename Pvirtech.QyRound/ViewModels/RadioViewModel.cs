using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Pvirtech.QyRound.Commons;
using Pvirtech.QyRound.Core.Common;
using Pvirtech.QyRound.Models;
using Pvirtech.QyRound.Properties;
using Pvirtech.QyRound.SDK;
using Pvirtech.TcpSocket.Scs.Client;
using Pvirtech.TcpSocket.Scs.Communication;
using Pvirtech.TcpSocket.Scs.Communication.EndPoints.Tcp;
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Pvirtech.QyRound.ViewModels
{
   public  class RadioViewModel: BindableBase
    {
        public IScsClient Client { get; set; }
        private readonly IEventAggregator _eventAggregator;
        public RadioViewModel(  IEventAggregator eventAggregator)
        {
            Init();
            InitModel();
            string temp_time = "20091225091010";
            string results = DateTime.ParseExact(temp_time, "yyyyMMddHHmmss", null).ToString("yyyy-MM-dd HH:mm:ss");
            ChannelCmd = new DelegateCommand<object>(OnChannnelCtrl);
            CorrectCmd= new DelegateCommand<object>(OnCorrectCtrl);
            ControlCmd = new DelegateCommand<object>(OnRateCtrl);
            _eventAggregator = eventAggregator;
        }

        private void OnRateCtrl(object obj)
        {
            PLControl();//频率控制
        }

        /// <summary>
        /// 标校控制
        /// </summary>
        /// <param name="obj"></param>
        private void OnCorrectCtrl(object obj)
        {
            bool single = false;
            if (SelectSignalForm.Name == "单载波")
            {
                single = true;
            }
            CorrectEnd = CorrectBegin;//设置起始频率， 结束频率相等
            double ATTValue = Correctrange;  
            bool isAnswer = true;// isNeedReadBack;
            double startFreq = CorrectBegin;
            double stopFreq = CorrectEnd;
            double stepFreq = 10;// Correctstep;
            int time = 0;// Correcttime * 1000;
            int dd = 1;
            if (stepFreq == 0.0)
            {
                dd = (int)Math.Ceiling((stopFreq - startFreq) / stepFreq) + 1;
            }
            for (int i = 0; i < dd; i++)
            {
                double outputFreq = startFreq + (double)i * stepFreq;
                if (outputFreq > stopFreq)
                {
                    outputFreq = stopFreq;
                }
                SetCalibration(isAnswer, single, outputFreq, ATTValue);
                outputFreq += Correctstep;
            }

        }

        private void OnChannnelCtrl(object obj)
        {
            SetChannelIFBWgain(ChannelNo, true, SelectedSignal.Id, SelectedKd.Id, Spzy, CenterZy1, CenterZy2);
        }
        /// <summary>
        /// 频率控制
        /// </summary>
        public void PLControl()
        {
            ControlEnd = ControlBegin;//设置频率相等
            double startFreq =ControlBegin;//开始频率 
            double stopFreq = ControlEnd;
            var stepFreq = ControlStep;
            int time =Controltime*1000;
            int dd = 1;
            if (stepFreq != 0.0)
            {
                dd = (int)Math.Ceiling((stopFreq - startFreq) / stepFreq) + 1;
                if (stopFreq < 60.0 || startFreq < 60.0 || stepFreq == 0.0)
                {
                    // MessageHelper.ReportMessage("频率设置错误！");
                }
            }
            for (int i = 0; i < dd; i++)
            {
                double outPutfre = startFreq + (double)i * stepFreq;
                if (outPutfre > stopFreq)
                {
                    outPutfre = stopFreq;
                }
                // bool isAnswer = this.rbFreYes.IsChecked.Value;
                CtrlFreq(true, outPutfre);
                //  MessageHelper.ReportMessage(string.Format("频率{0}控制完成。", outPutfre));
                Thread.Sleep(time);
            }
        }
        public bool CtrlFreq(bool isAnswer, double freq)
        {            
            try
            {
                byte[] sendByte = new byte[16];
                sendByte[0] = 85;
                sendByte[1] = 170;
                sendByte[2] = 16;
                sendByte[3] = 2;
                sendByte[4] = 0;
                if (isAnswer)
                {
                    sendByte[4] = 1;
                }
                freq = (freq - 30.0) * 100.0 + 1.0;
                byte[] freqByte = BitConverter.GetBytes((int)freq);
                for (int i = 0; i < 4; i++)
                {
                    sendByte[5 + i] = freqByte[3 - i];
                }
                byte[] dd = this.Getbytes(sendByte);
                sendByte[14] = dd[1];
                sendByte[15] = dd[0];
                byte[] readByte = new byte[16];
                
                if (Client.CommunicationState == TcpSocket.Scs.Communication.CommunicationStates.Connected)
                {
                    var text = new ScsRawDataMessage(sendByte);
                    Client.SendMessage(text);
                }
                else
                {
                    MessageBox.Show("设备已断开连接！");
                }
                return true;
            }
            catch 
            {
                _eventAggregator.GetEvent<InfoEventArgs>().Publish("指令下发出现异常!\n");
            }
            return true;
        }

        /// <summary>
        /// 通道控制
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="isAnswer"></param>
        /// <param name="IsCalibrationSignal"></param>
        /// <param name="IFBWNumber"></param>
        /// <param name="GainRF"></param>
        /// <param name="GainIF1"></param>
        /// <param name="GainIF2"></param>
        /// <returns></returns>
        public bool SetChannelIFBWgain(int channel, bool isAnswer, int IsCalibrationSignal, int IFBWNumber, double GainRF, double GainIF1, double GainIF2)
        {
            byte[] sendByte = new byte[16];
            sendByte[0] = 85;
            sendByte[1] = 170;
            sendByte[2] = 16;
            sendByte[3] = 1;
            if (isAnswer)
            {
                sendByte[4] = 1;
            }
            sendByte[5] = (byte)channel;
            int bitynumber = IsCalibrationSignal;
             
            if (IFBWNumber == 1)
            {
                bitynumber += 2;
            }
            sendByte[6] = (byte)bitynumber;
            sendByte[7] = (byte)GainRF;
            sendByte[8] = (byte)GainIF1;
            sendByte[9] = (byte)GainIF2;
            byte[] dd = this.Getbytes(sendByte);
            sendByte[14] = dd[1];
            sendByte[15] = dd[0];
            byte[] readByte = new byte[16];
            int count = 0;
            var text = new ScsRawDataMessage(sendByte);
            if (Client.CommunicationState == TcpSocket.Scs.Communication.CommunicationStates.Connected)
            {
                Client.SendMessage(text);
            }
            else
            {
                MessageBox.Show("射频TCP网络连接已断开！");
            }             
            return true;
        }

        /// <summary>
        /// 标校控制
        /// </summary>
        /// <param name="isAnswer"></param>
        /// <param name="IsSingleSignal"></param>
        /// <param name="outputFreq"></param>
        /// <param name="ATTValue"></param>
        /// <returns></returns>
        public bool SetCalibration(bool isAnswer, bool IsSingleSignal, double outputFreq, double ATTValue)
        {
            byte[] sendByte = new byte[16];
            sendByte[0] = 85;
            sendByte[1] = 170;
            sendByte[2] = 16;
            sendByte[3] = 3;
            if (isAnswer)
            {
                sendByte[4] = 1;
            }
            if (!IsSingleSignal)
            {
                sendByte[5] = 1;
            }
            else
            {
                sendByte[5] = 0;
            }
            outputFreq = (outputFreq - 30.0) * 0.1 + 1.0;
            byte[] ads = BitConverter.GetBytes((int)outputFreq);
            sendByte[6] = ads[1];
            sendByte[7] = ads[0];
            // sendByte[8] = (byte)(ATTValue - 1.0);
            sendByte[8] =(byte) ATTValue;
            byte[] dd = this.Getbytes(sendByte);
            sendByte[14] = dd[1];
            sendByte[15] = dd[0];
            byte[] readByte = new byte[16];           
            if (Client.CommunicationState == TcpSocket.Scs.Communication.CommunicationStates.Connected)
            {
                var text = new ScsRawDataMessage(sendByte);
                Client.SendMessage(text);
            }
            else
            {
                MessageBox.Show("设备已断开连接！");
            }
            bool result=true;
           
            return result;
        }

        private void InitModel()
        {
            _signals = new ObservableCollection<Signal>();
            var signal = new Signal() { Id = 0x00, Name = "阵列信号" };
            _signals.Add(signal);
            _signals.Add(new Signal() { Id = 0x01, Name = "标校信号" });
            SelectedSignal = signal;
            _bandWidths = new ObservableCollection<BandWidth>();
            var bandWidth = new BandWidth() { Id = 0x00, Name = "2MHz" };
            SelectedKd = bandWidth;
            _bandWidths.Add(bandWidth);
            _bandWidths.Add(new BandWidth() { Id = 0x01, Name = "60MHz" });
            _signalForms = new ObservableCollection<SignalForm>();
            var signalform = new SignalForm() { Id = 0x00, Name = "梳状谱" };
            SelectSignalForm = signalform;
            _signalForms.Add(signalform);
            _signalForms.Add(new SignalForm() { Id = 0x01, Name = "单载波" });
        }

        private void Init()
        {
            _spIp = Settings.Default.SpIP;
            _spPort = Settings.Default.SpPort;
            Client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(SpIP, SpPort));
        }
        public void Connect()
        {
            try
            {

                //Create a client object to connect a server on 127.0.0.1 (local) IP and listens 10085 TCP port
                if (IsConnected)
                {
                    return;
                }
                Client.WireProtocol = new CustomWireProtocol();
                Client.ConnectTimeout = 5;
                Client.MessageReceived += OnMessageReceived;
                Client.Connected += OnConnected;
                Client.Disconnected += OnDisconnected;
                Client.Connect(); //Connect to the server  
                //Send message to the server
               // Client.SendMessage(new ScsTextMessage("3F", "q1"));

                //client.Disconnect(); //Close connection to server
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Client.Dispose();
                LogHelper.ErrorLog(ex, "射频Tcp网络连接异常!");
            }
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            IsConnected = false;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            var client = sender as IScsClient;
            if (client.CommunicationState == CommunicationStates.Connected)
            {
                IsConnected = true;
            }

        }
        public bool MonitorSystemSelfCal()
        { 
            List<string> aaa = new List<string>();
            byte[] sendByte = new byte[16];
            sendByte[0] = 85;
            sendByte[1] = 170;
            sendByte[2] = 16;
            sendByte[3] = 5; 
             sendByte[4] = 1; 
            byte[] dd = Getbytes(sendByte);
            sendByte[14] = dd[1];
            sendByte[15] = dd[0];
            byte[] readByte = new byte[16];
          
            var text = new ScsRawDataMessage(sendByte, "sp_checkself");
            if (Client.CommunicationState == CommunicationStates.Connected)
            {
                Client.SendMessage(text);                
            }
            else
            {
                _eventAggregator.GetEvent<InfoEventArgs>().Publish("射频TCP网络连接异常！\n");                 
            } 
            return true;
        }
        private byte[] Getbytes(byte[] data)
        {
            byte[] sendBytes = new byte[2];
            int tmpValue = (int)this.InitialValue;
            for (int i = 0; i < data.Length - 2; i++)
            {
                tmpValue ^= (int)data[i];
                for (int j = 0; j < 8; j++)
                {
                    if (1 == (tmpValue & 1))
                    {
                        tmpValue >>= 1;
                        tmpValue ^= 40961;
                    }
                    else
                    {
                        tmpValue >>= 1;
                    }
                }
            }
            sendBytes[1] = (byte)((tmpValue & 65280) >> 8);
            sendBytes[0] = (byte)(tmpValue & 255);
            this.InitialValue = (ushort)tmpValue;
            return sendBytes;
        }
        
        /// <summary>
        ///  恢复到出厂设置。
        /// </summary>
        /// <param name="deviceId"></param>
        public void RestoreConfig(int deviceId)
        {
            SDKApi.EagleControl_RestoreConfig(deviceId);
        }
        public string SelfError { get; set; }
        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
           
            var message = e.Message as ScsRawDataMessage;
            var data = message.MessageData;
            var byteLength = data.Length;
            int dataType = data[3];
            switch (dataType)
            {
                case 4://自检应答
                       // CRC16 crc = new CRC16();
                       //var s= crc.ExecuteCheck(data);

                    StringBuilder sb6 = new StringBuilder();
                    var data6 = data[6];
                    for (int i = 0; i < 8; i++)
                    {
                        var bv = data6 >> i & 1;
                        switch (i)
                        {
                            case 0:
                                sb6.AppendLine(string.Format("通信模块电压:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                sb6.AppendLine(string.Format("变频模块{0}电压:{1}", i, bv == 1 ? "正常" : "异常"));
                                break;
                            case 5:
                                sb6.AppendLine(string.Format("标准源电压:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            case 6:
                                sb6.AppendLine(string.Format("本振模块电压:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            case 7:
                                sb6.AppendLine(string.Format("变频模块5电压:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            default:
                                break;
                        }
                    }
                    var data5 = data[5];
                    for (int i = 0; i < 3; i++)
                    {
                        var bv = data6 >> i & 1;
                        switch (i)
                        {
                            case 0:
                            case 1:
                            case 2:
                                sb6.AppendLine(string.Format("变频模块{0}电压:{0}", i + 6, bv == 1 ? "正常" : "异常"));
                                break;
                        }                    
                    }
                    var data8 = data[8];
                    for (int i = 0; i < 8; i++)
                    {
                        var bv = data8 >> i & 1;
                        switch (i)
                        {
                            case 0:
                                sb6.AppendLine(string.Format("通信模块电流:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                sb6.AppendLine(string.Format("变频模块{0}电流:{1}", i, bv == 1 ? "正常" : "异常"));
                                break;
                            case 5:
                                sb6.AppendLine(string.Format("标准源电流:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            case 6:
                                sb6.AppendLine(string.Format("本振模块电流:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            case 7:
                                sb6.AppendLine(string.Format("变频模块5电流:{0}", bv == 1 ? "正常" : "异常"));
                                break;
                            default:
                                break;
                        }
                    }

                    var data7 = data[7];
                    for (int i = 0; i < 3; i++)
                    {
                        var bv = data6 >> i & 1;
                        switch (i)
                        {
                            case 0:
                            case 1:
                            case 2:
                                sb6.AppendLine(string.Format("变频模块{0}电流:{0}", i + 6, bv == 1 ? "正常" : "异常"));
                                break;
                        }
                    }
                    var data9 = data[9];
                    for (int i = 0; i <4; i++)
                    {
                        var bv = data6 >> i & 1;
                        switch (i)
                        {
                            case 0:
                            case 1:
                            case 2:
                                sb6.AppendLine(string.Format("校准源模块本振{0}锁定指示:{1}", i+1, bv == 1 ? "锁定" : "未锁定"));
                                break;
                        }
                    }
                    var data10 = data[10];
                    for (int i = 0; i < 4; i++)
                    {
                        var bv = data6 >> i & 1;
                        switch (i)
                        {
                            case 0:
                                break;
                            case 1:
                            case 2:
                                sb6.AppendLine(string.Format("本振模块本振{0}锁定指示:{1}", i + 1, bv == 1 ? "锁定" : "未锁定"));
                                break;
                        }
                    }
                   // _eventAggregator.GetEvent<InfoEventArgs>().Publish(sb6.ToString()+"\n");
                    if (data[5]==255)
                    {
                        sb6.AppendLine("\n");
                        sb6.AppendLine("变频模块电压正常!");
                    }
                    if (data[6]==255)
                    {
                        sb6.AppendLine("标准源电压正常!");
                        sb6.AppendLine("本振模块电压正常!");
                        sb6.AppendLine("校准源电压正常!");
                         sb6.AppendLine("通信模块电压正常!");
                    }
                    if (data[7] == 255)
                    {
                        sb6.AppendLine("\n");
                        sb6.AppendLine("变频模块电流正常!");
                    }
                    if (data[8] == 255)
                    {
                        sb6.AppendLine("标准源电流正常!");
                        sb6.AppendLine("本振模块电流正常!");
                        sb6.AppendLine("校准源电流正常!");
                        sb6.AppendLine("通信模块电流正常!");
                    }
                    if (data[9] == 15)
                    {
                        sb6.AppendLine("校准源模块本振锁定指示：锁定!");
                       
                    }
                    sb6.AppendLine("本振模块锁定指示：锁定!");
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish(sb6.ToString());
                    break; 
            }
        }
        #region 属性

        private ushort m_InitialValue = 65535; 
        public ushort InitialValue
        {
            get
            {
                return this.m_InitialValue;
            }
            set
            {
                this.m_InitialValue = value;
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }
        private int _spPort;
        public int SpPort
        {
            get { return _spPort; }
            set { SetProperty(ref _spPort, value); }
        }
        private string _spIp;
        public string SpIP
        {
            get { return _spIp; }
            set { SetProperty(ref _spIp, value); }
        }

        private ObservableCollection<Signal> _signals;
        public ObservableCollection<Signal> Signals
        {
            get { return _signals; }
            set { SetProperty(ref _signals, value); }
        }
        private ObservableCollection<BandWidth> _bandWidths;
        public ObservableCollection<BandWidth> BandWidths
        {
            get { return _bandWidths; }
            set { SetProperty(ref _bandWidths, value); }
        }
        private ObservableCollection<SignalForm> _signalForms;
        public ObservableCollection<SignalForm> SignalForms //信号形式
        {
            get { return _signalForms; }
            set { SetProperty(ref _signalForms, value); }
        }
        private int _channelNo=1;
        public int ChannelNo
        {
            get { return _channelNo; }
            set { SetProperty(ref _channelNo, value); }

        }
        private int _spzy=0;
        public int Spzy
        {
            get { return _spzy; }
            set { SetProperty(ref _spzy, value); }

        }
        private int _centerZy1=0;
        public int CenterZy1
        {
            get { return _centerZy1; }
            set { SetProperty(ref _centerZy1, value); }

        }
        private int _centerZy2=0;
        public int CenterZy2
        {
            get { return _centerZy2; }
            set { SetProperty(ref _centerZy2, value); }

        }
        private int  _controlBegin=0;
        public int ControlBegin
        {
            get { return _controlBegin; }
            set { SetProperty(ref _controlBegin, value); }

        }
        private int _controlEnd=0;
        public int ControlEnd
        {
            get { return _controlEnd; }
            set { SetProperty(ref _controlEnd, value); }

        }
        private int _controlStep=0;
        public int ControlStep
        {
            get { return _controlStep; }
            set { SetProperty(ref _controlStep, value); }

        }
        private int _controltime;
        public int Controltime
        {
            get { return _controltime; }
            set { SetProperty(ref _controltime, value); }

        }
        private int _correctBegin;
        public int CorrectBegin
        {
            get { return _correctBegin; }
            set { SetProperty(ref _correctBegin, value); }

        }
        private int _correctend;
        public int CorrectEnd
        {
            get { return _correctend; }
            set { SetProperty(ref _correctend, value); }

        }
        private int _correctstep=10;
        public int Correctstep
        {
            get { return _correctstep; }
            set { SetProperty(ref _correctstep, value); }

        }
        private int _correcttime;
        public int Correcttime //校准时间间隔
        {
            get { return _correcttime; }
            set { SetProperty(ref _correcttime, value); }

        }
        private int _correctrange;//幅度
        public int Correctrange
        {
            get { return _correctrange; }
            set { SetProperty(ref _correctrange, value); }

        }
        private Signal _selectsignal;
        public Signal SelectedSignal
        {
            get { return _selectsignal; }
            set { SetProperty(ref _selectsignal, value); }

        }
        private BandWidth _selectkd;
        public BandWidth SelectedKd
        {
            get { return _selectkd; }
            set { SetProperty(ref _selectkd, value); }

        }
        private SignalForm _selectSignalForm;
        public SignalForm SelectSignalForm
        {
            get { return _selectSignalForm; }
            set { SetProperty(ref _selectSignalForm, value); }

        }
        public ICommand ChannelCmd { get; private set; }
        public ICommand CorrectCmd { get; private set; }//校准
        public ICommand ControlCmd { get; private set; } //频率控制
        #endregion
    }
}
