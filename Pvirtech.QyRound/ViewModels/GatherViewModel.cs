using Prism.Mvvm;
using Pvirtech.QyRound.Properties;
using Pvirtech.TcpSocket.Scs.Client;
using Pvirtech.TcpSocket.Scs.Communication.EndPoints.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using Pvirtech.TcpSocket.Scs.Communication;
using System.Collections.ObjectModel;
using Pvirtech.QyRound.Models;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using Pvirtech.TcpSocket.Scs.Communication.Protocols.BinarySerialization;
using Pvirtech.QyRound.Commons;
using Prism.Events;

namespace Pvirtech.QyRound.ViewModels
{
    /// <summary>
    /// 数据采集控制类
    /// </summary>

    public class GatherViewModel : BindableBase
    {
        public  IScsClient Client { get; set; }
        private readonly IEventAggregator _eventAggregator;
        public GatherViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            SelectedModeCmd =new   DelegateCommand<object>(OnSelectedMode);
            Init();
            InitModel();
        }

        private void OnSelectedMode(object obj)
        {
            int s = 8;
        }

        /// <summary>
        /// 采集自检
        /// </summary>
        public void OnSelfCheck()
        {
            OnSendData(0x03,0x04,0x00);
        }
        private void InitModel()
        {
            _collectMode = new ObservableCollection<CollectMode>();
            _collectMode.Add(new CollectMode() { ModeByte = 0xff, Name = "选择模式" });
            _collectMode.Add(new CollectMode() { ModeByte=0x01,Name="盲采集"});
            _collectMode.Add(new CollectMode() { ModeByte = 0x02, Name = "门限采集" });
            _collectMode.Add(new CollectMode() { ModeByte = 0x03, Name = "标准采集" });

        }

        private void Init()
        {
            _cjIp = Settings.Default.CjIP;
            _cjPort = Settings.Default.CjPort;
            Client = ScsClientFactory.CreateClient(new ScsTcpEndPoint(CjIP, CjPort));            
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
                 Client.WireProtocol =new CustomWireProtocol(); //Set custom wire protocol
                //Register to MessageReceived event to receive messages from server.
                Client.ConnectTimeout = 5;
                //Client.WireProtocol = GetDefaultWireProtocol();
                Client.MessageReceived +=OnMessageReceived;
                Client.Connected +=OnConnected;
                Client.Disconnected +=OnDisconnected;
                Client.Connect(); //Connect to the server  
                //Send message to the server
                //findItem.Client.SendMessage(new ScsTextMessage("3F", "q1"));

                //client.Disconnect(); //Close connection to server
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Client.Dispose();
                _eventAggregator.GetEvent<InfoEventArgs>().Publish("TCP连接出现异常\n");                            
            }
        }
        public void OnSendData(byte length, byte mark, byte data)
        {
            var modeByte = new List<byte>();
            modeByte.Add(0x7e);//帧头
            modeByte.Add(length);//帧长度
            modeByte.Add(mark);//帧标识
            modeByte.Add(data);//数据
            //和校验
            var andValue = (byte)(length + mark + data);
            modeByte.Add(andValue);
            modeByte.Add(0xf5);//帧尾
            var t1 = string.Join("", modeByte);
            var t2 = CommonHelper.ByteToString(modeByte.ToArray());
            var text = new ScsRawDataMessage(modeByte.ToArray(),"k1");
            if (Client.CommunicationState == TcpSocket.Scs.Communication.CommunicationStates.Connected)
            {
                Client.SendMessage(text);               
            }
        }

        public void OnSendData(byte length, byte mark, byte data1, byte data2, byte data3)
        {
            var modeByte = new List<byte>();
            modeByte.Add(0x7e);//帧头
            modeByte.Add(length);//帧长度
            modeByte.Add(mark);//帧标识
            modeByte.Add(data1);//数据1
            modeByte.Add(data2);//数据2
            modeByte.Add(data2);//数据3
            //和校验
            var andValue = (byte)(length + mark + data1 + data2 + data3);
            modeByte.Add(andValue);
            modeByte.Add(0xf5);//帧尾
            //var t1 = string.Join("", modeByte);
            var t2 = CommonHelper.ByteToString(modeByte.ToArray());
            var text = new ScsRawDataMessage(modeByte.ToArray());
            if (Client.CommunicationState == TcpSocket.Scs.Communication.CommunicationStates.Connected)
            {
                Client.SendMessage(text);               
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

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
           // var message = e.Message as ScsRawDataMessage;
            var msg = e.Message as ScsRawDataMessage;
            var result = Commons.CommonHelper.NoSpaceByteToString(msg.MessageData);
            if (result == "7E03050109F5")
            {
               // _eventAggregator.GetEvent<InfoEventArgs>().Publish("执行正常！\n");
            }
            else
            {
                if (msg.MessageData[0] == 126 && msg.MessageData[1] == 28)
                { 
                    var year =(ushort)( (msg.MessageData[4] << 8) + msg.MessageData[3]);
                    var month = msg.MessageData[5];
                    var day = msg.MessageData[7];
                    var hour = msg.MessageData[9];
                    var minute = msg.MessageData[11];
                    var second = msg.MessageData[13];
                    var fpgaT = msg.MessageData[16];
                    var dspT = msg.MessageData[18];
                    var tmpV12 = (msg.MessageData[20] << 8) + msg.MessageData[19];
                    var v12 = (tmpV12 / 4096.0 * 2).ToString("f2");
                    var v18 = (((msg.MessageData[22] << 8) + msg.MessageData[21]) / 4096.0 * 2).ToString("f2");
                    var v33 = (((msg.MessageData[24] << 8) + msg.MessageData[23]) / 4096.0 * 4).ToString("f2");
                    var v1 = (((msg.MessageData[26] << 8) + msg.MessageData[25]) / 4096.0 * 2).ToString("f2");
                    var v125 = (((msg.MessageData[28] << 8) + msg.MessageData[27]) / 4096.0 * 2).ToString("f2");
                    if (!IsUpdateTime)
                    { 
                        var strInfo = new StringBuilder();
                        strInfo.AppendLine(string.Format("采集设备FPGA温度：{0}℃", fpgaT));
                        strInfo.AppendLine(string.Format("采集设备DSP温度：{0}℃", dspT));
                        strInfo.AppendLine(string.Format("1.2V电压值：{0}V", v12));
                        strInfo.AppendLine(string.Format("1.8V电压值：{0}V", v18));
                        strInfo.AppendLine(string.Format("3.3V电压值：{0}V", v33));
                        strInfo.AppendLine(string.Format("1V电压值：{0}V", v1));
                        strInfo.AppendLine(string.Format("1.25V电压值：{0}V", v125));
                        _eventAggregator.GetEvent<InfoEventArgs>().Publish(strInfo.ToString());
                    }
                        Task.Run(() =>{
                        UpdateTime(year,  month,  day,  hour,  minute,  second);
                    });
                    IsUpdateTime = false;
                }
            }
        }
        private void UpdateTime(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second)
        {
            bool flag = NativeMethods.SyncLocalTime(year, month, day, hour, minute, second);
            if (!flag)
              _eventAggregator.GetEvent<InfoEventArgs>().Publish("时间更新失败！\n");
            else
            {
                _eventAggregator.GetEvent<InfoEventArgs>().Publish("时间更新成功！\n");
            }
        }
        
        #region 属性
        public ICommand SelectedModeCmd { get; private set; }
        private bool _isUpdateTime = false;
        public bool IsUpdateTime
        {
            get { return _isUpdateTime; }
            set { SetProperty(ref _isUpdateTime, value); }
        }
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }
        private bool _isRecording=false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                SetProperty(ref _isRecording, value);
                if (_isRecording)
                {
                    CjButtonText = "停止采集";
                }
                else
                {
                    CjButtonText = "开始采集";
                }
            }
        }
        private string _cjbuttonText = "开始采集";
        public string CjButtonText
        {
            get { return _cjbuttonText; }
            set { SetProperty(ref _cjbuttonText, value); }
        }
        private bool _cjbuttonEnable = true;
        public bool CjButtonEnable
        {
            get { return _cjbuttonEnable; }
            set { SetProperty(ref _cjbuttonEnable, value); }
        }
        private int _cjPort;
        public int CjPort
        {
            get { return _cjPort; }
            set { SetProperty(ref _cjPort, value); }
        }
        private string _cjIp;
        public string CjIP
        {
            get { return _cjIp; }
            set { SetProperty(ref _cjIp, value); }
        }
        private int _menxianzhi;
        public int MenXianZhi
        {
            get { return _menxianzhi; }
            set { SetProperty(ref _menxianzhi, value); }
        }
        private ObservableCollection<CollectMode> _collectMode;
        public ObservableCollection<CollectMode>  CollectModes
        {
            get { return _collectMode; }
            set { SetProperty(ref _collectMode, value); }
        }
        private CollectMode _currentMode;
        public CollectMode CurrentMode
        {
            get { return _currentMode; }
            set { SetProperty(ref _currentMode, value); }
        }
        #endregion
    }
}
