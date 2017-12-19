using Microsoft.Practices.ServiceLocation;
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

namespace Pvirtech.QyRound.ViewModels
{
    
    public class ReadAdDataViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IServiceLocator _serviceLocator;
        private DispatcherTimer dispatcherTimer;
        private long readSize = 0;
        private long tmpSize = 0;
        private long totalSize = 0;
        private bool IsClose = false;
        public ReadAdDataViewModel(IUnityContainer container, IServiceLocator serviceLocator, string fileName)
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
            ProgressText = "正在解析AD数据...";
            Init();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            RateText = string.Format("{0}MB/s", ((readSize - tmpSize) / 1048576.0).ToString("f2"));
            tmpSize = readSize;
            ProgressValue = (int)(readSize * 100 / totalSize);
        }

        private void Init()
        {
            var index = FileName.LastIndexOf("\\");
            var name = FileName.Substring(index + 1, FileName.Length - index - 1);
             
            var saveFilePath = FileName.Substring(0,index + 1) ;
            for (int i = 1; i <= 16; i++)
            {
                var dirName = string.Format("AdBar{0}",i);
                if (!Directory.Exists(dirName))
                {
                    var result = CreateDir(dirName);
                }              
            }
            
            Dictionary<int, StreamWriter> dicFiles = new Dictionary<int, StreamWriter>();
            for (int i =1; i <=16; i++)
            {
                var dirName = string.Format("AdBar{0}\\{1}-{2}.dat", i,name,DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                dicFiles.Add(i, new StreamWriter(saveFilePath+dirName,true));
            }
         
            Task.Run(() => ReadData(dicFiles));

        }
        private void ReadData(Dictionary<int, StreamWriter> dicFiles)
        {
            using (FileStream fsReader = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                dispatcherTimer.Start();
                totalSize = fsReader.Length;
                byte[] bytes = new byte[16 * 8];//4kB是合适的；
                long readNum;
                long noIndex = 0;//过滤数据头 年月日秒通道
             //   while ((readNum = fsReader.Read(bytes, 0, bytes.Length)) != 0)//小于说明读完了
               while ((readNum = fsReader.Read(bytes, 0, bytes.Length)) != 0)//只读取1M 数据
                    {
                    noIndex++;
                    if (noIndex<4)
                    {
                        continue;
                    }
                    readSize += 128;
                    if (readSize>= 6711168)
                    {
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
                        break;
                    }                   
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
                        var strByte = string.Empty;
                        //var s = saveByte[0] << 8;
                        for (int n = 1;n <=8; n++)
                        {
                            var tmpStr = string.Empty;// string.Format("{0:X2}", saveByte[n-1]);
                            if (n%2==0)
                            {
                                int tmpValue = (saveByte[n - 2] << 8) + saveByte[n - 1];
                                if (tmpValue>32768)
                                {
                                    tmpStr = (tmpValue - 65536).ToString();
                                    tmpStr += "\n";
                                }
                                else
                                {
                                    tmpStr = tmpValue.ToString();
                                    tmpStr += "\n";
                                }
                              
                            }
                            strByte += tmpStr;
                        }
                        WriteFile(i, strByte, dicFiles);
                    }
                }
             
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
        private void WriteFile(int index, string tmpResult, Dictionary<int, StreamWriter> dicts)
        { 
           
            dicts[index + 1].Write(tmpResult);
             dicts[index + 1].Flush();
        }   

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
        public bool BtnIsEnable
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
