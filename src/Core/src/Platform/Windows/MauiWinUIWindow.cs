﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Microsoft.Maui
{
	public class MauiWinUIWindow : UI.Xaml.Window, IPlatformSizeRestrictedWindow
	{
		static readonly SizeInt32 DefaultMinimumSize = new SizeInt32(0, 0);
		static readonly SizeInt32 DefaultMaximumSize = new SizeInt32(int.MaxValue, int.MaxValue);

		readonly WindowMessageManager _windowManager;

		IntPtr _windowIcon;
		bool _enableResumeEvent;
		bool _isActivated;

		public MauiWinUIWindow()
		{
			_windowManager = WindowMessageManager.Get(this);

			Activated += OnActivated;
			Closed += OnClosedPrivate;
			VisibilityChanged += OnVisibilityChanged;

			// We set this to true by default so later on if it's
			// set to false we know the user toggled this to false 
			// and then we can react accordingly
			if (AppWindowTitleBar.IsCustomizationSupported())
				base.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

			SubClassingWin32();
			SetIcon();
		}

		protected virtual void OnActivated(object sender, UI.Xaml.WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != UI.Xaml.WindowActivationState.Deactivated)
			{
				// We have to track isActivated calls because WinUI will call OnActivated Twice
				// when maximizing a window
				// https://github.com/microsoft/microsoft-ui-xaml/issues/7343
				if (_isActivated)
					return;

				_isActivated = true;

				if (_enableResumeEvent)
					Services?.InvokeLifecycleEvents<WindowsLifecycle.OnResumed>(del => del(this));
				else
					_enableResumeEvent = true;
			}
			else
			{
				_isActivated = false;
			}

			Services?.InvokeLifecycleEvents<WindowsLifecycle.OnActivated>(del => del(this, args));
		}

		private void OnClosedPrivate(object sender, UI.Xaml.WindowEventArgs args)
		{
			OnClosed(sender, args);

			if (_windowIcon != IntPtr.Zero)
			{
				DestroyIcon(_windowIcon);
				_windowIcon = IntPtr.Zero;
			}

			Window = null;
		}

		protected virtual void OnClosed(object sender, UI.Xaml.WindowEventArgs args)
		{
			Services?.InvokeLifecycleEvents<WindowsLifecycle.OnClosed>(del => del(this, args));
		}

		protected virtual void OnVisibilityChanged(object sender, UI.Xaml.WindowVisibilityChangedEventArgs args)
		{
			Services?.InvokeLifecycleEvents<WindowsLifecycle.OnVisibilityChanged>(del => del(this, args));
		}

		public IntPtr WindowHandle => _windowManager.WindowHandle;

		void SubClassingWin32()
		{
			Services?.InvokeLifecycleEvents<WindowsLifecycle.OnPlatformWindowSubclassed>(
				del => del(this, new WindowsPlatformWindowSubclassedEventArgs(WindowHandle)));

			_windowManager.WindowMessage += OnWindowMessage;

			void OnWindowMessage(object? sender, WindowMessageEventArgs e)
			{
				if (e.MessageId == PlatformMethods.MessageIds.WM_GETMINMAXINFO)
				{
					var win = this as IPlatformSizeRestrictedWindow;
					var minSize = win.MinimumSize;
					var maxSize = win.MaximumSize;

					var changedMinSize = minSize != DefaultMinimumSize;
					var changedMaxSize = maxSize != DefaultMaximumSize;

					if (changedMinSize || changedMaxSize)
					{
						var rect = Marshal.PtrToStructure<PlatformMethods.MinMaxInfo>(e.LParam);

						if (changedMinSize)
						{
							var newMinSize = new PlatformMethods.POINT
							{
								X = Math.Max(minSize.Width, rect.MinTrackSize.X),
								Y = Math.Max(minSize.Height, rect.MinTrackSize.Y)
							};
							rect.MinTrackSize = newMinSize;
						}

						if (changedMaxSize)
						{
							var newMaxSize = new PlatformMethods.POINT
							{
								X = Math.Min(maxSize.Width, rect.MaxTrackSize.X),
								Y = Math.Min(maxSize.Height, rect.MaxTrackSize.Y)
							};
							rect.MaxTrackSize = newMaxSize;
						}

						Marshal.StructureToPtr(rect, e.LParam, true);
					}
				}

				Services?.InvokeLifecycleEvents<WindowsLifecycle.OnPlatformMessage>(
					m => m.Invoke(this, new WindowsPlatformMessageEventArgs(e.Hwnd, e.MessageId, e.WParam, e.LParam)));
			}
		}

		/// <summary>
		/// Default the Window Icon to the icon stored in the .exe, if any.
		/// 
		/// The Icon can be overriden by callers by calling SetIcon themselves.
		/// </summary>
		void SetIcon()
		{
			var processPath = Environment.ProcessPath;
			if (!string.IsNullOrEmpty(processPath))
			{
				var index = IntPtr.Zero; // 0 = first icon in resources
				_windowIcon = ExtractAssociatedIcon(IntPtr.Zero, processPath, ref index);
				if (_windowIcon != IntPtr.Zero)
				{
					var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowHandle));
					if (appWindow is not null)
					{
						var iconId = Win32Interop.GetIconIdFromIcon(_windowIcon);
						appWindow.SetIcon(iconId);
					}
				}
			}
		}

		SizeInt32 IPlatformSizeRestrictedWindow.MinimumSize { get; set; } = DefaultMinimumSize;

		SizeInt32 IPlatformSizeRestrictedWindow.MaximumSize { get; set; } = DefaultMaximumSize;

		internal IWindow? Window { get; private set; }

		internal IServiceProvider? Services =>
			Window?.Handler?.GetServiceProvider() ??
			MauiWinUIApplication.Current.Services;

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, string iconPath, ref IntPtr index);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int DestroyIcon(IntPtr hIcon);

		internal void SetWindow(IWindow window)
		{
			Window = window;
		}
	}

	interface IPlatformSizeRestrictedWindow
	{
		SizeInt32 MinimumSize { get; set; }

		SizeInt32 MaximumSize { get; set; }
	}
}
