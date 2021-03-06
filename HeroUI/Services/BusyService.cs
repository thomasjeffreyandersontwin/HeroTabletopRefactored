﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HeroUI
{
    public interface BusyService
    {
        void ShowBusy();
        void ShowBusy(string text);
        void ShowBusy(string[] windowNames);
        void HideBusy();
        void HideAllBusy();
        bool IsShowingBusy { get; }

    }

    public class BusyServiceImpl : BusyService, IHandle<PopupOpenedEvent>
    {
        #region Properties
        
        private int counter;

        private List<BusyControl> busyControls;
        private List<Window> targetWindows;
        private List<FrameworkElement> targetElements;

        private IEventAggregator eventAggregator;

        public bool IsShowingBusy
        {
            get { return counter > 0; }
        }

        #endregion

        #region Constructors

        public BusyServiceImpl(IEventAggregator eventAggregator) 
        {
            counter = 0;

            this.eventAggregator = eventAggregator;

            busyControls = new List<BusyControl>();
            targetWindows = new List<Window>();
            targetElements = new List<FrameworkElement>();

            //container.Resolve<IEventAggregator>().GetEvent<PopupOpened>().Subscribe(this.PopupOpened);
        }

        #endregion Constructors

        public void Handle(PopupOpenedEvent message)
        {
            if (!IsShowingBusy)
                return;
            Window win = message.PopupWindow;
            win.Loaded += this.win_Loaded;
        }

        void win_Loaded(object sender, RoutedEventArgs e)
        {
            ((Window)sender).Loaded -= this.win_Loaded;

            if (!IsShowingBusy)
                return;

            AddBusyToPopup((Window)sender);
        }

        void AddBusyToPopup(Window targetWindow)
        {
            if (targetWindows.Contains(targetWindow))
                return;

            FrameworkElement targetElement =
                typeof(FrameworkElement).IsAssignableFrom(targetWindow.Content.GetType()) ? (FrameworkElement)targetWindow.Content : null;

            targetElements.Add(targetElement);

            if (targetElement != null)
            {
                BusyControl busyControl = new BusyControl();
                targetElement.SetValue(AdornerBehavior.ControlProperty, busyControl);
                targetElement.SetValue(AdornerBehavior.ShowAdornerProperty, true);

                busyControl.BeginAnimation();

                busyControls.Add(busyControl);
            }
            else
            {
                busyControls.Add(null);
            }

            targetWindows.Add(targetWindow);
        }

        void RemoveBusyFromPopup(Window targetWindow)
        {
            if (!targetWindows.Contains(targetWindow))
                return;

            targetWindows.Remove(targetWindow);
        }

        #region Public Methods

        public void ShowBusy(string[] windowNames)
        {
            ShowBusy(string.Empty, windowNames);
        }
        public void ShowBusy()
        {
            ShowBusy(string.Empty, null); // show default text (i.e. "Loading...")
        }
        public void ShowBusy(string text)
        {
            ShowBusy(text, null);
        }

        public void ShowBusy(string text, string[] windowNames)
        {
            counter++;
            if (counter > 1) // busy is already showing
                return;

            // Get all windows
            targetWindows = Application.Current.Windows.OfType<Window>().ToList();

            //targetWindows = new List<Window>{Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive)};
            // Get the element on which busy control will be shown
            foreach (var targetWindow in targetWindows)
            {
                //FrameworkElement targetElement =
                //    typeof(FrameworkElement).IsAssignableFrom(targetWindow.Content.GetType()) ? (FrameworkElement)targetWindow.Content : null;
                FrameworkElement targetElement = null;
                if (targetWindow.Content.GetType() == typeof(System.Windows.Controls.Grid))
                {
                    targetElement = (FrameworkElement)targetWindow.Content;
                }
                else if (targetWindow.Content.GetType() == typeof(System.Windows.Documents.AdornerDecorator)) // Hack for popup windows
                {
                    var decorator = targetWindow.Content as System.Windows.Documents.AdornerDecorator;
                    var grid = decorator.Child as System.Windows.Controls.Grid;
                    if (grid != null)
                        targetElement = grid;
                }
                else
                {
                    targetElement = typeof(FrameworkElement).IsAssignableFrom(targetWindow.Content.GetType()) ? (FrameworkElement)targetWindow.Content : null;
                }
                targetElements.Add(targetElement);
                if (targetElement != null)
                {
                    BusyControl busyControl = new BusyControl();
                    if (!string.IsNullOrEmpty(text))
                        busyControl.tbLoadingText.Text = text;
                    targetElement.SetValue(AdornerBehavior.ControlProperty, busyControl);
                    targetElement.SetValue(AdornerBehavior.ShowAdornerProperty, true);
                    if (windowNames != null && !windowNames.Contains(targetWindow.Name))
                        busyControl.OverlayWithoutAnimation();
                    else
                        busyControl.BeginAnimation();

                    busyControls.Add(busyControl);
                }
                else
                {
                    busyControls.Add(null);
                }
            }
        }

        public void HideBusy()
        {
            if (!IsShowingBusy) // busy is not showing, so no need to hide
                return;

            counter--;
            if (counter > 0)  // continue showing busy until counter == 0
                return;

            System.Action d =
                delegate ()
                {
                    for (int i = 0; i < targetWindows.Count; i++)
                    {
                        //var targetWindow = targetWindows[i];
                        var targetElement = targetElements[i];
                        var busyControl = busyControls[i];

                        if (targetElement != null)
                        {
                            busyControl.StopAnimation();

                            targetElement.SetValue(AdornerBehavior.ControlProperty, null);
                            targetElement.SetValue(AdornerBehavior.ShowAdornerProperty, false);
                            targetElement = null;
                        }
                    }

                    targetWindows.Clear();
                    targetElements.Clear();
                    busyControls.Clear();
                };
            Application.Current.Dispatcher.BeginInvoke(d);
        }

        public void HideAllBusy()
        {
            counter = 1;
            HideBusy();
        }

        #endregion Public Methods
    }
}
