
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Pvirtech.QyRound.Properties;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Pvirtech.QyRound.Models;
using System;
using Pvirtech.QyRound.SDK;

namespace Pvirtech.QyRound.ViewModels
{
	public class SettingsViewModel: BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container; 
		private readonly IServiceLocator _serviceLocator; 
		public SettingsViewModel(IUnityContainer container, IEventAggregator eventAggregator, IServiceLocator serviceLocator)
		{
			_container = container;
			_eventAggregator = eventAggregator; 
			_serviceLocator = serviceLocator;
            _netCardInfos = new ObservableCollection<NetCardInfo>();
			SaveCommand = new DelegateCommand(OnSaveData);
			Initializer();
            InitNetCards();
		}

        public void InitNetCards()
        {
            SelectNetCardCmd = new DelegateCommand<NetCardInfo>(OnSelectNetCard);
        }
        /// <summary>
        /// 网卡选择，更新连接设备
        /// </summary>
        /// <param name="netCardInfo"></param>
        private void OnSelectNetCard(NetCardInfo netCardInfo)
        {
            CurrentNetCard = netCardInfo.dev_description;
            var index = NetCardInfos.IndexOf(netCardInfo);
            Settings.Default.NetCardIndex = index;
            Settings.Default.Save();
            Settings.Default.Reload();
            SetCurrentNetCard();
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
        private void OnSaveData()
		{
            Settings.Default.Byte0 = Byte0;
            Settings.Default.Byte1 = Byte1;
            Settings.Default.FrameTop = FrameTop;
            Settings.Default.CjIP = CjIP;
            Settings.Default.CjPort = CjPort;
            Settings.Default.SpIP = SpIP;
            Settings.Default.SpPort = SpPort;
            Settings.Default.TaskName = TaskName;
            //Settings.Default.DwonloadUrl = DownloadUrl;
            Settings.Default.LocalPath = LocalPath;
            Settings.Default.Save();
            Settings.Default.Reload();
        }

		private void Initializer()
		{
            _byte0 = Settings.Default.Byte0;
            _byte1 = Settings.Default.Byte1;
            _frameTop = Settings.Default.FrameTop;
            _cjIp = Settings.Default.CjIP;
            _cjPort = Settings.Default.CjPort;
            _spIp = Settings.Default.SpIP;
           _spPort= Settings.Default.SpPort;
            _taskName = Settings.Default.TaskName;
            _localPath = Settings.Default.LocalPath;
        }
        #region 属性
        private string _localPath;
        public string LocalPath
        {
            get { return _localPath; }
            set { SetProperty(ref _localPath, value); }
        }
        private string _taskName;
        public string TaskName
        {
            get { return _taskName; }
            set { SetProperty(ref _taskName, value); }
        }
        private byte _byte0;
		public byte Byte0
		{
			get { return _byte0; }
			set { SetProperty(ref _byte0, value); }
		}
		private byte _byte1;
		public byte Byte1
        {
			get { return _byte1; }
			set { SetProperty(ref _byte1, value); }
		}

        private byte _frameTop;
        public byte FrameTop
        {
            get { return _frameTop; }
            set { SetProperty(ref _frameTop, value); }
        }
        private string _cjIp;
		public string CjIP
		{
			get { return _cjIp; }
			set { SetProperty(ref _cjIp, value); }
		}
		private int _cjPort;
		public int CjPort
		{
			get { return _cjPort; }
			set { SetProperty(ref _cjPort, value); }
		}
		private string _spIp;
		public string SpIP
		{
			get { return _spIp; }
			set { SetProperty(ref _spIp, value); }
		}
		private int _spPort;
		public int SpPort
        {
			get { return _spPort; }
			set { SetProperty(ref _spPort, value); }
		}
        private string _currentNetCard;
        public string CurrentNetCard
        {
            get { return _currentNetCard; }
            set { SetProperty(ref _currentNetCard, value); }
        }
        
        private ObservableCollection<NetCardInfo> _netCardInfos;
        public ObservableCollection<NetCardInfo> NetCardInfos
        {
            get { return _netCardInfos; }
            set { SetProperty(ref _netCardInfos, value); }
        }
        public ICommand SaveCommand { get; private set; }
        public ICommand SelectNetCardCmd { get; private set; }

        #endregion
    }
}
