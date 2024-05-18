﻿using CommunityToolkit.Mvvm.Messaging;
using OwlCore.AbstractUI.Models;
using System.Windows;
using System.Windows.Controls;
using ZuneModCore;
using ZuneModCore.Services;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage : UserControl
    {
        private const string MOD_MANAGER_TITLE = "MOD MANAGER";

        private readonly IModCoreConfig _modConfig;

        public ModsPage(IModCoreConfig modConfig)
        {
            _modConfig = modConfig;

            InitializeComponent();
            ModList.ItemsSource = Mod.AvailableMods;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetModFromControl(sender, out var mod))
                return;

            ProgressDialogViewModel progDialog = new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Preparing to apply '{mod.Title}'...",
                ShowAffirmativeButton = false,
                IsIndeterminate = true,
                Maximum = 3,
            };
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(progDialog));

            mod.ZuneInstallDir = _modConfig.ZuneInstallDir;

            // Stage 0: Initialize mod
            await mod.Init();
            ++progDialog.Progress;

            // Stage 1: Display AbstractUI for options
            progDialog.Description = $"Awaiting options for '{mod.Title}'...";
            if (mod.OptionsUI != null)
            {
                var optionsDialog = new AbstractUIGroupDialog(mod.OptionsUI);
                optionsDialog.Title = optionsDialog.Title + " | " + mod.Title;
                bool? optionsResult = optionsDialog.ShowDialog();
                if (!(optionsResult.HasValue && optionsResult.Value))
                {
                    WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
                    return;
                }
                mod.OptionsUI = (AbstractUICollection)optionsDialog.ViewModel.Model;
            }
            ++progDialog.Progress;

            // Stage 2: Apply mod
            progDialog.Description = $"Applying '{mod.Title}'...";
            string applyResult = await mod.Apply();
            if (applyResult != null)
            {
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

                DialogViewModel errorDialog = new()
                {
                    Title = MOD_MANAGER_TITLE,
                    Description = $"Failed to apply '{mod.Title}'.\r\n{applyResult}",
                };
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(errorDialog));

                return;
            }
            ++progDialog.Progress;

            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Successfully applied '{mod.Title}'",
            }));
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetModFromControl(sender, out var mod))
                return;

            ProgressDialogViewModel progDialog = new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Preparing to reset '{mod.Title}'...",
                ShowAffirmativeButton = false,
                IsIndeterminate = true,
                Maximum = 2,
            };
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(progDialog));

            mod.ZuneInstallDir = _modConfig.ZuneInstallDir;

            await mod.Init();
            ++progDialog.Progress;

            // TODO: Implement AbstractUI display for reset options
            //if (mod.OptionsUI != null)
            //{
            //    var optionsDialog = new AbstractUIGroupDialog();
            //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUICollectionViewModel(mod.OptionsUI);
            //    optionsDialog.ShowDialog();
            //}

            progDialog.Description = $"Resetting '{mod.Title}'...";
            string resetResult = await mod.Reset();
            if (resetResult != null)
            {
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

                DialogViewModel errorDialog = new()
                {
                    Title = MOD_MANAGER_TITLE,
                    Description = $"Failed to reset '{mod.Title}'.\r\n{resetResult}",
                };
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(errorDialog));

                return;
            }

            ++progDialog.Progress;

            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Successfully reset '{mod.Title}'",
            }));
        }

        private static bool TryGetModFromControl(object sender, out Mod mod)
        {
            mod = (sender as FrameworkElement)?.DataContext as Mod;
            return mod is not null;
        }
    }
}
