using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

using Pvirtech.QyRound.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Pvirtech.QyRound.Models;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using Telerik.Windows.Controls.GridView;
using System.Windows.Controls;
using System.IO;
using Pvirtech.QyRound.Properties;
using Pvirtech.QyRound.Views;
using Pvirtech.QyRound.Core.Interactivity;
using Telerik.Windows.Controls;
using Pvirtech.QyRound.Commons;
using System.Threading;

namespace Pvirtech.QyRound.ViewModels
{
    public class SDKViewModel: BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnityContainer _container;
        private EagleData_Record_Id[] _recordIds;
        private eagle_reocrd_status _reocrdstatus;
        public SDKViewModel(IUnityContainer container, IEventAggregator eventAggregator)
        {
            this._container = container;
            this._eventAggregator = eventAggregator;
            _reocrdstatus = new eagle_reocrd_status();
            _ccdRecordModels = new ObservableCollection<CcdRecordModel>();
            ListRightMenue = new DelegateCommand<object>(OnRightMenue);
            RecordCmd = new DelegateCommand<object>(OnRecordCommand); 
            FileReportCmd = new DelegateCommand<CcdRecordModel>(OnFileReort);
            DeleteCmd = new DelegateCommand<CcdRecordModel>(OnDeleteFile);
            RefreshCmd = new DelegateCommand<object>(OnFileRefresh);
            StopRecordCmd = new Prism.Commands.DelegateCommand(OnStopRecod);
            // FileDownloadCmd = new DelegateCommand<DirectoryInfoModel>(OnDownloadFile);
            Menu = new ObservableCollection<MenuItem>();
            Initializer();
        }
        public void OnStopRecod()
        {
            StopRecord(DeviceId);
            StopTask(DeviceId);
            Thread.Sleep(1000);
            Get_Record_List();
        }
        /// <summary>
        /// 开始记录
        /// </summary>
        /// <param name="obj"></param>
        private void OnRecordCommand(object obj)
        {
            if (IsConnected)
            {
                if (!IsRecording)
                {
                    StartRecord(DeviceId);
                    IsRecording = true;
                }
                else
                {
                  //  StartRecord(DeviceId); 
                    StopRecord(DeviceId);
                }
            }
            else
            {
                //设备未连接
            }
        }

        private void OnFileRefresh(object obj)
        {
            Get_Record_List();
        }

        private void OnDeleteFile(CcdRecordModel model)
        {
            var result = MessageBox.Show("你确定要删除当前记录吗？", "删除提醒", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {
                int ret = SDKApi.EagleData_DeleteRecord(model.record_id, model.id);
                if (ret != 0)
                {
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("删除存储记录失败!\n");
                    MessageBox.Show("删除记录失败！");
                }
                else
                {
                    Get_Record_List();
                }
            }
        }

        private  void OnFileReort(CcdRecordModel model)
        {
            if (model == null)
                return;
            var notification = new MessageNotification()
            {
                Title = "存储记录导出",
                Content = _container.Resolve<FileDownloadView>(new ParameterOverride("model", model)),
            };
            PopupWindows.NormalNotificationRequest.Raise(notification, (callback) => {

            });

            string selectDir = string.Empty;
            // var openFileDialog = new Microsoft.Win32.OpenDiDialog();
            //System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            //var result = folder.ShowDialog();
            //if (result == System.Windows.Forms.DialogResult.OK)
            //{
            //    string folderName = folder.SelectedPath;
            //    selectDir = folderName;
            //}
            //if (string.IsNullOrEmpty(selectDir))
            //    return; 
        }
        public Byte[]  GetFixData()
        {
            // byte start = 0xf;
            var g_szAddData = new Byte[64];
            for (int i = 0; i < 16; i++)
            {
                byte start = (byte)(0xf - i);
                var Pre = start * 16 + start;
                var Suf = i * 16 + i;
                g_szAddData[i * 4] = (byte)Pre;
                g_szAddData[i * 4 + 1] = (byte)Suf;
                g_szAddData[i * 4 + 2] = (byte)Pre;
                g_szAddData[i * 4 + 3] = (byte)Suf;
            }
            return g_szAddData;
        }
        private void OnMenuCommand(object obj)
        {
            
        }

        public ObservableCollection<MenuItem> Menu { get; set; }

        private void OnRightMenue(object obj)
        {
            Menu.Clear();
            var clickedItem = (obj as MouseButtonEventArgs).OriginalSource as FrameworkElement;
            if (clickedItem != null)
            {
                var parentRow = clickedItem.ParentOfType<GridViewRow>();
                if (parentRow != null)
                {
                    parentRow.IsSelected = true;
                    var model = parentRow.DataContext as CcdRecordModel; 
                    InitMenu(model);
                }
                else
                {
                    return;
                }

            }
        }

        private void InitMenu(CcdRecordModel model)
        { 
            //MenuItem mi0 = new MenuItem()
            //{
            //    Header = "下载",
            //    Command = FileDownloadCmd,
            //    CommandParameter = model
            //};
            //Menu.Add(mi0);
            MenuItem mi = new MenuItem()
            {
                Header = "导出",
                Command = FileReportCmd,
                CommandParameter = model
            };
            Menu.Add(mi);
             
            mi = new MenuItem()
            {
                Header = "删除",
                Command = DeleteCmd,
                CommandParameter = model
            };
            Menu.Add(mi);
            mi = new MenuItem()
            {
                Header = "刷新",
                Command = RefreshCmd,
                CommandParameter = model
            };
            Menu.Add(mi); 

    }

       private void Initializer()
        {
           
            LoadNetCard();//获取本机所有网卡
            //设置采集网卡
            SetCurrentNetCard();
        }

        public eagle_reocrd_status GetRecordStatus()
        {
           // eagle_reocrd_status reocrdstatus = new eagle_reocrd_status();//记录时间，大小计算速度
           var ret = SDKApi.EagleControl_GetRecordStatus(DeviceId, ref _reocrdstatus);          
            return _reocrdstatus;
        }
        /// <summary>
        /// 加载网卡信息
        /// </summary>
        public void LoadNetCard()
        { 
            eagle_all_netcards nics = new eagle_all_netcards(); 
            int ret = SDKApi.EagleControl_GetSystemNICs(ref nics); 
            for (int i = 1; i <= nics.card_num; i++)
            {
              // LogHelper.WriteLog(string.Format("{0} : {1}\n", i, nics.cards[i - 1].dev_description));
            }
            var settingModel = _container.Resolve<SettingsViewModel>();
            for (int i = 0; i < nics.cards.Count(); i++)
            {
                if (!string.IsNullOrEmpty(nics.cards[i].dev_name))
                {
                    if (i==Settings.Default.NetCardIndex)
                    {
                        settingModel.CurrentNetCard = nics.cards[i].dev_description;
                    }
                    settingModel.NetCardInfos.Add(new Models.NetCardInfo() { dev_name = nics.cards[i].dev_name, dev_description = nics.cards[i].dev_description });
                }
            }           

        }
        /// <summary>
        /// 设置网卡
        /// </summary>
        /// <returns></returns>
        public int SetCurrentNetCard()
        {
            int cardIndex = Properties.Settings.Default.NetCardIndex;
            eagle_all_netcards nics = new eagle_all_netcards();
            int ret = SDKApi.EagleControl_GetSystemNICs(ref nics);
            if (nics.card_num > 0)
            {  
                var infoList = new eagle_netcard_info[10];                
                infoList[0] = nics.cards[cardIndex];
                var setnice = new eagle_all_netcards();
                setnice.card_num = 1;
                setnice.cards = infoList;
                ret = SDKApi.EagleControl_SetControlNICs(ref setnice);
            }
            return ret;
        }
        /// <summary>
        /// 查询连接存储设备
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
           
            int device_num = 0;
           int ret = SDKApi.EagleControl_ScanAndGetDeviceNum(ref device_num);
            if (ret==0)
            {
                if (device_num > 0)
                {
                    IntPtr[] device_ids = new IntPtr[device_num];
                    int ids = 0;
                    ret = SDKApi.EagleControl_GetDeviceIds(device_ids, 1, ref ids);
                    if (ret == 0)
                    {
                        var initdevice = SDKApi.EagleData_Init();//初始化设备
                        DeviceId = (int)device_ids[0];
                        IsConnected = true;
                        return true;
                    }                  
                }
            }
             
            return false;
        }
        public void ShowDeviceInfo()
        {

        }
        public void LoadDeviceStatus()
        {
            var nameStr = new StringBuilder();
           // var DeviceName = string.Empty;
            var name = SDKApi.EagleControl_GetDeviceName(DeviceId, nameStr);
            DeviceName = nameStr.ToString();
            var sys = new eagle_system_status();
            var volume = new eagle_disk_total_volume();
            int ret = SDKApi.EagleControl_GetSystemStatus(DeviceId, out sys);//获取系统状态 温度‘磁盘数及剩余容量

            ret = SDKApi.EagleControl_GetDeviceDiskVolume(DeviceId, ref volume);//磁盘总容量
            Temperature = string.Format("{0}℃", sys.fpga_tempture);
            int freevolume = 0;
            uint totalvolume = 0;
            for (int i = 0; i < sys.disk_num; i++)
            {
                freevolume += sys.remained_volume[i];
                totalvolume += volume.total_volume[i];
            }
            FreeVolume = string.Format("{0}GB可用",freevolume);
            TotalVolume = string.Format("{0}GB", totalvolume);
            DiskPercent = 100.0 - (int)(freevolume * 100.0 / totalvolume);
            //设置相机Check
            for (int i = 1; i < 5; i++)
            {
                ret = SDKApi.EagleControl_CheckChannel(DeviceId, i, i == 1 ? true : false);

            }
        }


        public bool GetSystemStatus()
        {
            var sys = new eagle_system_status(); 
            int ret = SDKApi.EagleControl_GetSystemStatus(DeviceId, out sys);//获取系统状态 温度‘磁盘数及剩余容量
            if (ret == 0)
            {
                IsConnected = true;
                return true;
            }
            else
            {
                IsConnected = false;
                return false;
            }
        }
        /// <summary>
        /// 格式化磁盘
        /// </summary>
        /// <param name="deviceId"></param>
        public  void FormatDevice()
        {
            CanFormat = false;
            FormatText = "正在格式化";
            _eventAggregator.GetEvent<InfoEventArgs>().Publish("正在格式化磁盘...\n");
            Task.Run(() =>
            {
                var ret = SDKApi.EagleControl_ReformatDisk(DeviceId);
                if (ret == 0)
                {
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("正在初始化磁盘...\n");
                }
                else
                {
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("正在格式化磁盘出错...\n");
                    MessageBox.Show(string.Format("格式化出现异常:{0}!",ret));
                    FormatText = "格式化存储";
                    CanFormat = true;
                    return;
                }
                var result = SDKApi.EagleControl_ReinitDisk(DeviceId);
                if (result == 0)
                {
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("已成功初始化磁盘！\n");
                }
                else
                {
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("初始化磁盘异常！\n");
                }
                FormatText = "格式化存储";
                CanFormat = true;
                MessageBox.Show("格式化完成!");
            });
           
        }
        /// <summary>
        /// 获取记录列表
        /// </summary>
        public void Get_Record_List()
        {
            CcdRecordModels.Clear();
            SDKApi.EagleData_RefetchRecList();
            int record_num = SDKApi.EagleData_GetRecordNumber();
            if (record_num > 0)
            {
                IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EagleData_Record_Id)) * record_num);
                int nStructLength = Marshal.SizeOf(typeof(EagleData_Record));

                int actual_num = SDKApi.EagleData_GetRecordList(intPtr, record_num);

                _recordIds = new EagleData_Record_Id[actual_num];

                for (int i = 0; i < actual_num; i++)
                {
                    _recordIds[i] = (EagleData_Record_Id)Marshal.PtrToStructure(intPtr + Marshal.SizeOf(typeof(EagleData_Record_Id)) * i, typeof(EagleData_Record_Id));
                    // EagleData_Record ccd = new EagleData_Record();
                    var record = SDKApi.EagleData_GetRecordAndAllocMemory(_recordIds[i]);
                    var ccd = (EagleData_Record)Marshal.PtrToStructure(record, typeof(EagleData_Record));

                    //得到数据项
                    var item = (EagleData_CcdRecord)Marshal.PtrToStructure(ccd.ccd_record_list, typeof(EagleData_CcdRecord));
                    var model = new CcdRecordModel()
                    {
                        data_size = item.data_size,
                        data_source_type = item.data_source_type,
                        disk_bitmap = item.disk_bitmap,
                        end_time = item.end_time,
                        frame_number = item.frame_number,//镇数
                        head_size = item.head_size,
                        id = item.id,
                        pixel_sampling_type = item.pixel_sampling_type,
                        record_id = item.record_id,
                        record_index = item.record_index,
                        start_time = item.start_time,
                        task_index = item.task_index,
                        TaskName = item.record_id.task_name,
                        RecordName = item.record_id.task_name + item.record_id.start_time,
                    };
                    long totalsize = 0;
                    if (item.frame_number < 2)
                    {
                        totalsize = 0;
                    }
                    else
                        totalsize = (long)(item.frame_number + 1) * item.data_size;
                    model.TotalSize = String.Format("{0:F2}GB", totalsize / (1024.0 * 1024.0 * 1024.0));
                    CcdRecordModels.Add(model);
                    SDKApi.EagleData_FreeRecordMemory(record);
                    //  var r1 = SDKApi.EagleData_DeleteRecord(list.record_id, list.id);
                }
                Marshal.FreeHGlobal(intPtr);

            }
        }
        public void ReNameTask()
        {
            SDKApi.EagleData_RefetchRecList();
            int record_num = SDKApi.EagleData_GetRecordNumber();
            if (record_num > 0)
            {
                EagleData_Record_Id[] ids = new EagleData_Record_Id[record_num];
               // int actual_num = SDKApi.EagleData_GetRecordList(ref ids, record_num);
                //_print_record_list(ids, actual_num);
                //int record_index = _select_input("input record to rename", actual_num, 1, 0);
                //if (record_index == 0)
                //{ 
                //    return;
                //}
                //printf("input new name:");
                //wchar_t new_name[32] = { '\0' };
                //wscanf_s(L"%ls", new_name, 31);
                //int ret = SDKApi.EagleData_RenameTask(ids[actual_num - 1], "new-code");
                //PrintResult(ret, "rename");
                //delete[] ids;
                
            }
        }
        /// <summary>
        /// 开始记录
        /// </summary>
        /// <param name="deviceId"></param>
        public bool StartRecord(int deviceId,int capture_frame_num=0,int capture_time=0,int capture_frame_interval=0)
        {
            if (IsConnected)
            {
                var date = System.DateTime.Now;
                eagle_device_time time = new eagle_device_time();
                time.wYear = (UInt16)date.Year;
                time.wMouth = (UInt16)date.Month;
                time.wDay = (UInt16)date.Day;
                time.wHour = (UInt16)date.Hour;
                time.wMinite = (UInt16)date.Minute;
                time.wSecond = (UInt16)date.Second;
                time.wMillsecond = (UInt16)date.Millisecond;
                var settime = SDKApi.EagleControl_SetDeviceTimeBase(DeviceId, ref time);
                if (settime > 0)
                {
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("存储设备授时失败!\n");
                }
                bool flag = true;
                var ret = SDKApi.EagleControl_StartTask(DeviceId, Settings.Default.TaskName, 0x01, 0);
                if (ret!=0)
                {
                    IsRecording = false;
                    flag = false;
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("开启任务失败，请检查任务名称是否有效!\n");
                    MessageBox.Show("开启任务失败!");
                }
                var str = SDKApi.EagleControl_StartRecord(DeviceId, 0, 0, 0);
                if (str != 0)
                {
                    IsRecording = false;
                    flag = false;
                    _eventAggregator.GetEvent<InfoEventArgs>().Publish("开启存储记录失败!\n");
                    MessageBox.Show("开启存储记录失败!");
                }
                Thread.Sleep(10);
                Get_Record_List();
                return flag;
            }
            else
            {
                _eventAggregator.GetEvent<InfoEventArgs>().Publish("存储设备已断开连接!\n");
                return false;
            }
        }
        /// <summary>
        /// 暂停记录
        /// </summary>
        /// <param name="deviceId"></param>
        private void PauseRecord(int deviceId)
        {
            SDKApi.EagleControl_PauseRecord(deviceId);
        }
        /// <summary>
        /// 继续记录
        /// </summary>
        /// <param name="deviceId"></param>
        private void ResumeRecord(int deviceId)
        {
            SDKApi.EagleControl_ResumeRecord(deviceId);
        }
        public void StopRecord(int deviceId)
        {
            int ret= SDKApi.EagleControl_StopRecord(DeviceId);
            if (ret==0)
            {
                IsRecording = false;
            }
            else
            {
                IsRecording = true;
            }
        }

       
        private void StopTask(int deviceId)
        {
           var ret= SDKApi.EagleControl_StopTask(deviceId);
        }

        /// <summary>
        /// 扫描并获取设备数量
        /// </summary>
        /// <returns></returns>
        private int ScanAndGetDeviceNum()
        {
            int deviceNum = 0;
            SDKApi.EagleControl_ScanAndGetDeviceNum(ref deviceNum);
            return deviceNum;
        }

        public void Delete_Ccd_Record()
        {
            SDKApi.EagleData_RefetchRecList();
            int record_num = SDKApi.EagleData_GetRecordNumber();
            if (record_num > 0)
            {
                //get record list
                var ids = new EagleData_Record_Id[record_num];
                int actual_num = 0;// SDKApi.EagleData_GetRecordList(ref ids, record_num);
                 
                 
            }
        }

        public  List<T> MarshalPtrToStructArray<T>(IntPtr p, int count)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < count; i++, p = new IntPtr(p.ToInt32() + Marshal.SizeOf(typeof(T))))
            {
                T t = (T)Marshal.PtrToStructure(p, typeof(T));
                list.Add(t);
            }
            return list;
        }

        #region 属性

        public ICommand FileDownloadCmd { get; set; } 
        public ICommand FileReportCmd { get; private set; }
        public ICommand DeleteCmd { get; private set; }
        public ICommand RefreshCmd { get; private set; }
        private int _deviceId;
        public int DeviceId
        {
            get { return _deviceId; }
            set { SetProperty(ref _deviceId, value); }
        }
        /// <summary>
        /// 设备名称
        /// </summary>
        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }
        /// <summary>
        /// 通道名称
        /// </summary>
        private string _channelName;
        public string ChannelName
        {
            get { return _channelName; }
            set { SetProperty(ref _channelName, value); }
        }
        
         private string _temperature;
        /// <summary>
        /// 温度
        /// </summary>
        public string Temperature
        {
            get { return _temperature; }
            set { SetProperty(ref _temperature, value); }
        }
        private int _channelNo=1;
        public int ChannelNo
        {
            get { return _channelNo; }
            set { SetProperty(ref _channelNo, value); }
        }
        private string _totalVolume;
        public string TotalVolume
        {
            get { return _totalVolume; }
            set { SetProperty(ref _totalVolume, value); }
        }
        private string _freeVolume;
        public string FreeVolume
        {
            get { return _freeVolume; }
            set { SetProperty(ref _freeVolume, value); }
        }
        /// <summary>
        /// 硬盘使用百分比
        /// </summary>
        private double _diskPercent;
        public double DiskPercent
        {
            get
            {
                return _diskPercent;
            }
            set
            {
                SetProperty(ref _diskPercent, value);
            }
        }

        private int _recordStatus;
        public int RecordStatus
        {
            get { return _recordStatus; }
            set { SetProperty(ref _recordStatus, value); }
        }
        private string _recordRateText="0.00GB/s";
        public string RecordRateText
        {
            get { return _recordRateText; }
            set { SetProperty(ref _recordRateText, value); }
        }
        private string _recordStatusText="就绪";
        public string RecordStatusText
        {
            get { return _recordStatusText; }
            set { SetProperty(ref _recordStatusText, value); }
        }
        private string  _recordText="开始记录";
        public string RecordText
        {
            get { return _recordText; }
            set { SetProperty(ref _recordText, value); }
        }
        
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }
        private bool _isRecording;
        public bool IsRecording
        {
            get { return _isRecording; }
            set {
                SetProperty(ref _isRecording, value);
                if (_isRecording)
                {
                    CanRecord = false;
                    RecordText = "停止记录";
                }
                else
                {
                    CanRecord = true;
                    RecordText = "开始记录";
                }
            }
        }
        private bool _canRecord=true;
        public bool CanRecord
        {
            get { return _canRecord; }
            set { SetProperty(ref _canRecord, value); }
        }
        private bool _canFormat = true;
        public bool CanFormat
        {
            get { return _canFormat; }
            set { SetProperty(ref _canFormat, value); }
        }
        private string _formatText= "格式化存储";
        public string FormatText
        {
            get { return _formatText; }
            set { SetProperty(ref _formatText, value); }
        }
        private ObservableCollection<CcdRecordModel> _ccdRecordModels;
        public ObservableCollection<CcdRecordModel> CcdRecordModels
        {
            get { return _ccdRecordModels; }
            set { SetProperty(ref _ccdRecordModels, value); }
        }
        public ICommand ListRightMenue { get; private set; }
        public ICommand RecordCmd { get; private set; }
        public ICommand MenuItemCommand { get; private set; }
     
        public ICommand StopRecordCmd { get; private set; }
        #endregion
    }
}
