﻿using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.ViewModels
{
	public class BanAnalysisViewModel : BindableBase
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IUnityContainer _container;
		private readonly IServiceLocator _serviceLocator;
		public BanAnalysisViewModel(IUnityContainer container, IEventAggregator eventAggregator, IServiceLocator serviceLocator)
		{
			_container = container;
			_eventAggregator = eventAggregator;
			_serviceLocator = serviceLocator;

		}
	}
}
