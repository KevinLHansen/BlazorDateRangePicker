﻿/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDateRangePicker
{
    public class DatePickerComponentBase : ComponentBase, IConfigurableOptions
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Guid for container id used for JSInterop 
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Guid for parent id used for JSInterop 
        /// </summary>
        public string ParentId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Attach a named properties config object to this instance of datepicker
        /// </summary>
        [Parameter]
        public string Config { get; set; }

        /// <summary>
        /// All unmatched parameters will be passed to parent element
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        private Dictionary<string, object> ConfigAttributes { get; set; }

        protected private Dictionary<string, object> CombinedAttributes
        {
            get
            {
                var combined = new Dictionary<string, object>();
                if (ConfigAttributes != null)
                {
                    foreach (var attr in ConfigAttributes)
                    {
                        combined[attr.Key] = attr.Value;
                    }
                }

                if (Attributes != null)
                {
                    foreach (var attr in Attributes)
                    {
                        combined[attr.Key] = attr.Value;
                    }
                }
                return combined;
            }
        }

        /// <summary>
        /// Set predefined date ranges the user can select from. Each RangeItem.Name is the label for the range, and its Start and End value representing the bounds of the range.
        /// </summary>
        [Parameter]
        public Dictionary<string, DateRange> Ranges { get; set; }

        /// <summary>
        /// Hide the apply and cancel buttons, and automatically apply a new date range as soon as two dates are clicked.
        /// </summary>
        [Parameter]
        public bool? AutoApply { get; set; }

        /// <summary>
        /// Show only a single calendar to choose one date, instead of a range picker with two calendars. The start and end dates provided to your callback will be the same single date chosen. 
        /// </summary>
        [Parameter]
        public bool? SingleDatePicker { get; set; }

        /// <summary>
        /// Show only one calendar in the picker instead of two calendars.
        /// </summary>
        [Parameter]
        public bool? ShowOnlyOneCalendar { get; set; }

        /// <summary>
        /// Normally, if you use the ranges option to specify pre-defined date ranges, calendars for choosing a custom date range are not shown until the user clicks "Custom Range". When this option is set to true, the calendars for choosing a custom date range are always shown instead. 
        /// </summary>
        [Parameter]
        public bool? AlwaysShowCalendars { get; set; }

        /// <summary>
        /// CSS class names that will be added to both the apply and cancel buttons.
        /// </summary>
        [Parameter]
        public string ButtonClasses { get; set; }

        /// <summary>
        /// CSS class names that will be added only to the apply button. 
        /// </summary>
        [Parameter]
        public string ApplyButtonClasses { get; set; }

        /// <summary>
        /// CSS class names that will be added only to the cancel button. 
        /// </summary>
        [Parameter]
        public string CancelButtonClasses { get; set; }

        [Parameter]
        public string ApplyLabel { get; set; }

        [Parameter]
        public string CancelLabel { get; set; }

        [Parameter]
        public string CustomRangeLabel { get; set; }

        /// <summary>
        /// The beginning date of the initially selected date range.
        /// </summary>
        [Parameter]
        public DateTimeOffset? StartDate { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset?> StartDateChanged { set; get; }

        /// <summary>
        /// The end date of the initially selected date range
        /// </summary>
        [Parameter]
        public DateTimeOffset? EndDate { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset?> EndDateChanged { set; get; }

        /// <summary>
        /// Specify the format string to display dates, default is Culture.DateTimeFormat.ShortDatePattern
        /// </summary>
        [Parameter]
        public string DateFormat { get; set; }

        /// <summary>
        /// Show localized week numbers at the start of each week on the calendars.
        /// </summary>
        [Parameter]
        public bool? ShowWeekNumbers { get; set; }

        /// <summary>
        /// Show ISO week numbers at the start of each week on the calendars.
        /// </summary>
        [Parameter]
        public bool? ShowISOWeekNumbers { get; set; }

        /// <summary>
        /// When enabled, the two calendars displayed will always be for two sequential months (i.e. January and February), and both will be advanced when clicking the left or right arrows above the calendars. When disabled, the two calendars can be individually advanced and display any month/year.
        /// </summary>
        [Parameter]
        public bool? LinkedCalendars { get; set; }

        /// <summary>
        /// Show year and month select boxes above calendars to jump to a specific month and year.
        /// </summary>
        [Parameter]
        public bool? ShowDropdowns { get; set; } = true;

        /// <summary>
        /// Displays "Custom Range" at the end of the list of predefined ranges, when the ranges option is used. This option will be highlighted whenever the current date range selection does not match one of the predefined ranges. Clicking it will display the calendars to select a new range.
        /// </summary>
        [Parameter]
        public bool? ShowCustomRangeLabel { get; set; } = true;

        /// <summary>
        /// Inline mode
        /// </summary>
        [Parameter]
        public bool? Inline { get; set; } = false;

        /// <summary> Specify the culture to display dates and text in. Default is CultureInfo.CurrentCulture.</summary>
        [Parameter]
        public System.Globalization.CultureInfo Culture { get; set; }

        /// <summary>The text to display on the Week number heading</summary>
        [Parameter]
        public string WeekAbbreviation { get; set; }

        /// <summary>The day of the week to start from</summary>
        [Parameter]
        public DayOfWeek? FirstDayOfWeek { get; set; }

        /// <summary>The earliest date that can be selected, inclusive. A value of null indicates that there is no minimum date.</summary>
        [Parameter]
        public DateTimeOffset? MinDate { get; set; }

        /// <summary>The latest date that can be selected, inclusive. A value of null indicates that there is no maximum date.</summary>
        [Parameter]
        public DateTimeOffset? MaxDate { get; set; }

        /// <summary>
        /// The maximum TimeSpan between the selected start and end dates. A value of null indicates that there is no limit.
        /// </summary>
        [Parameter]
        public TimeSpan? MaxSpan { get; set; }

        /// <summary>
        /// Picker popup visibility. Use Open() instead.
        /// </summary>
        [Parameter]
        public bool Visible { get; set; }

        /// <summary>
        /// Whether the picker should close on outside click
        /// </summary>
        [Parameter]
        public bool? CloseOnOutsideClick { get; set; }

        /// <summary>
        /// Whether the picker should pick months based on selected range
        /// </summary>
        [Parameter]
        public bool? AutoAdjustCalendars { get; set; }

        /// <summary>
        /// Whether the picker appears aligned to the left, to the right, or centered under the HTML element it's attached to.
        /// </summary>
        [Parameter]
        public SideType? Opens { get; set; }

        /// <summary>
        /// Whether the picker appears below (default) or above the HTML element it's attached to.
        /// </summary>
        [Parameter]
        public DropsType? Drops { get; set; }

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, and may return true or false to indicate whether that date should be available for selection or not. 
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, bool> DaysEnabledFunction { get; set; }

        /// <summary>
        /// String of CSS class name to apply to calendar cell when <seealso cref="CustomDateFunction"/> returns true
        /// </summary>
        [Parameter]
        public string CustomDateClass { get; set; }

        /// <summary>
        /// A function to which each date from the calendars is passed before they are displayed, 
        /// may return a bool value indicates whether <seealso cref="CustomDateClass"/> will be added to the cell, 
        /// or a string with CSS class name to add to that date's calendar cell.
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, object> CustomDateFunction { get; set; }

        /// <summary>
        /// Triggered when the apply button is clicked, or when a predefined range is clicked
        /// </summary>
        [Parameter]
        public EventCallback<DateRange> OnRangeSelect { get; set; }

        /// <summary>An event that is invoked when the DatePicker is opened.</summary>
        [Parameter]
        public EventCallback OnOpened { get; set; }

        /// <summary>An event that is invoked when the DatePicker is closed.</summary>
        [Parameter]
        public EventCallback OnClosed { get; set; }

        /// <summary>An event that is invoked on backdrop click (false) or cancel button click (true).</summary>
        [Parameter]
        public EventCallback<bool> OnCancel { get; set; }

        /// <summary>An event that is invoked when left or right calendar's month changed.</summary>
        [Parameter]
        public EventCallback OnMonthChanged { get; set; }

        public CalendarType LeftCalendar { get; set; }
        public CalendarType RightCalendar { get; set; }

        internal DateTimeOffset? OldStartValue { get; set; }
        internal DateTimeOffset? OldEndValue { get; set; }
        internal string ChosenLabel { get; set; }
        internal bool CalendarsVisible { get; set; }

        protected override void OnInitialized()
        {
            var configs = ServiceProvider.GetServices<DateRangePickerConfig>();
            var config = configs?.FirstOrDefault();
            if (!string.IsNullOrEmpty(Config) && configs.Any(c => c.Name == Config))
            {
                config = configs.First(c => c.Name == Config);
            }

            if (config == null) config = new DateRangePickerConfig();
            config.CopyProperties(this);

            ConfigAttributes = config.Attributes;

            if (string.IsNullOrEmpty(DateFormat))
            {
                DateFormat = Culture.DateTimeFormat.ShortDatePattern;
            }

            if (SingleDatePicker == true) AutoApply = true;

            if (!FirstDayOfWeek.HasValue)
            {
                FirstDayOfWeek = Culture.DateTimeFormat.FirstDayOfWeek;
            }

            LeftCalendar = new CalendarType(FirstDayOfWeek.Value);
            RightCalendar = new CalendarType(FirstDayOfWeek.Value);

            StartDate = StartDate?.Date;
            EndDate = EndDate?.Date.AddDays(1).AddTicks(-1);

            AdjustCalendars();

            base.OnInitialized();
        }

        /// <summary>
        /// Show picker popup
        /// </summary>
        public void Open()
        {
            if (Visible) return;

            OldStartValue = StartDate;
            OldEndValue = EndDate;

            if (AutoAdjustCalendars == true) AdjustCalendars();

            var selectedRange = Ranges?.FirstOrDefault(r =>
                r.Value.Start.Date == StartDate?.Date &&
                r.Value.End.Date == EndDate?.Date);
            if (selectedRange != null)
            {
                ChosenLabel = selectedRange.Value.Key;
            }
            else
            {
                ChosenLabel = CustomRangeLabel;
                ShowCalendars();
            }

            Visible = true;

            JSRuntime.InvokeAsync<object>("clickAndPositionHandler.addClickOutsideEvent", Id, ParentId, DotNetObjectReference.Create(this));
            JSRuntime.InvokeAsync<object>("clickAndPositionHandler.getPickerPosition", Id, ParentId,
                Enum.GetName(typeof(DropsType), Drops).ToLower(), Enum.GetName(typeof(SideType), Opens).ToLower());

            OnOpened.InvokeAsync(null);

            StateHasChanged();
        }

        public void AdjustCalendars()
        {
            LeftCalendar.Month = StartDate ?? DateTime.Now;
            RightCalendar.Month = EndDate ?? DateTime.Now.AddMonths(1);
            if (LeftCalendar.Month.Year == RightCalendar.Month.Year
                && LeftCalendar.Month.Month == RightCalendar.Month.Month)
            {
                RightCalendar.Month = RightCalendar.Month.AddMonths(1);
            }
        }

        /// <summary>
        /// Toggle picker popup state
        /// </summary>
        public void Toggle()
        {
            if (Visible) Close();
            else Open();
        }

        /// <summary>
        /// Close picker popup
        /// </summary>
        public void Close()
        {
            Visible = false;
            StateHasChanged();
            OnClosed.InvokeAsync(null);
        }

        /// <summary>
        /// JSInvokable callback to handle outside click
        /// </summary>
        [JSInvokable]
        public virtual void InvokeClickOutside()
        {
            if (Visible && CloseOnOutsideClick == true)
            {
                StartDate = OldStartValue;
                EndDate = OldEndValue;
                Close();
                OnCancel.InvokeAsync(false);
            }
        }

        internal void ShowCalendars()
        {
            CalendarsVisible = true;
        }

        internal void HideCalendars()
        {
            CalendarsVisible = false;
        }
    }
}
