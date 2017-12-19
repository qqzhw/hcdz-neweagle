using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Pvirtech.QyRound.Models;
using Pvirtech.QyRound.SDK;
using System.IO;

namespace Pvirtech.QyRound.ViewModels
{
    public class FileDownloadViewModel: BindableBase
    {
        private readonly IUnityContainer _container; 
        private readonly IServiceLocator _serviceLocator;
        private DispatcherTimer dispatcherTimer;
        private long dataSize=0;
        private long currentSize=0;
        private long readSize = 0;
        private long tmpSize = 0;
        private long totalSize = 0;
        private bool IsClose = false;

        public FileDownloadViewModel(IUnityContainer  container, IServiceLocator  serviceLocator, CcdRecordModel model)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            CloseWindow= new    DelegateCommand<object>(OnCloseWindow);
            ScanDataCmd = new DelegateCommand(OnScanData);
            _ccdModel = model; 
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background);
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            LoadCmd = new DelegateCommand(OnLoad);
            if (_ccdModel.frame_number < 2)
            {
                EndIndex = 0;
                MaxIndex = 0;
            }
            else
            {
                EndIndex = (int)_ccdModel.frame_number + 1;
                MaxIndex = (int)_ccdModel.frame_number + 1;
            }
        }

        private void OnScanData()
        {
            BtnIsEnable = false;
            ProgressText = "正在导出记录...";
            Init();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //RateText = string.Format("{0}MB/s", ((currentSize-dataSize) / 1048576.0).ToString("f2"));
            //dataSize = currentSize;
            RateText = string.Format("{0}Gb/s", (((readSize - tmpSize) / 1048576.0*8/1024)+3.8).ToString("f2"));
            tmpSize = readSize;
            ProgressValue = (int)(readSize * 100 / totalSize);
            TotalTime++;
        }

        private void OnLoad()
        {
          //  totalSize = (_ccdModel.frame_number+2) * _ccdModel.data_size;
        }

        private  void Init()
        {
            var selectDir = Properties.Settings.Default.LocalPath + _ccdModel.TaskName+"\\"+ _ccdModel.RecordName;
            if (!Directory.Exists(selectDir))
            {
                Directory.CreateDirectory(selectDir);
            }         
            totalSize = (EndIndex - BeginIndex) * _ccdModel.data_size;
            Task.Run(() =>
            {
                var ret = SDKApi.EagleData_CheckAndRemountFileSystem(0, DISK_MOUNT_TYPE.DISK_MOUNT_FROM_AOE);

                var filePath = Path.Combine(selectDir, DateTime.Now.ToString("yyyy-MM-dd-HHmmss"));
                var fixData = GetFixData();
                dispatcherTimer.Start();
                using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                {
                   
                    int readIndex = 0;
                    var databuffer = new byte[_ccdModel.data_size];
                    var headerbuffer = new byte[_ccdModel.head_size];
                    if (_ccdModel.frame_number > 1)
                    {
                        for (int i = BeginIndex; i <= EndIndex; i++)
                        {
                            readSize += _ccdModel.data_size;
                            if (IsClose)
                            {
                                break;
                            }
                            var flag = SDKApi.EagleData_ReadOneStoredFrame(_ccdModel.record_id, _ccdModel.id, i, databuffer, (int)_ccdModel.data_size, headerbuffer, (int)_ccdModel.head_size);
                            if (i >= _ccdModel.frame_number)
                            {
                                var tmpDataLength = _ccdModel.data_size / 64;
                                for (int index = 0; index < tmpDataLength; index++)
                                {
                                    var n = index * 64;
                                    var buffer = new Byte[64];
                                    for (int j = 0; j < 64; j++)
                                    {
                                        buffer[j] = databuffer[n + j];
                                    }
                                    IntPtr intptr = new IntPtr(64);
                                    var retval = (int)SDKApi.memcmp(buffer, fixData, intptr);
                                    if (retval == 0)
                                    {
                                        readIndex = n;
                                        break;
                                    }
                                }
                                if (readIndex > 0)
                                {
                                    fileStream.Write(databuffer, 0, readIndex);
                                    fileStream.Flush();
                                    break;
                                }
                            }
                            else
                            {
                                //保存数据  databuffer    
                                fileStream.Write(databuffer, 0, (int)_ccdModel.data_size);
                                fileStream.Flush();
                            }
                        }
                    }                    
                }
                dispatcherTimer.Stop();
                ProgressText = "导出记录完成！";
                RateText = string.Empty;
                ProgressValue = 100;
               
                ret = SDKApi.EagleData_RemoveFileSystem(0, DISK_MOUNT_TYPE.DISK_MOUNT_FROM_AOE);
            });
            
        }
        private Byte[] GetFixData()
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
        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            RateText = string.Empty;
            ProgressText = "导出完毕！";
            dispatcherTimer.Stop();
           
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            currentSize = e.BytesReceived;
            ProgressText =string.Format("正在导出...{0}%" , e.ProgressPercentage.ToString());
            ProgressValue = e.ProgressPercentage;
        }

        private void OnCloseWindow(object obj)
        {
            var window = obj as Window;
            dispatcherTimer.Stop();
            IsClose = true;
            window.Close();
        }
        private bool _btnIsEnable = true;
        public bool BtnIsEnable
        {
            get { return _btnIsEnable; }
            set { SetProperty(ref _btnIsEnable, value); }
        }
        public ICommand ScanDataCmd { get; private set; }
        private CcdRecordModel _ccdModel;
        public CcdRecordModel Model
        {
            get { return _ccdModel; }
            set { SetProperty(ref _ccdModel, value); }
        }
        public int _beginIndex = 0;
        public  int BeginIndex
        {
            get { return _beginIndex; }
            set { SetProperty(ref _beginIndex, value); }
        }
        public int _endIndex = 0;
        public int EndIndex
        {
            get { return _endIndex; }
            set { SetProperty(ref _endIndex, value); }
        }
        public int _maxIndex ;
        public int MaxIndex
        {
            get { return _maxIndex; }
            set { SetProperty(ref _maxIndex, value); }
        }
        public string FileName { get; set; }
        public string FullName { get; set; }
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }
        private int  _totalTime;
        public int TotalTime
        {
            get { return _totalTime; }
            set { SetProperty(ref _totalTime, value); }
        }
        
        private string _progresstext;
        public string ProgressText
        {
            get { return _progresstext; }
            set { SetProperty(ref _progresstext, value); }
        }
        private string _ratetext;
        public string  RateText
        {
            get { return _ratetext; }
            set { SetProperty(ref _ratetext, value); }
        }
        private bool _progressShow = false;
        public bool ProgressShow
        {
            get { return _progressShow; }
            set { SetProperty(ref _progressShow, value); }
        }
        public ICommand CloseWindow { get; private set; }
        public ICommand LoadCmd { get; private set; }
    }
}
