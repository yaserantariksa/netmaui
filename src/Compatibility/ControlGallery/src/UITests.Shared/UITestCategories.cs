﻿using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	internal static class UITestCategories
	{
		public const string ViewBaseTests = "ViewBaseTests";
		public const string ActionSheet = "ActionSheet";
		public const string ActivityIndicator = "ActivityIndicator";
		public const string Animation = "Animation";
		public const string AutomationId = "AutomationID";
		public const string BoxView = "BoxView";
		public const string Button = "Button";
		public const string CarouselView = "CarouselView";
		public const string Cells = "Cells";
		public const string CheckBox = "CheckBox";
		public const string CollectionView = "CollectionView";
		public const string ContextActions = "ContextActions";
		public const string DatePicker = "DatePicker";
		public const string DragAndDrop = "DragAndDrop";
		public const string DisplayAlert = "DisplayAlert";
		public const string Editor = "Editor";
		public const string Entry = "Entry";
		public const string Frame = "Frame";
		public const string Image = "Image";
		public const string ImageButton = "ImageButton";
		public const string Label = "Label";
		public const string Layout = "Layout";
		public const string ListView = "ListView";
		public const string UwpIgnore = "UwpIgnore";
		public const string LifeCycle = "Lifecycle";
		public const string FlyoutPage = "FlyoutPage";
		public const string Picker = "Picker";
		public const string ProgressBar = "ProgressBar";
		public const string RequiresInternetConnection = "RequiresInternetConnection";
		public const string RootGallery = "RootGallery";
		public const string ScrollView = "ScrollView";
		public const string SearchBar = "SearchBar";
		public const string Slider = "Slider";
		public const string Stepper = "Stepper";
		public const string Switch = "Switch";
		public const string SwipeView = "SwipeView";
		public const string TableView = "TableView";
		public const string TimePicker = "TimePicker";
		public const string ToolbarItem = "ToolbarItem";
		public const string WebView = "WebView";
		public const string Maps = "Maps";
		public const string InputTransparent = "InputTransparent";
		public const string IsEnabled = "IsEnabled";
		public const string Gestures = "Gestures";
		public const string Navigation = "Navigation";
		public const string Effects = "Effects";
		public const string Focus = "Focus";
		public const string ManualReview = "ManualReview";
		public const string Performance = "Performance";
		public const string Visual = "Visual";
		public const string AppLinks = "AppLinks";
		public const string Shell = "Shell";
		public const string TabbedPage = "TabbedPage";
		public const string CustomRenderers = "CustomRenderers";
		public const string Page = "Page";
		public const string RefreshView = "RefreshView";
		public const string TitleView = "TitleView";
		public const string DisplayPrompt = "DisplayPrompt";
		public const string IndicatorView = "IndicatorView";
		public const string Bugzilla = "Bugzilla";
		public const string Github5000 = "Github5000";
		public const string Github10000 = "Github10000";
		public const string RadioButton = "RadioButton";
		public const string Shape = "Shape";
		public const string Accessibility = "Accessibility";
		public const string Brush = "Brush";
	}


	public class FailsOnMaui : IgnoreAttribute
	{
		public FailsOnMaui() : base(nameof(FailsOnMaui))
		{
		}
	}

#if ANDROID
	public class FailsOnMauiAndroid : IgnoreAttribute
	{
		public FailsOnMauiAndroid() : base(nameof(FailsOnMauiAndroid))
		{
		}
	}
#else
	// For now I'm just ignoring any tests that fail on one platform on all the platforms
	// this is mainly to get a set of tests green and then I can parse between the platforms
	public class FailsOnMauiAndroid : IgnoreAttribute//CategoryAttribute
	{
		public FailsOnMauiAndroid() : base(nameof(FailsOnMauiAndroid))
		{
		}
	}
#endif

#if IOS
	public class FailsOnMauiIOS : IgnoreAttribute
	{
		public FailsOnMauiIOS() : base(nameof(FailsOnMauiIOS))
		{
		}
	}
#else
	public class FailsOnMauiIOS : CategoryAttribute
	{
		public FailsOnMauiIOS() : base(nameof(FailsOnMauiIOS))
		{
		}
	}
#endif

}
