﻿using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace  Pvirtech.QyRound.ViewModels
{
    
    public class ReadDataViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IServiceLocator _serviceLocator;
        private DispatcherTimer dispatcherTimer;
        private long readSize = 0;
        private long tmpSize = 0;
        private long totalSize = 0;
        private bool IsClose = false;
        public ReadDataViewModel(IUnityContainer container, IServiceLocator serviceLocator, string fileName)
        {
            _container = container;
            _serviceLocator = serviceLocator;
            CloseWindow = new DelegateCommand<object>(OnCloseWindow);
            ScanDataCmd = new DelegateCommand(OnScanData);
            FileName = fileName; 
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background);
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
           

        }

        private void OnScanData()
        {
            BtnIsEnable = false;
            ProgressText = "正在解析数据... ";            
            Init();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            RateText = string.Format("{0}MB/s", ((readSize - tmpSize) / 1048576.0).ToString("f2"));
            tmpSize = readSize;            
            ProgressValue =(int) (readSize* 100 / totalSize);
        }

        private void Init()
        {
            var index = FileName.LastIndexOf("\\");
            var name = FileName.Substring(index + 1, FileName.Length - index - 1);
            var path = FileName.Substring(0,index+1);
            //if (!Properties.Settings.Default.LocalPath.EndsWith("\\"))
            //{
            //    Properties.Settings.Default.LocalPath += "\\";
            //}
           // var saveFilePath = Properties.Settings.Default.LocalPath + name;
            var saveFilePath =path + name;
            //var dir1 = CreateDir("Bar1") + name;
            // var dir2 =CreateDir("Bar2")+name;
            //var dir3 = CreateDir("Bar3") + name;
            //var dir4 = CreateDir("Bar4") + name;
            //var dir5 = CreateDir("Bar5") + name;
            //var dir6 = CreateDir("Bar6") + name;
            //var dir7 = CreateDir("Bar7") + name;
            //var dir8 = CreateDir("Bar8") + name;
            //var dir9 = CreateDir("Bar9") + name;
            //var dir10 = CreateDir("Bar10") + name;
            //var dir11 = CreateDir("Bar11") + name;
            //var dir12 = CreateDir("Bar12") + name;
            //var dir13= CreateDir("Bar13") + name;
            //var dir14 = CreateDir("Bar14") + name;
            //var dir15 = CreateDir("Bar15") + name;
            //var dir16 = CreateDir("Bar16") + name;
            for (int i = 1; i <= 16; i++)
            {
                var dirName = string.Format("Bar{0}", i);
                if (!Directory.Exists(dirName))
                {
                    var result = CreateDir(dirName);
                }
            }

            Dictionary<int, FileStream> dicFiles = new Dictionary<int, FileStream>();
            for (int i = 1; i <= 16; i++)
            {
                var dirName = string.Format("Bar{0}\\{1}", i, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                dicFiles.Add(i, new FileStream(path + dirName, FileMode.Append, FileAccess.Write));
            }

            //Dictionary<int, FileStream> dicFiles = new Dictionary<int, FileStream>();
            //dicFiles.Add(1,new FileStream(dir1,FileMode.Append,FileAccess.Write));
            //dicFiles.Add(2, new FileStream(dir2, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(3, new FileStream(dir3, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(4, new FileStream(dir4 , FileMode.Append, FileAccess.Write));
            //dicFiles.Add(5, new FileStream(dir5, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(6, new FileStream(dir6, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(7, new FileStream(dir7, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(8, new FileStream(dir8, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(9, new FileStream(dir9, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(10, new FileStream(dir10, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(11, new FileStream(dir11, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(12, new FileStream(dir12, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(13, new FileStream(dir13, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(14, new FileStream(dir14, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(15, new FileStream(dir15, FileMode.Append, FileAccess.Write));
            //dicFiles.Add(16, new FileStream(dir16, FileMode.Append, FileAccess.Write));         
            Task.Run(() => ReadData(dicFiles));
          
        }
        private void ReadData(Dictionary<int, FileStream> dicFiles)
        {
            using (FileStream fsReader = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                dispatcherTimer.Start();
                totalSize = fsReader.Length;
                byte[] bytes = new byte[16*8];//4kB是合适的；
                int readNum;
                while ((readNum = fsReader.Read(bytes, 0, bytes.Length)) != 0)//小于说明读完了               
                {
                    readSize += 128;
                    if (IsClose)
                    {
                        break;
                    }
                    var tmpResult = bytes.Length / 8;
                    for (int i = 0; i < tmpResult; i++)
                    {
                        var index = i * 8;
                        byte[] result = new byte[8];
                        for (int j = 0; j < 8; j++)
                        {
                            result[j] = bytes[index + j];
                        }
                        var saveByte = result.Reverse().ToArray();
                        WriteFile(i, saveByte, dicFiles);
                    }                   
                }
                foreach (var item in dicFiles)
                {
                    item.Value.Flush();
                    item.Value.Close();
                    item.Value.Dispose();
                }
                dispatcherTimer.Stop();
                ProgressText = "解析完成!";
                RateText = string.Empty;
                ProgressValue = 100;
            }
        }
        private string CreateDir(string directory)
        {
            //var dir = Properties.Settings.Default.LocalPath + string.Format("\\{0}\\", directory);
            //if (!Directory.Exists(dir))
            //{
            //    Directory.CreateDirectory(dir);
            //}
            var index = FileName.LastIndexOf("\\");
            var name = FileName.Substring(index + 1, FileName.Length - index - 1);
            var dir = FileName.Substring(0, index + 1)+directory;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
        private void WriteFile(int index, byte[] tmpResult, Dictionary<int, FileStream> dicts)
        {
            dicts[index + 1].Write(tmpResult, 0, 8);
            dicts[index + 1].Flush();
        }
        //private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    ProgressText = "下载完毕！";

        //}

        //private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        //{
        //    ProgressText = string.Format("正在下载...{0}%", e.ProgressPercentage.ToString());
        //    ProgressValue = e.ProgressPercentage;
        //}

        private void OnCloseWindow(object obj)
        {
            IsClose = true;
            dispatcherTimer.Stop();
            var window = obj as Window;
            window.Close();
        }
        public string FileName { get; set; }
        public string FullName { get; set; }
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }
        private string _progresstext;
        public string ProgressText
        {
            get { return _progresstext; }
            set { SetProperty(ref _progresstext, value); }
        }
        private bool _btnIsEnable = true;
        public bool  BtnIsEnable
        {
            get { return _btnIsEnable; }
            set { SetProperty(ref _btnIsEnable, value); }
        }
        private string _ratetext;
        public string RateText
        {
            get { return _ratetext; }
            set { SetProperty(ref _ratetext, value); }
        }
        public ICommand CloseWindow { get; private set; }
        public ICommand ScanDataCmd { get; private set; }
    }
}
