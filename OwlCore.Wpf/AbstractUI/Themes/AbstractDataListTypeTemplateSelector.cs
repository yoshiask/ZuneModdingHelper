﻿using System;
using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.ViewModels;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Wpf.AbstractUI.Themes
{
    /// <summary>
    /// Selects the template that is used for an <see cref="AbstractDataList"/> based on the <see cref="AbstractDataList.PreferredDisplayMode"/>.
    /// </summary>
    public class AbstractDataListTypeTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// The data template used to display an <see cref="AbstractTextBox"/>.
        /// </summary>
        public DataTemplate? GridTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractDataList"/>.
        /// </summary>
        public DataTemplate? ListTemplate { get; set; }

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is AbstractDataListViewModel viewModel)
            {
                return viewModel.PreferredDisplayMode switch
                {
                    AbstractDataListPreferredDisplayMode.Grid => GridTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    AbstractDataListPreferredDisplayMode.List => ListTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    _ => throw new NotImplementedException(),
                };
            }

            return null!;
        }
    }
}