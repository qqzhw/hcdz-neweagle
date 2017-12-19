
using Microsoft.Practices.ServiceLocation;
using Pvirtech.QyRound.Core.Common;
using Pvirtech.QyRound.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Pvirtech.QyRound
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
	}
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private static Mutex SingleInstanceMutex = new Mutex(true, "{56A802DF-C96C-8769-BAA8-1BC527857BEB}");

		private static bool SingleInstanceCheck()
		{

			if (!SingleInstanceMutex.WaitOne(TimeSpan.Zero, true))
			{
				Process thisProc = Process.GetCurrentProcess();
				Process process = Process.GetProcessesByName(thisProc.ProcessName).FirstOrDefault(delegate (Process p)
				{
					if (p.Id != thisProc.Id)
					{
						return true;
					}
					return false;
				});
				if (process != null)
				{
					IntPtr mainWindowHandle = process.MainWindowHandle;
					if (NativeMethods.IsIconic(mainWindowHandle))
					{
						NativeMethods.ShowWindow(mainWindowHandle, 9);
					}
					NativeMethods.SetForegroundWindow(mainWindowHandle);
				}
				Application.Current.Shutdown(1);
				return false;
			}
			return true;
		}

		protected override void OnStartup(StartupEventArgs e)
		{           
            if (!SingleInstanceCheck())
			{
				return;
			}
            //double seconds = 1259666013 + 28800;
            //double secs = Convert.ToDouble(seconds);
            //DateTime dt = new DateTime(
            //1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(secs);
            ////TimeSpan span = 
            ////        TimeSpan.FromTicks(seconds*TimeSpan.TicksPerSecond); 
            //Console.WriteLine(dt);
            var s = 7;
            var t1 = (s >> 0) & 1;
            var data6 = 255;
            for (int i = 0; i < 8; i++)
            {
                var bv = data6 >> i & 1;
                switch (i)
                {
                    case 0:
                         
                        break;
                    default:
                        break;
                }
            }
            base.OnStartup(e);
			log4net.Config.XmlConfigurator.Configure();
			Initialize();
		}

		public void Initialize()
		{
			this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			Bootstrapper bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogHelper.ErrorLog((Exception)e.ExceptionObject);
            var gathervm=ServiceLocator.Current.GetInstance<GatherViewModel>();
            if (gathervm!=null)
            {
                gathervm.OnSendData(0x03, 0x02, 0x00);
            }
            var sdkvm = ServiceLocator.Current.GetInstance<SDKViewModel>();
            if (sdkvm!=null)
            {               
                Thread.Sleep(500);
                sdkvm.OnStopRecod();                
            }
            LogHelper.WriteLog("程序出现异常,已停止读取!");
        }

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LogHelper.ErrorLog(e.Exception);

            var gathervm = ServiceLocator.Current.GetInstance<GatherViewModel>();
            if (gathervm != null)
            {
                gathervm.OnSendData(0x03, 0x02, 0x00);
            }
            var sdkvm = ServiceLocator.Current.GetInstance<SDKViewModel>();
            if (sdkvm != null)
            {
                Thread.Sleep(500);
                sdkvm.OnStopRecod();
            }
            LogHelper.WriteLog("程序出现异常,已停止读取!");
        }

		internal static void FlushMemory()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				NativeMethods.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
			}
			GC.Collect();
		}
	}
}
