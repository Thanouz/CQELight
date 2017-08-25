﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQELight.MVVM.Interfaces
{
    /// <summary>
    /// Contract interface for waiting/loading panel display.
    /// </summary>
    public interface ILoadingPanelDisplayer
    {
        /// <summary>
        /// Show a loading panel on the view with specified message.
        /// </summary>
        /// <param name="waitMessage">Message.</param>
        Task ShowLoadingPanelAsync(string waitMessage);
        /// <summary>
        /// Hide the loading panel.
        /// </summary>
        Task HideLoadingPanelAsync();
    }
}
