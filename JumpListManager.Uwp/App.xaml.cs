using JumpListManager.Views;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JumpListManager
{
	public sealed partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			Suspending += OnSuspending;
		}

		/// <inheritdoc/>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active.
			if (Window.Current.Content is not Frame rootFrame)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					// TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
					ApplicationView.GetForCurrentView().Title = "Jump List Manager";

					// When the navigation stack isn't restored navigate to the first page, configuring
					// the new page by passing required information as a navigation parameter.
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}

				// Ensure the current window is active
				Window.Current.Activate();
			}
		}

		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception($"Failed to load page '{e.SourcePageType.FullName}'.");
		}

		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

			// TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}
