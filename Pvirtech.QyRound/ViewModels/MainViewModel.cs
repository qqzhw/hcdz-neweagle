using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Pvirtech.Metro.Controls;
using Pvirtech.QyRound.Commons;
using Pvirtech.QyRound.Core.Common;
using Pvirtech.QyRound.Core.Interactivity;
using Pvirtech.QyRound.Models;
using Pvirtech.QyRound.SDK;
using Pvirtech.QyRound.Services;
using Pvirtech.QyRound.Views;
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Pvirtech.QyRound.ViewModels
{

    public class MainViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;
        private readonly IServiceLocator _serviceLocator;
        private readonly IQyRoundService _qyRoundService;
        private DispatcherTimer dispatcherTimer;
        private DispatcherTimer mainTimer;
        private bool IsCompleted = false;
        private bool IsStop = false;
       
        public MainViewModel(IUnityContainer container, IEventAggregator eventAggregator, IServiceLocator serviceLocator, IQyRoundService qyRoundService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _serviceLocator = serviceLocator;
            _qyRoundService = qyRoundService;
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            mainTimer= new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            mainTimer.Tick += MainTimer_Tick;
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            LoadData();//加载基本信息
            ConnectCmd = new DelegateCommand(OnConnectedDevice);
            SdkInitCmd = new DelegateCommand(OnSDKInit);
            StartRecordCmd = new DelegateCommand(OnStartRecord);
            SelectedModeCmd = new DelegateCommand<CollectMode>(OnSelectedMode);
            SelectSignalCmd = new DelegateCommand<Signal>(OnSelectedSignal);
            SelectChannelCmd = new DelegateCommand<object>(OnSelectChannel);
            SelectKdCmd = new DelegateCommand<BandWidth>(OnSelectedKd);
            SelectCorrectCmd = new DelegateCommand<SignalForm>(OnSelectedCorrect);
            CjConnect = new DelegateCommand(OnCjConnected);
            // SpConnect = new DelegateCommand(OnSpConnected);射频连接
            CjStartCmd = new DelegateCommand(OnCjStart);
            ClearInfo=new DelegateCommand(OnClearInfo);
            SelectedRateCmd = new DelegateCommand<object>(OnSelectedRate);
            ReadDmaCmd = new DelegateCommand<object>(OnSelfChecked);
            MenxianChanged = new DelegateCommand<object>(OnMenxianChanged);
            PLChanged= new DelegateCommand<object>(OnPLChanged);
            LocalDataJxCmd = new DelegateCommand(OnLocalDataRead);
            AdDataJxCmd = new DelegateCommand(OnAdDataRead);
            UpdateTimeCmd = new DelegateCommand(OnUpdateTime);
            _eventAggregator.GetEvent<InfoEventArgs>().Subscribe(OnShowInfo, ThreadOption.UIThread);
            Application.Current.Exit += Current_Exit;
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

        private void OnPLChanged(object obj)
        { 
            var control = obj as NumericUpDown;
            if (control.Value == null)
                return;
            //RadioVm.ControlBegin = (int)control.Value;
            //RadioVm.PLControl();
        }

        private   void MainWindow_Closed(object sender, EventArgs e)
        {
            GatherVm.OnSendData(0x03, 0x02, 0x00);
            Thread.Sleep(500);
            SDKVm.OnStopRecod();
            mainTimer.Stop();
            dispatcherTimer.Stop();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //获取记录状态
            var status = SDKVm.GetRecordStatus();//记录时间，大小计算速度

            if (status.record_time[0] > 0)
            {
                MessageBox.Show("正在采集数据,请先停止采集！");
                e.Cancel = true;
            } 
            
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            GatherVm.OnSendData(0x03, 0x02, 0x00);
            Thread.Sleep(500);
            SDKVm.OnStopRecod();
            mainTimer.Stop();
            dispatcherTimer.Stop();
            SDKVm.RecordRateText = "0.00GB/s";
            SDKVm.RecordStatusText = "就绪";
            ButtonEnable = true;
            IsRecording = false;
        }
  

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            SDKVm.GetSystemStatus();
        }

        private void OnClearInfo()
        {
            LogInfo = string.Empty;
        }

        private void OnSelectChannel(object obj)
        {
            int index = Convert.ToInt32(obj);
            var s = index;
            RadioVm.ChannelNo = index + 1;
        }

        private void OnUpdateTime()
        {
            if (GatherVm.IsConnected)
            {
                LogInfo = "正在更新设备时间！\n";
                GatherVm.IsUpdateTime = true;
                GatherVm.OnSelfCheck();
            }
        }

        private void OnShowInfo(string logInfo)
        {
            LogInfo += logInfo;
        }

        /// <summary>
        /// AD数据解析
        /// </summary>
        private void OnAdDataRead()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                var fileName = openFileDialog.FileName;
                var notification = new MessageNotification()
                {
                    Title = "AD数据解析",
                    Content = _container.Resolve<ReadAdDataView>(new ParameterOverride("fileName", fileName)),
                };
                PopupWindows.NormalNotificationRequest.Raise(notification, (callback) =>
                {

                });
            }
        }


        /// <summary>
        /// 本地数据读取，按通道存储
        /// </summary>
        /// <param name="obj"></param>
        private void OnLocalDataRead()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                var fileName = openFileDialog.FileName;
                var notification = new MessageNotification()
                {
                    Title = "数据解析",
                    Content = _container.Resolve<ReadDataView>(new ParameterOverride("fileName", fileName)),
                };
                PopupWindows.NormalNotificationRequest.Raise(notification, (callback) => {

                });
            }
        }
        private void OnMenxianChanged(object obj)
        {
            var control = obj as NumericUpDown;
            if (control.Value == null)
                return;
            var result = control.Value + 149;
            GatherVm.OnSendData(0x05, 0x03, (byte)result, 0x00, 0x00);
        }

        private void OnSpConnected()
        {
            RadioVm.Connect();
        }

        private void OnCjConnected()
        {
            GatherVm.Connect();
        }

        private void OnSelectedCorrect(SignalForm signalForm)
        {
            RadioVm.SelectSignalForm = signalForm;
        }

        /// <summary>
        /// 所有设备自检
        /// </summary>
        /// <param name="obj"></param>
        private void OnSelfChecked(object obj)
        {
            //  OnSendData(0x03, 0x04, 0x00);//自检
            // OnSendData(0x05,0x03,0x32,0x00,0x00);//门限
            // OnSendData(0x03, 0x04, 0x00);
            // RadioVm.MonitorSystemSelfCal(true);
            //MessageBox.Show("开始执行通道控制,通道1,标校信号，60MHz 25 0 0");
            //setChannelIFBWgain(1,true,true,1,25,0,0);
            //Thread.Sleep(100);
            //MessageBox.Show("开始执行频率控制,开始频率100,终止100，步进1MHz，间隔2秒");
            //PLControl();
            LogInfo += "[采集]：正在自检...\n";
            GatherVm.OnSelfCheck();
            LogInfo+="[射频]：正在自检...\n";
            RadioVm.MonitorSystemSelfCal();
            if (GatherVm.IsConnected)
            {
                LogInfo += "采集分机正常!\n";
            }
            else
            {
                LogInfo += "采集分机未连接!\n";
            }
            if (SDKVm.IsConnected)
            {
                LogInfo += "存储分机正常!\n";
            }
            else
            {
                LogInfo += "存储分机未连接!\n";
            }
            if (RadioVm.IsConnected)
            {
                LogInfo += "射频分机正常!\n";
            }
            else
            {
                LogInfo += "射频分机未连接!\n";
            }
            
        }


        /// <summary>
        /// 频率输出选择值
        /// </summary>
        /// <param name="silderValue"></param>
        private void OnSelectedRate(object silderValue)
        {
            int result = 60;
            int.TryParse(silderValue.ToString(), out result);
            SelectedRate = result;
        }
        /// <summary>
        /// 选择信号
        /// </summary>
        /// <param name="signal"></param>
        private void OnSelectedSignal(Signal signal)
        {
            RadioVm.SelectedSignal = signal;
        }

        /// <summary>
        /// 选择带宽
        /// </summary>
        /// <param name="bandWidth"></param>
        private void OnSelectedKd(BandWidth bandWidth)
        {
            RadioVm.SelectedKd = bandWidth;
        }

        /// <summary>
        /// 采集启停控制
        /// </summary>
        private void OnCjStart()
        {
            if (GatherVm.CurrentMode == null)
            {
                LogInfo = "请选择采集模式";
                return;
            }
            if (GatherVm.IsConnected)
            {
             
                if (GatherVm.IsRecording)
                {
                    GatherVm.CjButtonEnable = false;
                    GatherVm.OnSendData(0x03, 0x02, 0x01);
                    GatherVm.IsRecording = true;

                }
                else
                {
                    GatherVm.CjButtonEnable = false;
                    // GatherVm.OnSendData(0x03, 0x02, 0x01);
                    GatherVm.OnSendData(0x03, 0x02, 0x00);
                    GatherVm.CjButtonEnable = true;
                    GatherVm.IsRecording = false;
                }
            }
            else
            {
                //设备未连接
            }
        }

        /// <summary>
        /// 选中控制模式事件
        /// </summary>
        /// <param name="collectMode"></param>
        private void OnSelectedMode(CollectMode collectMode)
        {
            if (collectMode.ModeByte == 0xff)
            {
                LogInfo = "请选择采集模式\n";
                MessageBox.Show("请选择一种采集模式!");
            }
            else
            {
                GatherVm.OnSendData(0x03, 0x00, collectMode.ModeByte);
                GatherVm.CurrentMode = collectMode;
            }
            
        }
        //开始记录 所有操作
        private void OnStartRecord()
        {
            if (GatherVm.CurrentMode == null)
            {
                LogInfo = "请选择采集模式！\n";
                MessageBox.Show("请先选择采集模式");
                return;
            }
             //如果计时大于O 定时采集
            if (RecordTime > 0)
            {
                ButtonEnable = false;
                IsRecording = true;
                var result = SDKVm.StartRecord(0);
                dispatcherTimer.Start();
                if (!result)
                {
                    //开启存储记录失败
                    LogInfo = "开启存储记录失败！";
                    ButtonEnable = true;
                    IsRecording = false;
                    dispatcherTimer.Stop();
                    SDKVm.RecordRateText = "0.00GB/s";
                    SDKVm.RecordStatusText = "就绪";
                    return;
                } 
                var dcTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(RecordTime),
                    IsEnabled = true
                };
                dcTimer.Tick += (s, e) =>
                {
                    GatherVm.OnSendData(0x03, 0x02, 0x00);
                    Thread.Sleep(500);
                    SDKVm.OnStopRecod();
                        //记录时间停止
                        dcTimer.Stop();
                    dispatcherTimer.Stop();
                    SDKVm.RecordRateText = "0.00GB/s";
                    SDKVm.RecordStatusText = "就绪";
                    ButtonEnable = true;
                    IsRecording = false;
                };
                Thread.Sleep(500);
                GatherVm.OnSendData(0x03, 0x02, 0x01);
                dcTimer.Start();
            }
            else
            {
                if (!IsRecording)
                {
                   
                    IsRecording = true;
                    var result = SDKVm.StartRecord(0);
                    dispatcherTimer.Start();
                    if (!result)
                    {
                        //开启存储记录失败
                        LogInfo = "开启存储记录失败！";
                        ButtonEnable = true;
                        IsRecording = false;
                        dispatcherTimer.Stop();
                        SDKVm.RecordRateText = "0.00GB/s";
                        SDKVm.RecordStatusText = "就绪";
                        return;
                    }
                    Thread.Sleep(500);
                    GatherVm.OnSendData(0x03, 0x02, 0x01);
                }
                else
                {
                    GatherVm.OnSendData(0x03, 0x02, 0x00);
                    Thread.Sleep(500);
                    SDKVm.OnStopRecod();
                    //记录时间停止 
                    dispatcherTimer.Stop(); 
                    IsRecording = false;
                    SDKVm.RecordRateText = "0.00GB/s";
                    SDKVm.RecordStatusText = "就绪";
                }
               
            }
        }

        private void OnSDKInit()
        {
           var result= MessageBox.Show("你确定要格式化存储磁盘吗？", "格式化提醒", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result==MessageBoxResult.OK)
            {
                SDKVm.FormatDevice();
            }
           
        }
       
        private unsafe void get_record_list()
        {
            SDKApi.EagleData_RefetchRecList();
            int record_num = SDKApi.EagleData_GetRecordNumber();
            if (record_num > 0)
            {

                //var ids =IntPtr.Zero ;// new EagleData_Record_Id();
                //  List<EagleData_Record_Id> list = new List<EagleData_Record_Id>();
                //  var ids = new IntPtr[record_num];//EagleData_Record_Id[record_num];
                //  for (int i = 0; i < record_num; i++)
                //  {
                //     // ids[i].task_name = new  string('\0',64);
                //     // list.Add(new EagleData_Record_Id() { task_name=new string ('\0',64),start_time=0 });
                //  }

                //// var intptr = Marshal.AllocHGlobal(Marshal.SizeOf(64));
              //  IntPtr[] ptArr = new IntPtr[1];
              //  ptArr[0] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EagleData_Record_Id)));
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EagleData_Record_Id)) * record_num);
                int nStructLength = Marshal.SizeOf(typeof(EagleData_Record));
              //  Marshal.Copy(ptArr, 0, pt, 1);

                int actual_num = SDKApi.EagleData_GetRecordList(pt, record_num);

                EagleData_Record_Id[] ids = new EagleData_Record_Id[actual_num];

                for (int i = 0; i < actual_num; i++)
                {
                   ids[i] = (EagleData_Record_Id)Marshal.PtrToStructure(pt + Marshal.SizeOf(typeof(EagleData_Record_Id)) * i, typeof(EagleData_Record_Id));
                    EagleData_Record ccd = new EagleData_Record();
                   var record = SDKApi.EagleData_GetRecordAndAllocMemory(ids[i]);
                    ccd= (EagleData_Record)Marshal.PtrToStructure(record, typeof(EagleData_Record));
                    int s = 0;   
                    var list= (EagleData_CcdRecord)Marshal.PtrToStructure(ccd.ccd_record_list, typeof(EagleData_CcdRecord));

                    var r1 = SDKApi.EagleData_DeleteRecord(list.record_id, list.id);
                    int ss1 = 0;
                    //var ret = SDKApi.EagleData_CheckAndRemountFileSystem(0, DISK_MOUNT_TYPE.DISK_MOUNT_FROM_AOE);
                    //var databuffer = new byte[list.data_size];
                    //var headerbuffer = new byte[list.head_size];
                      
                    //// if (record!= null)
                    //for (int index = 0; index < list.frame_number; i++)
                    //{
                    //    var flag = SDKApi.EagleData_ReadOneStoredFrame(list.record_id, list.id, index, databuffer, (int)list.data_size, headerbuffer, (int)list.head_size);
                    //   //保存数据  databuffer
                    //}
                    //ret = SDKApi.EagleData_RemoveFileSystem(0, DISK_MOUNT_TYPE.DISK_MOUNT_FROM_AOE);
                    //_print_record_detail(&record->id, i + 1);

                   
                    // EagleData_FreeRecordMemory(record);
              
                }
                Marshal.FreeHGlobal(pt);
            }
        }



        private void OnConnectedDevice()
        {
            GatherVm.Connect();//采集连接
            Thread.Sleep(100);
            RadioVm.Connect();//射频连接 
            Thread.Sleep(100);
            var result=SDKVm.Connected();//存储网卡连接
                                        
            if (result)
            {
                SDKVm. LoadDeviceStatus();
                SDKVm.Get_Record_List();
            }
        }
        public ICommand SelectedRateCmd { get; private set; }
        public ICommand ReadDmaCmd { get; private set; }
        public ICommand ConnectCmd { get; private set; }
        public ICommand SdkInitCmd { get; private set; }
        public ICommand StartRecordCmd { get; private set; }
        public ICommand SelectedModeCmd { get; private set; }
        public ICommand CjStartCmd { get; private set; }
        public ICommand ClearInfo { get; private set; }
        public ICommand SelectSignalCmd { get; private set; }
        public ICommand SelectChannelCmd { get; private set; }
        public ICommand SelectKdCmd { get; private set; }
        public ICommand SelectCorrectCmd { get; private set; }
        public ICommand CjConnect { get; private set; }
        public ICommand SpConnect { get; private set; }

        private CollectMode _selectCollectMode;
        public CollectMode SelectCollectMode
        {
            get { return _selectCollectMode; }
            set { SetProperty(ref _selectCollectMode, value); }

        }
        private Signal _selectsignal;
        public Signal SelectedSignal
        {
            get { return _selectsignal; }
            set { SetProperty(ref _selectsignal, value); }

        }
        
        
        private GatherViewModel _gatherVm;
        public GatherViewModel GatherVm
        {
            get { return _gatherVm; }
            set { SetProperty(ref _gatherVm, value); }

        }
        private RadioViewModel _radioVm;
        public RadioViewModel RadioVm
        {
            get { return _radioVm; }
            set { SetProperty(ref _radioVm, value); }

        }
        private SDKViewModel _sdkVm;
        public SDKViewModel SDKVm
        {
            get { return _sdkVm; }
            set { SetProperty(ref _sdkVm, value); }

        }
       
        private int _selectedRate = 60;
        public int SelectedRate
        {
            get { return _selectedRate; }
            set { SetProperty(ref _selectedRate, value); }
        }
        private int _selectedcorrect = 0;
        public int SelectedCorrect
        {
            get { return _selectedcorrect; }
            set { SetProperty(ref _selectedcorrect, value); }
        }
        private int _recordTime = 0;
        public int RecordTime
        {
            get { return _recordTime; }
            set { SetProperty(ref _recordTime, value); }
        }
        private string _logInfo;
        public string  LogInfo
        {
            get { return _logInfo; }
            set { SetProperty(ref _logInfo, value); }
        }
        private bool _isRecording = false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                SetProperty(ref _isRecording, value);
                if (_isRecording)
                {
                    ButtonText = "停止采集";
                }
                else
                {
                    ButtonText = "开始采集";
                }
            }
        }
        private string _buttonText = "开始采集";
        public string ButtonText
        {
            get { return _buttonText; }
            set { SetProperty(ref _buttonText, value); }
        }
        private bool buttonEnable = true;
        public bool ButtonEnable
        {
            get { return buttonEnable; }
            set { SetProperty(ref buttonEnable, value); }
        }
        public ICommand MenxianChanged { get; private set; }
        public ICommand PLChanged { get; private set; }
        public ICommand LocalDataJxCmd { get; private set; }
        public ICommand AdDataJxCmd { get; private set; }
        public ICommand UpdateTimeCmd { get; private set; }

        private void LoadData()
		{
            _gatherVm = _container.Resolve<GatherViewModel>();
            _radioVm = _container.Resolve<RadioViewModel>();
            _sdkVm = _container.Resolve<SDKViewModel>();
            var setting = _container.Resolve<SettingsViewModel>();
            mainTimer.Start();
        }

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
            //获取记录状态
            var status = SDKVm.GetRecordStatus();//记录时间，大小计算速度
           
            if (status.record_time[0]>0)
            {
                var highsize = (long)(status.record_size_high_part[0] << 32);
                var totalsize = highsize + status.record_size_low_part[0];
                var rate = (long)(totalsize / status.record_time[0])*1024*1000;
                double data = 0;
                if (rate>1.0)
                {
                    data = Convert.ToDouble(rate / 1024 / 1024 / 1024.0) +3;
                }
                
                SDKVm.RecordRateText = string.Format("{0}GB/s",data.ToString("f2"));
                SDKVm.RecordStatusText ="正在记录"; 
            }
            else
            {
                SDKVm.RecordRateText ="0.00GB/s";
                SDKVm.RecordStatusText = "就绪";
            }
        }
	}
}
