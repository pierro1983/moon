//
// Selector.cs
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace System.Windows.Controls.Primitives {
	public abstract class Selector : ItemsControl {
		internal const string TemplateScrollViewerName = "ScrollViewer";

		internal static readonly DependencyProperty IsSelectionActiveProperty =
			DependencyProperty.RegisterReadOnlyCore ("IsSelectionActive", typeof(bool), typeof(Selector), null); 
		
		public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty =
			DependencyProperty.Register ("IsSynchronizedWithCurrentItem", typeof(bool?), typeof(Selector),
						     new PropertyMetadata (null, new PropertyChangedCallback (OnIsSynchronizedWithCurrentItemChanged)));

		public static readonly DependencyProperty SelectedValueProperty = 
			DependencyProperty.Register ("SelectedValue", typeof (object), typeof (Selector), new PropertyMetadata (null, OnSelectedValueChanged));
		
		public static readonly DependencyProperty SelectedValuePathProperty = 
			DependencyProperty.Register ("SelectedValuePath", typeof (string), typeof (Selector), new PropertyMetadata (null, OnSelectedValuePathChanged));
		
		
		static void OnIsSynchronizedWithCurrentItemChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			((Selector) o).IsSynchronizedWithCurrentItemChanged (o, e);
		}
		
		public static readonly DependencyProperty SelectedIndexProperty =
			DependencyProperty.RegisterCore ("SelectedIndex", typeof(int), typeof(Selector),
						     new PropertyMetadata(-1, new PropertyChangedCallback(OnSelectedIndexChanged)));

		static void OnSelectedIndexChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			((Selector) o).SelectedIndexChanged (o, e);
		}

		// This is not a core property because it is a non-parenting property
		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register ("SelectedItem", typeof(object), typeof(Selector),
						     new PropertyMetadata(new PropertyChangedCallback(OnSelectedItemChanged_cb)));

		
		static void OnSelectedItemChanged_cb (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			((Selector) o).SelectedItemChanged (o, e);
		}

		static void OnSelectedValueChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine ("System.Windows.Controls.Primitives.Selector:OnSelectedValueChanged (): NIEX");
			throw new NotImplementedException ();
		}
		
		static void OnSelectedValuePathChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Console.WriteLine ("System.Windows.Controls.Primitives.Selector:OnSelectedValuePathChanged (): NIEX");
			throw new NotImplementedException ();
		}

		internal static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Selector s = (Selector) d;
			Style style = (Style) e.NewValue;

			int count = s.Items.Count;
			for (int i = 0; i < count; i++)
			{ 
				ListBoxItem item = (ListBoxItem) s.ItemContainerGenerator.ContainerFromIndex (i);
				if (item != null)  // May be null if GetContainerForItemOverride has not been called yet
					item.Style = style;
			}	
		}

		internal Selector ()
		{
			// Set default values for ScrollViewer attached properties 
			ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);
			ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
			Selection = new Selection (this);
		}
		
		bool SynchronizeWithCurrentItem {
			get {
				bool? sync = IsSynchronizedWithCurrentItem;
				
				return (ItemsSource is ICollectionView) && (!sync.HasValue || sync.Value);
			}
		}

		internal bool IsSelectionActive {
			get { return (bool) GetValue (IsSelectionActiveProperty); }
			set { SetValueImpl (IsSelectionActiveProperty, value); }
		}

		[TypeConverter (typeof (NullableBoolConverter))]
		public bool? IsSynchronizedWithCurrentItem {
			get { return (bool?) GetValue (IsSynchronizedWithCurrentItemProperty); }
			set {
				if (value.HasValue && value.Value)
					throw new ArgumentException ();
				
				SetValue (IsSynchronizedWithCurrentItemProperty, value);
			}
		}

		[Mono.Xaml.SetPropertyDelayed]
		public int SelectedIndex {
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue (SelectedIndexProperty, value); }
		}

		public object SelectedItem {
			get { return GetValue (SelectedItemProperty); }
			set { SetValue (SelectedItemProperty, value); }
		}

		public object SelectedValue {
			get { return GetValue(SelectedValueProperty); }
			set { SetValue (SelectedValueProperty, value); }
		}

		public string SelectedValuePath {
			get { return (string) GetValue (SelectedValuePathProperty); }
			set { SetValue (SelectedValuePathProperty, value); }
		}
		
		internal Selection Selection {
			get; private set;
		}

		internal ScrollViewer TemplateScrollViewer {
			get; private set;
		}
		
		void OnCurrentItemChanged (object sender, EventArgs args)
		{
			if (SynchronizeWithCurrentItem)
				Selection.Select (((ICollectionView) ItemsSource).CurrentItem);
		}
		
		internal override void OnItemsSourceChanged (IEnumerable oldSource, IEnumerable newSource)
		{
			if (oldSource is ICollectionView)
				(oldSource as ICollectionView).CurrentChanged -= OnCurrentItemChanged;
			
			if (newSource is ICollectionView)
				(newSource as ICollectionView).CurrentChanged += OnCurrentItemChanged;
			
			base.OnItemsSourceChanged (oldSource, newSource);
		}
		
		public event SelectionChangedEventHandler SelectionChanged;
		
		void IsSynchronizedWithCurrentItemChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			bool? sync = (bool?) e.NewValue;
			
			if ((!sync.HasValue || sync.Value) && ItemsSource is ICollectionView) {
				ICollectionView view = ItemsSource as ICollectionView;
				
				// synchronize with the current item
				view.MoveCurrentTo (SelectedItem);
			}
		}
		
		void SelectedIndexChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (Selection.Updating)
				return;

			var newVal = (int) e.NewValue;
			if (newVal < 0 || newVal >= Items.Count)
				Selection.Select (null);
			else
				Selection.Select (Items [newVal]);
		}
		
		void SelectedItemChanged (DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (Selection.Updating)
				return;

			// If the new item is null we clear our selection. If it is non-null
			// and not in the Items array, then we revert to the old selection as
			// we can't select something which is not in the Selector.
			if (e.NewValue == null) {
				Selection.Select (null);
			} else if (Items.IndexOf (e.NewValue) == -1) {
				Selection.Select (e.OldValue);
			} else {
				Selection.Select (e.NewValue);
			}
		}
		
		void OnSelectedItemChanged (object [] oldValues, object [] newValues)
		{
			foreach (var oldValue in oldValues) {
				if (oldValue != null) {
					var oldItem =  (ListBoxItem) ((oldValue as ListBoxItem) ?? ItemContainerGenerator.ContainerFromItem (oldValue));
	
					if (oldItem != null)
						oldItem.IsSelected = false;
				}
			}

			foreach (var newValue in newValues) {
				if (newValue != null) {
					var newItem =  (ListBoxItem) ((newValue as ListBoxItem) ?? ItemContainerGenerator.ContainerFromItem (newValue));
	
					if (newItem != null) {
						newItem.IsSelected = true;
						// FIXME: Sometimes the item should be focused and sometimes it shouldn't
						// I think that the selector won't steal focus from an element which isn't
						// a child of the selector.
						// Testcase:
						// 1) Open the Controls Toolkit.
						// 2) Click on a demo in the treeview
						// 3) Try to shrink the source textbox view.
						// Result: The view requires 2 clicks to collapse it. Subsequent attempts work on the first click.
						// This 'bug' should only happen if you change the source view tab manually, i.e. if you change the
						// source file being displayed you will need two clicks to collapse the view.
						newItem.Focus ();
					}
				}

				if (SynchronizeWithCurrentItem)
					(ItemsSource as ICollectionView).MoveCurrentTo (newValue);
			}
		}

		internal void RaiseSelectionChanged (object [] oldVals, object [] newVals)
		{
			oldVals = oldVals ?? new object [0];
			newVals = newVals ?? new object [0];
			OnSelectedItemChanged (oldVals, newVals);
			
			SelectionChangedEventHandler h = SelectionChanged;
			if (h != null)
				h (this, new SelectionChangedEventArgs (oldVals, newVals));
		}

		public static bool GetIsSelectionActive (DependencyObject element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			return (bool) element.GetValue (ListBox.IsSelectionActiveProperty);
		}

		protected override void ClearContainerForItemOverride (DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride (element, item);
			ListBoxItem lbItem = (ListBoxItem) element;
			lbItem.ParentSelector = null;
			if (element != item)
				lbItem.Content = null;
		}

		protected override void PrepareContainerForItemOverride (DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride (element, item);
			ListBoxItem listBoxItem = (ListBoxItem) element; 
			listBoxItem.ParentSelector = this; 
			if (listBoxItem.IsSelected && ItemContainerGenerator.ContainerFromIndex (SelectedIndex) != null)
				Selection.Select (listBoxItem);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();
			TemplateScrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
			
			if (TemplateScrollViewer != null)
			{
				TemplateScrollViewer.TemplatedParentHandlesScrolling = true;
				// Update ScrollViewer values
				TemplateScrollViewer.HorizontalScrollBarVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(this); 
				TemplateScrollViewer.VerticalScrollBarVisibility = ScrollViewer.GetVerticalScrollBarVisibility(this); 
			}
		}

		protected override void OnItemsChanged (NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				ListBoxItem item = e.NewItems [0] as ListBoxItem;
				if (item != null && item.IsSelected) {
					Selection.Select (item);
				} else if (SelectedItem != null) {
					// The index of our selected item may have changed, so we need to
					// reselect it to refresh the SelectedIndex property. This won't raise
					// a SelectionChanged event as the actual object is the same.
					Selection.Select (SelectedItem);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				Selection.Select (null);
				break;
				
			case NotifyCollectionChangedAction.Remove:
				if (e.OldItems [0] == SelectedItem) {
					Selection.Select (null);
				} else if (e.OldStartingIndex <= SelectedIndex) {
					Selection.Select (SelectedItem);
				}
				break;
			case NotifyCollectionChangedAction.Replace:
				Selection.Select (e.OldItems [0]);
				break;
			default:
				throw new NotSupportedException (string.Format ("Collection changed action '{0}' not supported", e.Action));
			}
			base.OnItemsChanged (e);
		}
		
		internal virtual void NotifyListItemClicked(ListBoxItem listBoxItem) 
		{
			Selection.Select (ItemContainerGenerator.ItemFromContainer (listBoxItem));
		}
		
		internal virtual void NotifyListItemLoaded (ListBoxItem listBoxItem)
		{
			if (ItemContainerGenerator.ItemFromContainer (listBoxItem) == SelectedItem) {
				listBoxItem.IsSelected = true;
				listBoxItem.Focus ();
			}
		}
		
		internal virtual void NotifyListItemGotFocus(ListBoxItem listBoxItemNewFocus)
		{
			
		}
		
		internal virtual void NotifyListItemLostFocus(ListBoxItem listBoxItemOldFocus)
		{
			
		}
	}
}
